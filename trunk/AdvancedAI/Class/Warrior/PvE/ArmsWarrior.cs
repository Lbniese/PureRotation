using Styx;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using AdvancedAI.Helpers;
using System.Linq;
using Action = Styx.TreeSharp.Action;

namespace AdvancedAI.Spec
{
    class ArmsWarrior
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        public static Composite CreateAWBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        ArmsWarriorPvP.CreateAWPvPBuffs),
                    Spell.Cast("Battle Shout", ret => !Me.HasAura("Battle Shout")));
            }
        }

        public static Composite CreateAWCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        ArmsWarriorPvP.CreateAWPvPCombat),
                    //new Throttle(1, 1,
                    //    new PrioritySelector(
                    //        Spell.Cast("Throw", on => PinkDino))),
                    Spell.Cast("Pummel", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast),
                    Spell.Cast("Impending Victory", ret => Me.HealthPercent <= 90 && Me.HasAura("Victorious")),
                    Spell.Cast("Die by the Sword", ret => Me.HealthPercent <= 20),
                    Item.CreateUsePotionAndHealthstone(50, 0),
                    new Decorator(ret => Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8 * 8) >= 4,
                        CreateAoe()),
                    new Decorator(ret => AdvancedAI.Burst,
                        new PrioritySelector(
                        Spell.Cast("Recklessness", ret => Me.CurrentTarget.IsBoss && Me.CurrentTarget.HasAuraExpired("Colossus Smash", 5)),
                        Spell.Cast("Bloodbath"),
                        Spell.Cast("Skull Banner", ret => Me.CurrentTarget.IsBoss && Me.HasAura("Recklessness")),
                        new Action(ret => { Item.UseHands(); return RunStatus.Failure; }))),
                    //new Action(ret => { Item.UseTrinkets(); return RunStatus.Failure; }),
                    Spell.Cast("Berserker Rage", ret => !Me.ActiveAuras.ContainsKey("Enrage")),
                    Spell.Cast("Sweeping Strikes", ret => Unit.NearbyUnfriendlyUnits.Count(u => u.IsWithinMeleeRange) >= 2),
                    Spell.Cast("Heroic Strike", ret => (Me.CurrentTarget.HasMyAura("Colossus Smash") && Me.CurrentRage >= 80 && Me.CurrentTarget.HealthPercent >= 20) || Me.CurrentRage >= 105),
                    Spell.Cast("Mortal Strike"),
                    Spell.Cast("Dragon Roar", ret => !Me.CurrentTarget.HasMyAura("Colossus Smash") && Me.HasAura("Bloodbath") && Me.CurrentTarget.Distance <= 8),
                    Spell.Cast("Colossus Smash", ret => Me.HasAuraExpired("Colossus Smash", 1)),
                    Spell.Cast("Execute", ret => Me.CurrentTarget.HasMyAura("Colossus Smash") || Me.HasAura("Recklessness") || Me.CurrentRage >= 95),
                    Spell.Cast("Dragon Roar", ret => (!Me.CurrentTarget.HasMyAura("Colossus Smash") && Me.CurrentTarget.HealthPercent < 20) || (Me.HasAura("Bloodbath") && Me.CurrentTarget.HealthPercent >= 20) && Me.CurrentTarget.Distance <= 8),
                    Spell.Cast("Thunder Clap", ret => Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8 * 8) >= 3 && Clusters.GetCluster(Me, Unit.NearbyUnfriendlyUnits, ClusterType.Radius, 8).Any(u => !u.HasMyAura("Deep Wounds"))),
                    Spell.Cast("Slam", ret => Me.CurrentTarget.HasMyAura("Colossus Smash") && (Me.CurrentTarget.GetAuraTimeLeft("Colossus Smash").TotalSeconds <= 1 || Me.HasAura("Recklessness")) && Me.CurrentTarget.HealthPercent >= 20),
                    Spell.Cast("Overpower", ret => Me.HasAura("Taste for Blood") && Me.Auras["Taste for Blood"].StackCount >= 3 && Me.CurrentTarget.HealthPercent >= 20),
                    Spell.Cast("Slam", ret => Me.CurrentTarget.HasAura("Colossus Smash") && Me.CurrentTarget.GetAuraTimeLeft("Colossus Smash").TotalSeconds <= 2.5 && Me.CurrentTarget.HealthPercent >= 20),
                    Spell.Cast("Execute", ret => !Me.HasAura("Sudden Execute")),
                    Spell.Cast("Overpower", ret => Me.CurrentTarget.HealthPercent >= 20 || Me.HasAura("Sudden Execute")),
                    Spell.Cast("Slam", ret => Me.CurrentRage >= 40 && Me.CurrentTarget.HealthPercent >= 20),
                    Spell.Cast("Battle Shout"),
                    Spell.Cast("Heroic Throw"),
                    Spell.Cast("Impending Victory", ret => Me.HealthPercent < 50));
            }
        }

        private static Composite CreateAoe()
        {
            return new PrioritySelector(
                new Decorator(ret => AdvancedAI.Burst,
                    new PrioritySelector(
                        Spell.Cast("Recklessness", ret => Me.CurrentTarget.IsBoss && Me.CurrentTarget.HasAuraExpired("Colossus Smash", 5)),
                        Spell.Cast("Bloodbath"),
                        Spell.Cast("Skull Banner", ret => Me.CurrentTarget.IsBoss && Me.HasAura("Recklessness")),
                        new Action(ret => { Item.UseHands(); return RunStatus.Failure; }))),
                Spell.Cast("Berserker Rage", ret => !Me.ActiveAuras.ContainsKey("Enrage")),
                Spell.Cast("Sweeping Strikes"),
                Spell.Cast("Bladestorm"),
                Spell.Cast("Whirlwind", ret => (Me.CurrentTarget.HasMyAura("Colossus Smash") && Me.CurrentRage >= 80 && Me.CurrentTarget.HealthPercent >= 20) || Me.CurrentRage >= 105),
                Spell.Cast("Mortal Strike"),
                Spell.Cast("Dragon Roar", ret => !Me.CurrentTarget.HasMyAura("Colossus Smash") && Me.HasAura("Bloodbath") && Me.CurrentTarget.Distance <= 8),
                Spell.Cast("Colossus Smash", ret => Me.HasAuraExpired("Colossus Smash", 1)),
                Spell.Cast("Execute", ret => Me.CurrentTarget.HasMyAura("Colossus Smash") || Me.HasAura("Recklessness") || Me.CurrentRage >= 95),
                Spell.Cast("Dragon Roar", ret => (!Me.CurrentTarget.HasMyAura("Colossus Smash") && Me.CurrentTarget.HealthPercent < 20) || (Me.HasAura("Bloodbath") && Me.CurrentTarget.HealthPercent >= 20) && Me.CurrentTarget.Distance <= 8),
                Spell.Cast("Thunder Clap", ret => Clusters.GetCluster(Me, Unit.NearbyUnfriendlyUnits, ClusterType.Radius, 8).Any(u => !u.HasMyAura("Deep Wounds"))),
                Spell.Cast("Slam", ret => Me.CurrentTarget.HasMyAura("Colossus Smash") && (Me.CurrentTarget.GetAuraTimeLeft("Colossus Smash").TotalSeconds <= 1 || Me.HasAura("Recklessness")) && Me.CurrentTarget.HealthPercent >= 20),
                Spell.Cast("Overpower", ret => Me.HasAura("Taste for Blood") && Me.Auras["Taste for Blood"].StackCount >= 3 && Me.CurrentTarget.HealthPercent >= 20),
                Spell.Cast("Slam", ret => Me.CurrentTarget.HasAura("Colossus Smash") && Me.CurrentTarget.GetAuraTimeLeft("Colossus Smash").TotalSeconds <= 2.5 && Me.CurrentTarget.HealthPercent >= 20),
                Spell.Cast("Execute", ret => !Me.HasAura("Sudden Execute")),
                Spell.Cast("Overpower", ret => Me.CurrentTarget.HealthPercent >= 20 || Me.HasAura("Sudden Execute")),
                Spell.Cast("Whirlwind", ret => Me.CurrentRage >= 40 && Me.CurrentTarget.HealthPercent >= 20),
                Spell.Cast("Battle Shout"),
                Spell.Cast("Heroic Throw"),
                Spell.Cast("Impending Victory", ret => Me.HealthPercent < 50));
        }

        #region Horridon Mechanics
        public static WoWUnit PinkDino
        {
            get
            {
                var direhornspirit = (from unit in ObjectManager.GetObjectsOfType<WoWUnit>(false)
                                where unit.IsAlive
                                where unit.InLineOfSight
                                where unit.Distance < 30
                                //where unit.Name == "Training Dummy"
                                where unit.Name == "Direhorn Spirit"
                                select unit).FirstOrDefault();
                return direhornspirit;
            }
        }
        #endregion

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
