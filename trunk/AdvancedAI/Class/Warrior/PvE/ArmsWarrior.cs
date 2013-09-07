using System.Linq;
using AdvancedAI.Helpers;
using CommonBehaviors.Actions;
using Styx;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals.WoWObjects;

namespace AdvancedAI.Class.Warrior.PvE
{
    class ArmsWarrior
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        private const int Enrage = 12880;

        [Behavior(BehaviorType.Combat, WoWClass.Warrior, WoWSpec.WarriorArms, WoWContext.Instances | WoWContext.Normal)]
        public static Composite ArmsCombat()
        {
            return new PrioritySelector(
                new Decorator(ret => Me.CurrentTarget != null && (!Me.CurrentTarget.IsWithinMeleeRange || Me.IsCasting || SpellManager.GlobalCooldown),
                    new ActionAlwaysSucceed()),
                new Decorator(ret => Me.CachedHasAura("Dire Fixation"),
                            new PrioritySelector(
                                BossMechs.HorridonHeroic())),
                Common.CreateInterruptBehavior(),
                Spell.Cast("Victory Rush", ret => Me.HealthPercent <= 90 && Me.CachedHasAura("Victorious")),
                Spell.Cast("Die by the Sword", ret => Me.HealthPercent <= 20),
                Item.UsePotionAndHealthstone(50),
                new Decorator(ret => Unit.UnfriendlyUnits(8).Count() >= 4,
                            CreateAoe()),
                new Decorator(ret => AdvancedAI.Burst && Me.CurrentTarget.IsBoss() && Me.CurrentTarget.CachedHasAura("Colossus Smash"),
                    new PrioritySelector(
                        Spell.Cast("Recklessness"),
                        Spell.Cast("Avatar"),
                        Spell.Cast("Skull Banner"))),
                Spell.Cast("Bloodbath"),
                new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),
                Spell.Cast("Berserker Rage", ret => !Me.CachedHasAura(Enrage)),
                Spell.Cast("Sweeping Strikes", ret => Unit.UnfriendlyUnits(8).Count() >= 2),
                Spell.Cast("Heroic Strike", ret => (Me.CurrentTarget.CachedHasAura("Colossus Smash") && Me.CurrentRage >= 80 && Me.CurrentTarget.HealthPercent >= 20) || Me.CurrentRage >= 105),
                Spell.Cast("Mortal Strike"),
                Spell.Cast("Dragon Roar", ret => !Me.CurrentTarget.CachedHasAura("Colossus Smash") && Me.CachedHasAura("Bloodbath") && Me.CurrentTarget.Distance <= 8),
                Spell.Cast("Colossus Smash", ret => Me.CachedHasAuraDown("Colossus Smash", 1, true, 1)),
                Spell.Cast("Execute", ret => Me.CurrentTarget.CachedHasAura("Colossus Smash") || Me.CachedHasAura("Recklessness") || Me.CurrentRage >= 95),
                Spell.Cast("Dragon Roar", ret => (!Me.CurrentTarget.HasMyAura("Colossus Smash") && Me.CurrentTarget.HealthPercent < 20) || (Me.HasAura("Bloodbath") && Me.CurrentTarget.HealthPercent >= 20) && Me.CurrentTarget.Distance <= 8),
                Spell.Cast("Thunder Clap", ret => Unit.UnfriendlyUnits(8).Count() >= 3 && Clusters.GetCluster(Me, Unit.UnfriendlyUnits(8), ClusterType.Radius, 8).Any(u => !u.CachedHasAura("Deep Wounds", 1, true))),
                Spell.Cast("Slam", ret => Me.CurrentTarget.CachedHasAura("Colossus Smash") && (Me.CurrentTarget.CachedGetAuraTimeLeft("Colossus Smash") <= 1 || Me.CachedHasAura("Recklessness")) && Me.CurrentTarget.HealthPercent >= 20),
                Spell.Cast("Overpower", ret => Me.CachedHasAura("Taste for Blood", 3) && Me.CurrentTarget.HealthPercent >= 20),
                Spell.Cast("Slam", ret => Me.CurrentTarget.CachedHasAura("Colossus Smash") && Me.CurrentTarget.CachedGetAuraTimeLeft("Colossus Smash") <= 2.5 && Me.CurrentTarget.HealthPercent >= 20),
                Spell.Cast("Execute", ret => !Me.CachedHasAura("Sudden Execute")),
                Spell.Cast("Overpower", ret => Me.CurrentTarget.HealthPercent >= 20 || Me.CachedHasAura("Sudden Execute")),
                Spell.Cast("Slam", ret => Me.CurrentRage >= 40 && Me.CurrentTarget.HealthPercent >= 20),
                Spell.Cast("Battle Shout"),
                Spell.Cast("Heroic Throw"),
                Spell.Cast("Impending Victory", ret => Me.HealthPercent < 50));
        }

        [Behavior(BehaviorType.PreCombatBuffs, WoWClass.Warrior, WoWSpec.WarriorArms,WoWContext.Instances | WoWContext.Normal)]
        public static Composite ArmsBuffs()
        {
            return new PrioritySelector(
                Spell.Cast("Battle Shout", ret => !Me.HasPartyBuff(PartyBuffType.AttackPower)));
        }

        [Behavior(BehaviorType.Pull, WoWClass.Warrior, WoWSpec.WarriorArms, WoWContext.Instances | WoWContext.Normal)]
        public static Composite ArmsPull()
        {
            return new PrioritySelector(
                Spell.Cast("Charge"));
        }

        private static Composite CreateAoe()
        {
            return new PrioritySelector(
                new Decorator(ret => AdvancedAI.Burst && Me.CurrentTarget.IsBoss() && Me.CurrentTarget.CachedHasAura("Colossus Smash"),
                    new PrioritySelector(
                        Spell.Cast("Recklessness"),
                        Spell.Cast("Avatar"),
                        Spell.Cast("Skull Banner"))),
                Spell.Cast("Bloodbath"),
                new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),
                Spell.Cast("Berserker Rage", ret => !Me.CachedHasAura(Enrage)),
                Spell.Cast("Sweeping Strikes"),
                Spell.Cast("Bladestorm"),
                Spell.Cast("Whirlwind", ret => (Me.CurrentTarget.CachedHasAura("Colossus Smash") && Me.CurrentRage >= 80 && Me.CurrentTarget.HealthPercent >= 20) || Me.CurrentRage >= 105),
                Spell.Cast("Mortal Strike"),
                Spell.Cast("Dragon Roar", ret => !Me.CurrentTarget.CachedHasAura("Colossus Smash") && Me.CachedHasAura("Bloodbath") && Me.CurrentTarget.Distance <= 8),
                Spell.Cast("Colossus Smash", ret => Me.CachedHasAuraDown("Colossus Smash", 1, true, 1)),
                Spell.Cast("Execute", ret => Me.CurrentTarget.CachedHasAura("Colossus Smash") || Me.CachedHasAura("Recklessness") || Me.CurrentRage >= 95),
                Spell.Cast("Dragon Roar", ret => (!Me.CurrentTarget.HasMyAura("Colossus Smash") && Me.CurrentTarget.HealthPercent < 20) || (Me.HasAura("Bloodbath") && Me.CurrentTarget.HealthPercent >= 20) && Me.CurrentTarget.Distance <= 8),
                Spell.Cast("Thunder Clap", ret => Unit.UnfriendlyUnits(8).Count() >= 3 && Clusters.GetCluster(Me, Unit.UnfriendlyUnits(8), ClusterType.Radius, 8).Any(u => !u.CachedHasAura("Deep Wounds", 1, true))),
                Spell.Cast("Slam", ret => Me.CurrentTarget.CachedHasAura("Colossus Smash") && (Me.CurrentTarget.CachedGetAuraTimeLeft("Colossus Smash") <= 1 || Me.CachedHasAura("Recklessness")) && Me.CurrentTarget.HealthPercent >= 20),
                Spell.Cast("Overpower", ret => Me.CachedHasAura("Taste for Blood", 3) && Me.CurrentTarget.HealthPercent >= 20),
                Spell.Cast("Slam", ret => Me.CurrentTarget.CachedHasAura("Colossus Smash") && Me.CurrentTarget.CachedGetAuraTimeLeft("Colossus Smash") <= 2.5 && Me.CurrentTarget.HealthPercent >= 20),
                Spell.Cast("Execute", ret => !Me.CachedHasAura("Sudden Execute")),
                Spell.Cast("Overpower", ret => Me.CurrentTarget.HealthPercent >= 20 || Me.CachedHasAura("Sudden Execute")),
                Spell.Cast("Whirlwind", ret => Me.CurrentRage >= 40 && Me.CurrentTarget.HealthPercent >= 20),
                Spell.Cast("Battle Shout"),
                Spell.Cast("Heroic Throw"),
                Spell.Cast("Impending Victory", ret => Me.HealthPercent < 50));
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
