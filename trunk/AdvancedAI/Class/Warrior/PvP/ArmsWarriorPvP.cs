using AdvancedAI.Helpers;
using AdvancedAI;
using CommonBehaviors.Actions;
using Styx;
using Styx.Common;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = Styx.TreeSharp.Action;
using AdvancedAI.Managers;
using Styx.CommonBot;

namespace AdvancedAI.Spec
{
    class ArmsWarriorPvP
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        #region Disarm
        public static string[] Disarm = new[] { //Pally
                                                 "Holy Avenger", "Avenging Wrath",
                                                 //Warrior need to make so it want disarm a warr if it has die by the sword buff
                                                 "Avatar", "Recklessness",
                                                 //Rogue
                                                 "Shadow Dance", "Shadow Blades",
                                                 //Kitty
                                                 "Berserk", "Incarnation", "Nature's Vigil",
                                                 //Hunter
                                                 "Rapid Fire","Bestial Wrath",
                                                 //DK
                                                 "Unholy Frenzy", "Pillar of Frost" };
        #endregion
        #region DontDisarm
        public static string[] DontDisarm = new[] { //Warrior 
                                                    "Die by the Sword", 
                                                    // Rogue
                                                    "Evasion", 
                                                    // Hunter
                                                    "Deterrence" };
        #endregion

