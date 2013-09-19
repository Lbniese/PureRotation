using System.Linq;
using AdvancedAI.Helpers;
using CommonBehaviors.Actions;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;

namespace AdvancedAI.Class.Warrior.PvE
{
    public class FuryWarrior
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        private const int Enrage = 12880;

        [Behavior(BehaviorType.Combat, WoWClass.Warrior, WoWSpec.WarriorFury)]
        public static Composite FuryCombat()
        {
            return new PrioritySelector(
                Common.CreateInterruptBehavior(),
                HeroicLeap(),
                DemoBanner(),
                new Decorator(ret => Me.CurrentTarget != null && (!Me.CurrentTarget.IsWithinMeleeRange || Me.IsCasting || SpellManager.GlobalCooldown),
                    new ActionAlwaysSucceed()),
                new Decorator(ret => Me.HasAura("Dire Fixation"),
                    new PrioritySelector(
                        BossMechs.HorridonHeroic())),
                Spell.Cast("Shattering Throw", ret => Me.CurrentTarget.IsBoss() && PartyBuff.WeHaveBloodlust),
                Spell.Cast("Victory Rush", ret => Me.HealthPercent <= 90),
                Spell.Cast("Berserker Rage", ret => !Me.CachedHasAura(Enrage) && Me.CurrentTarget.CachedHasAura("Colossus Smash")),
                Spell.Cast("Colossus Smash", ret => Me.CurrentRage > 80 && Me.CachedHasAura("Raging Blow!") && Me.CachedHasAura(Enrage)),
                new Decorator(ret => Unit.UnfriendlyUnits(8).Count() > 2,
                    CreateAoe()),
                new Decorator(ret => Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent <= 20,
                    CreateExecuteRange()),
                new Decorator(ret => Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent > 20,
                    new PrioritySelector(
                        Item.UsePotionAndHealthstone(40),
                        new Decorator(ret => AdvancedAI.Burst && Me.CurrentTarget.IsBoss(),
                            new PrioritySelector(
                                Spell.Cast("Blood Fury"),
                                Spell.Cast("Recklessness"),
                                Spell.Cast("Avatar"),
                                Spell.Cast("Skull Banner"))),
                        Spell.Cast("Bloodbath"),
                        new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),
                        new Decorator(ret => !Me.CurrentTarget.CachedHasAura("Colossus Smash"),
                            new PrioritySelector(
                                Spell.Cast("Bloodthirst"),
                                Spell.Cast("Heroic Strike", ctx => Me.CurrentRage > 105 && ColossusSmashCheck()),
                                Spell.Cast("Raging Blow", ret => Me.CachedHasAura("Raging Blow!", 2) && ColossusSmashCheck()),
                                Spell.Cast("Wild Strike", ret => Me.CachedHasAura("Bloodsurge")),
                                Spell.Cast("Dragon Roar", ret => Me.CurrentTarget.Distance <= 8),
                                Spell.Cast("Raging Blow", ret => Me.CachedHasAura("Raging Blow!", 1) && ColossusSmashCheck()),
                                Spell.Cast("Battle Shout", ret => Me.RagePercent < 30 && Spell.GetSpellCooldown("Colossus Smash").TotalSeconds <= 2),
                                Spell.Cast("Shockwave"),
                                Spell.Cast("Wild Strike", ret => Me.CurrentRage >= 115 && ColossusSmashCheck()))),
                        new Decorator(ret => Me.CurrentTarget.CachedHasAura("Colossus Smash"),
                            new PrioritySelector(
                                Spell.Cast("Heroic Strike", ctx => Me.CurrentRage > 30),
                                Spell.Cast("Bloodthirst"),
                                Spell.Cast("Raging Blow"),
                                Spell.Cast("Wild Strike", ret => Me.CachedHasAura("Bloodsurge")))))));
        }

        [Behavior(BehaviorType.PreCombatBuffs, WoWClass.Warrior, WoWSpec.WarriorFury)]
        public static Composite FuryPreCombatBuffs()
        {
            return new PrioritySelector(
                //new Decorator(ret => AdvancedAI.PvPRot,
                //    FuryWarriorPvP.CreateFWPvPBuffs),
                Spell.Cast("Battle Shout", ret => !Me.HasPartyBuff(PartyBuffType.AttackPower)),
                FuryPull());

        }

        [Behavior(BehaviorType.Pull, WoWClass.Warrior, WoWSpec.WarriorFury, WoWContext.Instances | WoWContext.Normal)]
        public static Composite FuryPull()
        {
            return new PrioritySelector(
                Movement.CreateFaceTargetBehavior(70, false),
                Spell.CastOnGround("Heroic Leap", on => Me.CurrentTarget.Location, ret => SpellManager.Spells["Charge"].Cooldown),
                Spell.Cast("Charge"));
        }

        private static Composite CreateAoe()
        {
            return new PrioritySelector(
                new Decorator(ret => Unit.UnfriendlyUnits(8).Count() >= 5,
                    new PrioritySelector(
                        Spell.Cast("Whirlwind"),
                        Spell.Cast("Bloodthirst"),
                        Spell.Cast("Raging Blow"))),
                Spell.Cast("Whirlwind", ret => !Me.CachedHasAura("Meat Cleaver", (int)MathEx.Clamp(1, 3, Unit.UnfriendlyUnits(8).Count() - 1))),
                Spell.Cast("Bloodthirst"),
                Spell.Cast("Raging Blow", ret => Me.CachedHasAura("Meat Cleaver", (int)MathEx.Clamp(1, 3, Unit.UnfriendlyUnits(8).Count() - 1))),
                Spell.Cast("Cleave", ret => Me.CurrentRage >= 105 && Spell.GetSpellCooldown("Colossus Smash").TotalSeconds >= 3));
        }

        private static Composite CreateExecuteRange()
        {
            return new PrioritySelector(
                new Decorator(ret => !Me.CurrentTarget.CachedHasAura("Colossus Smash"),
                    new PrioritySelector(
                        Spell.Cast("Bloodthirst"),
                        Spell.Cast("Raging Blow"),
                        new Decorator(ret => Me.RagePercent < 85,
                            new Action(ret => RunStatus.Success)))),
                new Decorator(ret => Me.CurrentTarget.CachedHasAura("Colossus Smash"),
                    new PrioritySelector(
                        Spell.Cast("Execute"))));
        }

        private static Composite HeroicLeap()
        {
            return
                new Decorator(ret => SpellManager.CanCast("Heroic Leap") &&
                    Lua.GetReturnVal<bool>("return IsLeftControlKeyDown() and not GetCurrentKeyBoardFocus()", 0),
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
                    Lua.GetReturnVal<bool>("return IsLeftShiftKeyDown() and not GetCurrentKeyBoardFocus()", 0),
                    new Action(ret =>
                    {
                        SpellManager.Cast("Demoralizing Banner");
                        Lua.DoString("if SpellIsTargeting() then CameraOrSelectOrMoveStart() CameraOrSelectOrMoveStop() end");
                        return;
                    }));
        }

        private static bool ColossusSmashCheck()
        {
            return (Spell.GetSpellCooldown("Colossus Smash").TotalSeconds >= 3 ||
                    !SpellManager.Spells["Colossus Smash"].Cooldown);
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
