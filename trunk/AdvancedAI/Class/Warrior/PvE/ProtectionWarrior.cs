using System.Linq;
using System.Windows.Forms;
using AdvancedAI.Helpers;
using AdvancedAI.Managers;
using CommonBehaviors.Actions;
using Styx;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;

namespace AdvancedAI.Class.Warrior.PvE
{
    class ProtectionWarrior
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        private const int Enrage = 12880;

        [Behavior(BehaviorType.Combat, WoWClass.Warrior, WoWSpec.WarriorProtection)]
        public static Composite ProtCombat()
        {
            return new PrioritySelector(
                    new Decorator(ret => Me.CurrentTarget != null && (!Me.CurrentTarget.IsWithinMeleeRange || Me.IsCasting || SpellManager.GlobalCooldown),
                    new ActionAlwaysSucceed()),
                    new Decorator(ret => Me.HasAura("Dire Fixation"),
                        new PrioritySelector(
                            BossMechs.HorridonHeroic())),
                    new Throttle(1, 1,
                        new PrioritySelector(
                            Common.CreateInterruptBehavior())),
                    new Decorator(ret => AdvancedAI.Burst && Me.CurrentTarget.IsWithinMeleeRange,
                        new PrioritySelector(
                            Spell.Cast("Recklessness"),
                            Spell.Cast("Bloodbath"),
                            new Decorator(ret => Me.HasAura("Recklessness"),
                                new PrioritySelector(
                                    Spell.Cast("Avatar"),
                                    Spell.Cast("Skull Banner"))))),

                    Item.UsePotionAndHealthstone(40),
                    new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),

                    //CD's all bout living
                    Spell.Cast("Victory Rush", ret => Me.HealthPercent <= 90 && Me.HasAura("Victorious")),
                    Spell.Cast("Impending Victory", ret => Me.HealthPercent <= 85),
                    Spell.Cast("Berserker Rage", ret => !Me.HasAura(Enrage)),
                    Spell.Cast("Enraged Regeneration", ret => (Me.HealthPercent <= 80 && Me.HasAura(Enrage) ||
                                                              Me.HealthPercent <= 50 && Spell.GetSpellCooldown("Berserker Rage").TotalSeconds > 10)
                                                              && TalentManager.IsSelected((int)WarriorTalents.EnragedRegeneration)),
                    Spell.Cast("Last Stand", ret => Me.HealthPercent <= 15 && !Me.HasAura("Shield Wall")),
                    Spell.Cast("Shield Wall", ret => Me.HealthPercent <= 30 && !Me.HasAura("Last Stand")),

                    //Might need some testing
                    new Throttle(1, 1,
                        new PrioritySelector(
                    Spell.Cast("Rallying Cry", ret => HealerManager.GetCountWithHealth(55) > 6),
                    Spell.Cast("Demoralizing Shout", ret => Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 10 * 10) >= 1 && IsCurrentTank()))),

                    Spell.Cast("Shield Block", ret => !Me.HasAura("Shield Block") && IsCurrentTank() && AdvancedAI.Weave),
                    Spell.Cast("Shield Barrier", ret => Me.CurrentRage > 60 && !Me.HasAura("Shield Barrier") && IsCurrentTank() && !AdvancedAI.Weave),
                    Spell.Cast("Shield Barrier", ret => Me.CurrentRage > 30 && Me.HasAura("Shield Block") && Me.HealthPercent <= 70),

                    Spell.Cast("Shattering Throw", ret => Me.CurrentTarget.IsBoss && PartyBuff.WeHaveBloodlust && !Me.IsMoving),

                    Spell.Cast("Shield Slam"),
                    Spell.Cast("Revenge", ret => Me.CurrentRage < 90),
                    Spell.Cast("Storm Bolt"),
                    Spell.Cast("Dragon Roar", ret => Me.CurrentTarget.Distance <= 8),
                    Spell.Cast("Execute"),
                    Spell.Cast("Thunder Clap", ret => !Me.CurrentTarget.HasAura("Weakened Blows") && Me.CurrentTarget.Distance <= 8),