        public static Composite CreateAWPvPCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.Movement,
                        Movement.CreateFaceTargetBehavior(70f, false)),
                    CreateChargeBehavior(),
                    Spell.Cast("Rallying Cry", ret => Me.HealthPercent <= 30),
                    new Throttle(
                        new PrioritySelector(
                            CreateInterruptSpellCast(on => BestInterrupt))),
                    Spell.Cast("Impending Victory", ret => Me.HealthPercent <= 90 && Me.HasAura("Victorious")),
                    ShatterBubbles(),
                    Spell.Cast("Piercing Howl", ret => !Me.CurrentTarget.IsStunned() && !Me.CurrentTarget.IsCrowdControlled() && !Me.CurrentTarget.HasAuraWithEffectsing(WoWApplyAuraType.ModDecreaseSpeed) && !Me.CurrentTarget.HasAnyAura("Piercing Howl", "Hamsting")),
                    Spell.Cast("Hamstring", ret => !Me.CurrentTarget.IsStunned() && !Me.CurrentTarget.IsCrowdControlled() && !Me.CurrentTarget.HasAuraWithEffectsing(WoWApplyAuraType.ModDecreaseSpeed) && !Me.CurrentTarget.HasAnyAura("Piercing Howl", "Hamsting")),
                    //DemoBanner(),
                    //MockingBanner(),
                    //Spell.Cast("Intervene", on => BestBanner),
                    Spell.CastOnGround("Demoralizing Banner", on => Me.Location, ret => Me.HealthPercent < 40),
                    Spell.Cast("Disarm", ret => Me.CurrentTarget.HasAnyAura(Disarm) && !Me.CurrentTarget.HasAnyAura(DontDisarm)),
                    Spell.Cast("Die by the Sword", ret => Me.HealthPercent <= 20 && Me.CurrentTarget.IsMelee()),
                    new Decorator(ret => AdvancedAI.Burst,
                        new PrioritySelector(
                    Spell.Cast("Recklessness", ret => Me.CurrentTarget.IsWithinMeleeRange),
                    Spell.Cast("Bloodbath", ret => Me.CurrentTarget.IsWithinMeleeRange),
                    Spell.Cast("Avatar", ret => Me.HasAura("Recklessness") && Me.CurrentTarget.IsWithinMeleeRange),
                    Spell.Cast("Skull Banner", ret => Me.HasAura("Recklessness") && Me.CurrentTarget.IsWithinMeleeRange))),
                    new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),
                    //new Action(ret => { Item.UseTrinkets(); return RunStatus.Failure; }),
                    //Spell.Cast("Berserker Rage", ret => !Me.ActiveAuras.ContainsKey("Enrage")),
                    //Spell.Cast("Sweeping Strikes",
                    //           ret => Unit.NearbyUnfriendlyUnits.Count(u => u.IsWithinMeleeRange) >= 2),
                    Spell.Cast("Intervene", on => BestInterveneTarget),
                    Spell.Cast("Charge", on => ChargeInt),
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
                    //Spell.Cast("Thunder Clap",
                    //           ret =>
                    //           Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8*8) >= 2 &&
                    //           Clusters.GetCluster(Me, Unit.NearbyUnfriendlyUnits, ClusterType.Radius, 8).Any(
                    //               u => !u.HasMyAura("Deep Wounds"))),
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
                    Spell.Cast("Slam", ret => Me.CurrentRage >= 50 && Me.CurrentTarget.HealthPercent >= 20),
                    Spell.Cast("Battle Shout"),
                    Spell.Cast("Heroic Throw"),
                    Spell.Cast("Impending Victory", ret => Me.CurrentTarget.HealthPercent > 20 || Me.HealthPercent < 50),
                    new Decorator(ret => AdvancedAI.Movement,
                        Movement.CreateMoveToMeleeBehavior(true)),
                    new ActionAlwaysSucceed());
            }
        }

        public static Composite CreateAWPvPBuffs
        {
            get
            {
                return new PrioritySelector(
                    Spell.BuffSelf("Battle Shout"),
                    new ActionAlwaysSucceed());
            }
        }

        #region Best Banner
        public static WoWUnit BestBanner//WoWUnit
        {
            get
            {
                if (!StyxWoW.Me.GroupInfo.IsInParty)
                    return null;
                if (StyxWoW.Me.GroupInfo.IsInParty)
                {
                    var closePlayer = FriendlyUnitsNearTarget(6f).OrderBy(t => t.DistanceSqr).FirstOrDefault(t => t.IsAlive);
                    if (closePlayer != null)
                        return closePlayer;
                    var bestBan = (from unit in ObjectManager.GetObjectsOfType<WoWUnit>(false)
                                   //where (unit.Equals(59390) || unit.Equals(59398))
                                   //where unit.Guid.Equals(59390) || unit.Guid.Equals(59398)
                                   where unit.Entry.Equals(59390) || unit.Entry.Equals(59398)
                                   //where (unit.Guid == 59390 || unit.Guid == 59398) 
                                   where unit.InLineOfSight
                                   select unit).FirstOrDefault();
                    return bestBan;
                }
                return null;
            }
        }
        #endregion

        #region BestInterrupt
        public static WoWUnit BestInterrupt
        {
            get
            {
                var bestInt = (from unit in ObjectManager.GetObjectsOfType<WoWPlayer>(false)
                                where unit.IsAlive
                                where unit.IsPlayer
                                where !unit.IsInMyPartyOrRaid
                                where unit.InLineOfSight
                                where unit.Distance <= 10
                                where unit.IsCasting
                                where unit.CanInterruptCurrentSpellCast
                                where Interuptdelay(unit)
                                select unit).FirstOrDefault();
                return bestInt;
            }
        }

        public static bool Interuptdelay(WoWUnit inttar)
        {
            return (inttar.CurrentCastTimeLeft.TotalSeconds / inttar.CastingSpell.CastTime) < MathEx.Random(10, 70);
        }
        #endregion

        #region Best Intervene
        public static WoWUnit BestInterveneTarget
        {
            get
            {
                if (!StyxWoW.Me.GroupInfo.IsInParty)
                    return null;
                if (StyxWoW.Me.GroupInfo.IsInParty)
                {
                    var bestTank = Group.Tanks.OrderBy(t => t.DistanceSqr).FirstOrDefault(t => t.IsAlive);
                    if (bestTank != null)
                        return bestTank;
                    var bestInt = (from unit in ObjectManager.GetObjectsOfType<WoWPlayer>(false)
                                   where unit.IsAlive
                                   where unit.HealthPercent <= 30
                                   where unit.IsPlayer
                                   where !unit.IsHostile
                                   where unit.InLineOfSight
                                   select unit).FirstOrDefault();
                    return bestInt;
                }
                return null;
            }
        }
        #endregion

        #region ChargeInterupt
        public static WoWUnit ChargeInt
        {
            get
            {
                if (!StyxWoW.Me.GroupInfo.IsInParty)
                    return null;
                if (StyxWoW.Me.GroupInfo.IsInParty)
                {
                    var bestInt = (from unit in ObjectManager.GetObjectsOfType<WoWPlayer>(false)
                                   where unit.IsAlive
                                   where unit.IsCasting
                                   where unit.CanInterruptCurrentSpellCast
                                   where unit.IsPlayer
                                   where unit.IsHostile
                                   where unit.InLineOfSight
                                   where unit.Distance <= 25
                                   where unit.Distance >= 8
                                   select unit).FirstOrDefault();
                    return bestInt;
                }
                return null;
            }
        }
        #endregion

        #region CreateChargeBehavior
        static Composite CreateChargeBehavior()
        {
            return new Decorator(
                    ret => StyxWoW.Me.CurrentTarget != null && !IsGlobalCooldown()/*&& PreventDoubleCharge*/,

                    new PrioritySelector(
                        Spell.Cast("Charge",
                            ret => StyxWoW.Me.CurrentTarget.Distance >= 10 && StyxWoW.Me.CurrentTarget.Distance < (TalentManager.HasGlyph("Long Charge") ? 30f : 25f)),

                        Spell.CastOnGround("Heroic Leap",
                            ret => StyxWoW.Me.CurrentTarget.Location,
                            ret => StyxWoW.Me.CurrentTarget.Distance > 13 && StyxWoW.Me.CurrentTarget.Distance < 40 && SpellManager.Spells["Charge"].Cooldown)
                        )
                );
        }
        #endregion

        #region CreateInterruptSpellCast
        public static Composite CreateInterruptSpellCast(UnitSelectionDelegate onUnit)
        {
            return new Decorator(
                // If the target is casting, and can actually be interrupted, AND we've waited out the double-interrupt timer, then find something to interrupt with.
                ret => onUnit != null && onUnit(ret) != null/*&& PreventDoubleInterrupt*/,
                new PrioritySelector(
                    Spell.Cast("Pummel", onUnit),
                    // AOE interrupt
                    Spell.Cast("Disrupting Shout", onUnit, ret => onUnit(ret).Distance < 10),
                    //Spell.Cast("Mass Spell Reflection", onUnit, ret => onUnit(ret).IsCasting),
                    //Spell.Cast("Shockwave", onUnit, ret => onUnit(ret).Distance < 10 && Me.IsFacing(onUnit(ret))),
                    //Spell.Cast("Indimidating Shout", onUnit, ret => onUnit(ret).Distance < 8),
                    // Racials last.
                    Spell.Cast("Arcane Torrent", onUnit),
                    // Don't waste stomp on bosses. They can't be stunned 99% of the time!
                    Spell.Cast("War Stomp", onUnit, ret => !onUnit(ret).IsBoss() && onUnit(ret).Distance < 8),
                    Spell.Cast("Quaking Palm", onUnit)
                    ));
        }
        #endregion

        #region Demo Banner
        private static Composite DemoBanner()
        {
            return new Decorator(ret => SpellManager.Spells["Charge"].Cooldown &&
                                        SpellManager.Spells["Heroic Leap"].Cooldown &&
                                       !SpellManager.Spells["Demoralizing Banner"].Cooldown &&
                                       !SpellManager.Spells["Intervene"].Cooldown &&
                                       !FriendlyUnitsNearTarget(6f).Any() &&
                                        StyxWoW.Me.CurrentTarget.Distance >= 10 && StyxWoW.Me.CurrentTarget.Distance <= 25,
                            new Action(ret =>
                            {
                                SpellManager.Cast("Demoralizing Banner");
                                SpellManager.ClickRemoteLocation(StyxWoW.Me.CurrentTarget.Location);
                            }));
        }
        #endregion

        #region FriendlyUnitsNearTarget
        public static IEnumerable<WoWUnit> FriendlyUnitsNearTarget(float distance)
        {
            var dist = distance * distance;
            var curTarLocation = StyxWoW.Me.CurrentTarget.Location;
            return ObjectManager.GetObjectsOfType<WoWUnit>(false, false).Where(
                        p => ValidUnit(p) && p.IsFriendly && p.Location.DistanceSqr(curTarLocation) <= dist).ToList();
        }
        #endregion

        #region IsGlobalCooldown
        public static bool IsGlobalCooldown(bool faceDuring = false, bool allowLagTollerance = true)
        {
            uint latency = allowLagTollerance ? StyxWoW.WoWClient.Latency : 0;
            TimeSpan gcdTimeLeft = SpellManager.GlobalCooldownLeft;
            return gcdTimeLeft.TotalMilliseconds > latency;
        }
        #endregion

        #region Mocking Banner
        private static Composite MockingBanner()
        {
            return new Decorator(ret => SpellManager.Spells["Demoralizing Banner"].Cooldown &&
                                        SpellManager.Spells["Demoralizing Banner"].CooldownTimeLeft.TotalSeconds <= 165 &&
                                        SpellManager.Spells["Charge"].Cooldown &&
                                        SpellManager.Spells["Heroic Leap"].Cooldown &&
                                       !SpellManager.Spells["Mocking Banner"].Cooldown &&
                                       !SpellManager.Spells["Intervene"].Cooldown &&
                                       !FriendlyUnitsNearTarget(6f).Any() &&
                                        StyxWoW.Me.CurrentTarget.Distance >= 10 && StyxWoW.Me.CurrentTarget.Distance <= 25,
                            new Action(ret =>
                            {
                                SpellManager.Cast("Mocking Banner");
                                SpellManager.ClickRemoteLocation(StyxWoW.Me.CurrentTarget.Location);
                            }));
        }
        #endregion

        #region ShatterBubbles
        static Composite ShatterBubbles()
        {
            return new Decorator(
                    ret => StyxWoW.Me.CurrentTarget.IsPlayer &&
                          (StyxWoW.Me.CurrentTarget.ActiveAuras.ContainsKey("Ice Block") ||
                           StyxWoW.Me.CurrentTarget.ActiveAuras.ContainsKey("Hand of Protection") ||
                           StyxWoW.Me.CurrentTarget.ActiveAuras.ContainsKey("Divine Shield")),
                    new PrioritySelector(
                        Spell.WaitForCast(FaceDuring.Yes),
                        Spell.Cast("Shattering Throw")));
        }
        #endregion

        #region ValidUnit
        public static bool ValidUnit(WoWUnit p)
        {
            // Ignore shit we can't select/attack
            if (!p.CanSelect || !p.Attackable)
                return false;

            // Duh
            if (p.IsDead)
                return false;

            // check for players
            if (p.IsPlayer)
                return true;

            // Dummies/bosses are valid by default. Period.
            if (p.IsTrainingDummy() || p.IsBoss())
                return true;

            // If its a pet, lets ignore it please.
            if (p.IsPet || p.OwnedByRoot != null)
                return false;

            // And ignore critters/non-combat pets
            if (p.IsNonCombatPet || p.IsCritter)
                return false;

            if (p.CreatedByUnitGuid != 0 || p.SummonedByUnitGuid != 0)
                return false;

            return true;
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
