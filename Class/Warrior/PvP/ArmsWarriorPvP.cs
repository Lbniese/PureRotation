using AdvancedAI.Helpers;
using AdvancedAI;
using CommonBehaviors.Actions;
using Styx;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = Styx.TreeSharp.Action;

namespace AdvancedAI.Spec
{
    class ArmsWarriorPvP// : AdvancedAI
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        public static Composite CreateAWPvPCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Cast("Pummel",
                               ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast),
                    Spell.Cast("Pummel",
                               ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast),
                    Spell.Cast("Impending Victory", ret => Me.HealthPercent <= 90 && Me.HasAura("Victorious")),
                    Spell.Cast("Hamstring", ret => !Me.CurrentTarget.HasAuraWithMechanic(WoWSpellMechanic.Slowed)),
                    Spell.Cast("Die by the Sword", ret => Me.HealthPercent <= 20),
                    Spell.Cast("Recklessness",
                               ret => Me.CurrentTarget.IsBoss && Me.CurrentTarget.HasAuraExpired("Colossus Smash", 5)),
                    Spell.Cast("Bloodbath"),
                    Spell.Cast("Skull Banner", ret => Me.CurrentTarget.IsBoss && Me.HasAura("Recklessness")),
                    new Action(ret =>
                                   {
                                       Item.UseHands();
                                       return RunStatus.Failure;
                                   }),
                    //new Action(ret => { Item.UseTrinkets(); return RunStatus.Failure; }),
                    Spell.Cast("Berserker Rage", ret => !Me.ActiveAuras.ContainsKey("Enrage")),
                    Spell.Cast("Sweeping Strikes",
                               ret => Unit.NearbyUnfriendlyUnits.Count(u => u.IsWithinMeleeRange) >= 2),
                    Spell.Cast("Heroic Strike",
                               ret =>
                               (Me.CurrentTarget.HasMyAura("Colossus Smash") && Me.CurrentRage >= 70) ||
                               Me.CurrentRage >= 95),
                    Spell.Cast("Mortal Strike"),
                    Spell.Cast("Dragon Roar",
                               ret =>
                               !Me.CurrentTarget.HasMyAura("Colossus Smash") && Me.HasAura("Bloodbath") &&
                               Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8*8) >= 1),
                    Spell.Cast("Colossus Smash", ret => Me.HasAuraExpired("Colossus Smash", 1)),
                    Spell.Cast("Execute",
                               ret =>
                               Me.CurrentTarget.HasMyAura("Colossus Smash") || Me.HasAura("Recklessness") ||
                               Me.CurrentRage >= 85),
                    Spell.Cast("Dragon Roar",
                               ret =>
                               (!Me.CurrentTarget.HasMyAura("Colossus Smash") && Me.CurrentTarget.HealthPercent < 20) ||
                               (Me.HasAura("Bloodbath") && Me.CurrentTarget.HealthPercent >= 20) &&
                               Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8*8) >= 1),
                    Spell.Cast("Thunder Clap",
                               ret =>
                               Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8*8) >= 2 &&
                               Clusters.GetCluster(Me, Unit.NearbyUnfriendlyUnits, ClusterType.Radius, 8).Any(
                                   u => !u.HasMyAura("Deep Wounds"))),
                    Spell.Cast("Slam",
                               ret =>
                               Me.CurrentTarget.HasMyAura("Colossus Smash") &&
                               (Me.CurrentTarget.GetAuraTimeLeft("Colossus Smash").TotalSeconds <= 1 ||
                                Me.HasAura("Recklessness")) && Me.CurrentTarget.HealthPercent >= 20),
                    Spell.Cast("Overpower",
                               ret =>
                               Me.HasAura("Taste for Blood") && Me.Auras["Taste for Blood"].StackCount >= 3 &&
                               Me.CurrentTarget.HealthPercent >= 20),
                    Spell.Cast("Slam",
                               ret =>
                               Me.CurrentTarget.HasAura("Colossus Smash") &&
                               Me.CurrentTarget.GetAuraTimeLeft("Colossus Smash").TotalSeconds <= 2.5 &&
                               Me.CurrentTarget.HealthPercent >= 20),
                    Spell.Cast("Execute", ret => !Me.HasAura("Sudden Execute")),
                    Spell.Cast("Overpower", ret => Me.CurrentTarget.HealthPercent >= 20 || Me.HasAura("Sudden Execute")),
                    Spell.Cast("Slam", ret => Me.CurrentRage >= 40 && Me.CurrentTarget.HealthPercent >= 20),
                    Spell.Cast("Battle Shout"),
                    Spell.Cast("Heroic Throw"),
                    Spell.Cast("Impending Victory", ret => Me.CurrentTarget.HealthPercent > 20 || Me.HealthPercent < 50),
                    new ActionAlwaysSucceed());
            }
        }

        public static Composite CreateAWPvPBuffs
        {
            get
            {
                return new PrioritySelector(

                    );
            }
        }
    }
}