                    new Decorator(ret => Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8 * 8) >= 2,
                        CreateAoe()),

                    DemoBanner(),
                    HeroicLeap(),
                    MockingBanner(),

                    Spell.Cast("Commanding Shout", ret => Me.HasPartyBuff(PartyBuffType.AttackPower)),
                    Spell.Cast("Battle Shout"),
                    Spell.Cast("Heroic Strike", ret => Me.CurrentRage > 85 || Me.HasAura(122510) || Me.HasAura(122016) || (!IsCurrentTank() && Me.CurrentRage > 60 && Me.CurrentTarget.IsBoss)),
                    Spell.Cast("Heroic Throw", ret => Me.CurrentTarget.Distance >= 10),
                    Spell.Cast("Devastate"));
        }

        [Behavior(BehaviorType.PreCombatBuffs, WoWClass.Warrior, WoWSpec.WarriorProtection)]
        internal static Composite ProtPreCombatBuffs()
        {
            return new PrioritySelector(
            //    new Decorator(ret => AdvancedAI.PvPRot,
            //        ProtectionWarriorPvP.CreatePWPvPBuffs)
            );

        }

        private static Composite CreateAoe()
        {
            return new PrioritySelector(
                Spell.Cast("Shockwave", ret => Clusters.GetClusterCount(Me, Unit.NearbyUnfriendlyUnits, ClusterType.Cone, 9) >= 3),
                Spell.Cast("Bladestorm", ret => Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8 * 8) >= 2),
                Spell.Cast("Thunder Clap"),
                Spell.Cast("Cleave", ret => (Me.CurrentRage > 85 || Me.HasAura(122510) || Me.HasAura(122016)) && Clusters.GetClusterCount(Me, Unit.NearbyUnfriendlyUnits, ClusterType.Cone, 5) >= 2),
                Spell.Cast("Thunder Clap", ret => Me.CurrentTarget.HasAura("Weakened Blows"))
                );
        }

        private static Composite HeroicLeap()
        {
            return
                new Decorator(ret => SpellManager.CanCast("Heroic Leap") &&
                    Lua.GetReturnVal<bool>("return IsLeftAltKeyDown() and not GetCurrentKeyBoardFocus()", 0),
                    new Action(ret =>
                    {
                        SpellManager.Cast("Heroic Leap");
                        Lua.DoString("if SpellIsTargeting() then CameraOrSelectOrMoveStart() CameraOrSelectOrMoveStop() end");
                        return;
                    }));
        }

        private static Composite DemoBanner()
        {
            return
                new Decorator(ret => SpellManager.CanCast("Demoralizing Banner") &&
                    KeyboardPolling.IsKeyDown(Keys.Z),
                    new Action(ret =>
                    {
                        SpellManager.Cast("Demoralizing Banner");
                        Lua.DoString("if SpellIsTargeting() then CameraOrSelectOrMoveStart() CameraOrSelectOrMoveStop() end");
                        return;
                    }));
        }

        private static Composite MockingBanner()
        {
            return
                new Decorator(ret => SpellManager.CanCast("Mocking Banner") &&
                    KeyboardPolling.IsKeyDown(Keys.C),
                    new Action(ret =>
                    {
                        SpellManager.Cast("Mocking Banner");
                        Lua.DoString("if SpellIsTargeting() then CameraOrSelectOrMoveStart() CameraOrSelectOrMoveStop() end");
                        return;
                    }));
        }

        private static bool NeedZerker()
        {

            return (!Me.HasAura(Enrage) && !TalentManager.IsSelected((int)WarriorTalents.EnragedRegeneration) ||
                   (TalentManager.IsSelected((int)WarriorTalents.EnragedRegeneration) && !Me.HasAura(Enrage) &&
                    Me.HealthPercent <= 80 && !SpellManager.Spells["Enraged Regeneration"].Cooldown ||
                    Spell.GetSpellCooldown("Enraged Regeneration").TotalSeconds > 30 && SpellManager.Spells["Enraged Regeneration"].Cooldown));
        }

        static bool IsCurrentTank()
        {
            return StyxWoW.Me.CurrentTarget.CurrentTargetGuid == StyxWoW.Me.Guid;
        }

        #region WarriorTalents
        public enum WarriorTalents
        {
            None = 0,
            Juggernaut,
            DoubleTime,
            Warbringer,
            EnragedRegeneration,
            SecondWind,
            ImpendingVictory,
            StaggeringShout,
            PiercingHowl,
            DisruptingShout,
            Bladestorm,
            Shockwave,
            DragonRoar,
            MassSpellReflection,
            Safeguard,
            Vigilance,
            Avatar,
            Bloodbath,
            StormBolt
        }
        #endregion
    }
}
