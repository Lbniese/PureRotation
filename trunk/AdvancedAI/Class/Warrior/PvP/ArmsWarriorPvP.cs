using System.Windows.Forms;
using AdvancedAI.Helpers;
using CommonBehaviors.Actions;
using Styx;
using Styx.Common;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedAI.Managers;
using Styx.CommonBot;
using Action = Styx.TreeSharp.Action;

namespace AdvancedAI.Class.Warrior.PvP
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
        public static DateTime LastInterrupt;


        [Behavior(BehaviorType.Combat, WoWClass.Warrior, WoWSpec.WarriorArms, WoWContext.Battlegrounds)]
        public static Composite ArmsPvPCombat()
        {
            return new PrioritySelector(
                new Decorator(ret => Me.CurrentTarget != null && Me.IsCasting,
                    new ActionAlwaysSucceed()),
                CreateChargeBehavior(),
                Spell.Cast("Rallying Cry", ret => Me.HealthPercent <= 30),
                new Throttle(1, 1,
                    new Sequence(
                        CreateInterruptSpellCast(on => BestInterrupt))),
                Item.UsePotionAndHealthstone(40),
                Spell.Cast("Victory Rush", ret => Me.HealthPercent <= 90 && Me.CachedHasAura("Victorious")),
                ShatterBubbles(),
                new Decorator(
                    new PrioritySelector(ret => Me.CurrentTarget.IsPlayer && !Me.CurrentTarget.IsStunned() && !Me.CurrentTarget.IsCrowdControlled() && !Me.CurrentTarget.HasAuraWithEffectsing(WoWApplyAuraType.ModDecreaseSpeed) && !Me.CurrentTarget.HasAnyAura("Piercing Howl", "Hamsting"),
                Spell.Cast("Piercing Howl"),
                Spell.Cast("Hamstring"))),
                DemoBanner(),
                HeroicLeap(),
                MockingBanner(),
                new Decorator(ret => AdvancedAI.Movement,
                    new PrioritySelector(
                        DemoBannerAuto(),
                        MockingBannerAuto())),
                Spell.Cast("Intervene", on => BestBanner),
                //Spell.CastOnGround("Demoralizing Banner", on => Me.Location, ret => Me.HealthPercent < 40),
                Spell.Cast("Disarm", ret => Me.CurrentTarget.HasAnyAura(Disarm) && !Me.CurrentTarget.HasAnyAura(DontDisarm)),
                Spell.Cast("Die by the Sword", ret => Me.HealthPercent <= 20 /*&& Me.CurrentTarget.IsMelee()*/),
                new Decorator(ret => AdvancedAI.Burst && Me.CurrentTarget.IsWithinMeleeRange,
                    new PrioritySelector(
                        Spell.Cast("Recklessness"),
                        Spell.Cast("Bloodbath"),
                        Spell.Cast("Avatar", ret => Me.CachedHasAura("Recklessness")),
                        Spell.Cast("Skull Banner", ret => Me.CachedHasAura("Recklessness")))),
                new Action(ret => { Item.UseHands(); return RunStatus.Failure; } ),
                Spell.Cast("Intervene", on => BestInterveneTarget),
                Spell.Cast("Charge", on => ChargeInt),
                Spell.Cast("Heroic Strike", ret => (Me.CurrentTarget.CachedHasAura("Colossus Smash") && Me.CurrentRage >= 70) || Me.CurrentRage >= 95),

                new Decorator(ret => Me.CurrentTarget != null && SpellManager.GlobalCooldown,
                    new ActionAlwaysSucceed()),

                Spell.Cast("Mortal Strike"),
                Spell.Cast("Dragon Roar", ret => !Me.CurrentTarget.CachedHasAura("Colossus Smash") && Me.CachedHasAura("Bloodbath") && Me.CurrentTarget.Distance <= 8),
                Spell.Cast("Colossus Smash", ret => Me.HasAuraExpired("Colossus Smash", 1)),
                Spell.Cast("Execute", ret => Me.CurrentTarget.CachedHasAura("Colossus Smash") || Me.CachedHasAura("Recklessness") || Me.CurrentRage >= 30),
                Spell.Cast("Dragon Roar", ret => Me.CurrentTarget.Distance <= 8 && (!Me.CurrentTarget.CachedHasAura("Colossus Smash") && Me.CurrentTarget.HealthPercent < 20) || Me.CachedHasAura("Bloodbath")),
                Spell.Cast("Thunder Clap", ret => Clusters.GetClusterCount(Me, Unit.NearbyUnfriendlyUnits, ClusterType.Radius, 8f) >= 2 && Clusters.GetCluster(Me, Unit.NearbyUnfriendlyUnits, ClusterType.Radius, 8).Any(u => !u.HasMyAura("Deep Wounds"))),
                Spell.Cast("Slam", ret => (Me.CurrentTarget.CachedHasAura("Colossus Smash") || Me.CachedHasAura("Recklessness")) && Me.CurrentTarget.HealthPercent >= 20),
                Spell.Cast("Overpower", ret => Me.CurrentTarget.HealthPercent >= 20 || Me.CachedHasAura("Sudden Execute")),
                Spell.Cast("Execute", ret => !Me.CachedHasAura("Sudden Execute")),
                Spell.Cast("Slam", ret => Me.CurrentRage >= 50 && Me.CurrentTarget.HealthPercent >= 20),
                Spell.Cast("Battle Shout"),
                Spell.Cast("Heroic Throw"),
                Spell.Cast("Impending Victory", ret => Me.CurrentTarget.HealthPercent > 20 || Me.HealthPercent < 50),
                new Decorator(ret => AdvancedAI.Movement,
                    Movement.CreateMoveToMeleeBehavior(true)),
                    new ActionAlwaysSucceed());
        }

        [Behavior(BehaviorType.PreCombatBuffs, WoWClass.Warrior, WoWSpec.WarriorArms, WoWContext.Battlegrounds)]
        public static Composite CreateAWPvPBuffs()
        {
            return new PrioritySelector(
                Spell.BuffSelf("Battle Shout"));
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
                                where unit.CurrentCastTimeLeft.TotalMilliseconds <
                                      MyLatency + 1000 &&
                                      InterruptCastNoChannel(unit) > MyLatency ||
                                      unit.IsChanneling &&
                                      InterruptCastChannel(unit) > MyLatency
                                select unit).FirstOrDefault();
                return bestInt;
            }
        }

        

        public static bool Interuptdelay(WoWUnit inttar)
        {
            var totaltime = inttar.CastingSpell.CastTime / 1000;
            var timeleft = inttar.CurrentCastTimeLeft.TotalSeconds;
            //Logging.Write((totaltime / 1000).ToString());
            //Logging.Write(timeleft.ToString());

            return (timeleft / totaltime) < MathEx.Random(.10, .50);

        }

        private static int InteruptMiss = 0;

        private static void addone()
        {
            var add = InteruptMiss + 1;
            InteruptMiss = add;
        }

        private static void resetIntMiss()
        {
            InteruptMiss = 0;
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
                                   where unit.IsInMyPartyOrRaid
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
                            ret => StyxWoW.Me.CurrentTarget.Distance > 13 && StyxWoW.Me.CurrentTarget.Distance < 40 && SpellManager.Spells["Charge"].Cooldown))
                );
        }
        #endregion

        #region CreateInterruptSpellCast
        public static Composite CreateInterruptSpellCast(UnitSelectionDelegate onUnit)
        {
            return new Decorator(
                // If the target is casting, and can actually be interrupted, AND we've waited out the double-interrupt timer, then find something to interrupt with.
                ret => onUnit != null && onUnit(ret) != null/*Interuptdelay(onUnit(ret))&& PreventDoubleInterrupt*/,
                new PrioritySelector(
                    //Spell.Cast("Pummel", onUnit),
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
        private static Composite DemoBannerAuto()
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
        private static Composite MockingBannerAuto()
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

        #region InterruptCastNoChannel

        private static double InterruptCastNoChannel(WoWUnit target)
        {
            if (target == null || !target.IsPlayer)
            {
                return 0;
            }
            double timeLeft = 0;

            if (target.IsCasting && (//target.CastingSpell.Name == "Arcane Blast" ||
                                     target.CastingSpell.Name == "Banish" ||
                //target.CastingSpell.Name == "Binding Heal" ||
                                     target.CastingSpell.Name == "Cyclone" ||
                //target.CastingSpell.Name == "Chain Heal" ||
                //target.CastingSpell.Name == "Chain Lightning" ||
                //target.CastingSpell.Name == "Chi Burst" ||
                                     target.CastingSpell.Name == "Chaos Bolt" ||
                                     target.CastingSpell.Name == "Demonic Circle: Summon" ||
                //target.CastingSpell.Name == "Denounce" ||
                //target.CastingSpell.Name == "Divine Light" ||
                //target.CastingSpell.Name == "Divine Plea" ||
                                     target.CastingSpell.Name == "Dominated Mind" ||
                                     target.CastingSpell.Name == "Elemental Blast" ||
                                     target.CastingSpell.Name == "Entangling Roots" ||
                //target.CastingSpell.Name == "Enveloping Mist" ||
                                     target.CastingSpell.Name == "Fear" ||
                //target.CastingSpell.Name == "Fireball" ||
                //target.CastingSpell.Name == "Flash Heal" ||
                //target.CastingSpell.Name == "Flash of Light" ||
                //target.CastingSpell.Name == "Frost Bomb" ||
                //target.CastingSpell.Name == "Frostjaw" ||
                //target.CastingSpell.Name == "Frostbolt" ||
                //target.CastingSpell.Name == "Frostfire Bolt" ||
                //target.CastingSpell.Name == "Greater Heal" ||
                //target.CastingSpell.Name == "Greater Healing Wave" ||
                                     target.CastingSpell.Name == "Haunt" ||
                                     target.CastingSpell.Name == "Heal" ||
                //target.CastingSpell.Name == "Healing Surge" ||
                //target.CastingSpell.Name == "Healing Touch" ||
                //target.CastingSpell.Name == "Healing Wave" ||
                                     target.CastingSpell.Name == "Hex" ||
                //target.CastingSpell.Name == "Holy Fire" ||
                //target.CastingSpell.Name == "Holy Light" ||
                //target.CastingSpell.Name == "Holy Radiance" ||
                //target.CastingSpell.Name == "Hibernate" ||
                                     target.CastingSpell.Name == "Mass Dispel" ||
                //target.CastingSpell.Name == "Mind Spike" ||
                //target.CastingSpell.Name == "Immolate" ||
                //target.CastingSpell.Name == "Incinerate" ||
                                     target.CastingSpell.Name == "Lava Burst" ||
                //target.CastingSpell.Name == "Mind Blast" ||
                //target.CastingSpell.Name == "Mind Spike" ||
                //target.CastingSpell.Name == "Nourish" ||
                                     target.CastingSpell.Name == "Polymorph" ||
                //target.CastingSpell.Name == "Prayer of Healing" ||
                //target.CastingSpell.Name == "Pyroblast" ||
                //target.CastingSpell.Name == "Rebirth" ||
                //target.CastingSpell.Name == "Regrowth" ||
                                     target.CastingSpell.Name == "Repentance" ||
                //target.CastingSpell.Name == "Scorch" ||
                //target.CastingSpell.Name == "Shadow Bolt" ||
                                     target.CastingSpell.Name == "Shackle Undead"
                //target.CastingSpell.Name == "Smite" ||
                //target.CastingSpell.Name == "Soul Fire" ||
                //target.CastingSpell.Name == "Starfire" ||
                //target.CastingSpell.Name == "Starsurge" ||
                //target.CastingSpell.Name == "Surging Mist" ||
                //target.CastingSpell.Name == "Transcendence" ||
                //target.CastingSpell.Name == "Transcendence: Transfer" ||
                //target.CastingSpell.Name == "Unstable Affliction" ||
                //target.CastingSpell.Name == "Vampiric Touch" ||
                //target.CastingSpell.Name == "Wrath")
                ))
            {
                timeLeft = target.CurrentCastTimeLeft.TotalMilliseconds;
            }
            return timeLeft;
        }

        #endregion

        #region InterruptCastChannel

        private static double InterruptCastChannel(WoWUnit target)
        {
            if (target == null || !target.IsPlayer)
            {
                return 0;
            }
            double timeLeft = 0;

            if (target.IsChanneling && (target.ChanneledSpell.Name == "Hymn of Hope" ||
                //target.ChanneledSpell.Name == "Arcane Barrage" ||
                                        target.ChanneledSpell.Name == "Evocation" ||
                                        target.ChanneledSpell.Name == "Mana Tea" ||
                //target.ChanneledSpell.Name == "Crackling Jade Lightning" ||
                //target.ChanneledSpell.Name == "Malefic Grasp" ||
                //target.ChanneledSpell.Name == "Hellfire" ||
                                        target.ChanneledSpell.Name == "Harvest Life" ||
                                        target.ChanneledSpell.Name == "Health Funnel" ||
                                        target.ChanneledSpell.Name == "Drain Soul" ||
                //target.ChanneledSpell.Name == "Arcane Missiles" ||
                //target.ChanneledSpell.Name == "Mind Flay" ||
                //target.ChanneledSpell.Name == "Penance" ||
                //target.ChanneledSpell.Name == "Soothing Mist" ||
                                        target.ChanneledSpell.Name == "Tranquility" ||
                                        target.ChanneledSpell.Name == "Drain Life"))
            {
                timeLeft = target.CurrentChannelTimeLeft.TotalMilliseconds;
            }

            return timeLeft;
        }

        #endregion

        #region UpdateMyLatency

        public static readonly double MyLatency = 65;

        public static void UpdateMyLatency()
        {
            //if (THSettings.Instance.LagTolerance)
            //{
            //    //If SLagTolerance enabled, start casting next spell MyLatency Millisecond before GlobalCooldown ready.

            //    MyLatency = (StyxWoW.WoWClient.Latency);
            //    //MyLatency = 0;
            //    //Use here because Lag Tolerance cap at 400
            //    //Logging.Write("----------------------------------");
            //    //Logging.Write("MyLatency: " + MyLatency);
            //    //Logging.Write("----------------------------------");

            //    if (MyLatency > 400)
            //    {
            //        //Lag Tolerance cap at 400
            //        MyLatency = 400;
            //    }
            //}
            //else
            //{
            //    //MyLatency = 400;
            //    MyLatency = 0;
            //}
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
