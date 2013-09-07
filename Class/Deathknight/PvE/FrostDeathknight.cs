using CommonBehaviors.Actions;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.Helpers;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using AdvancedAI.Helpers;
using AdvancedAI.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = Styx.TreeSharp.Action;

namespace AdvancedAI.Spec
{
    class FrostDeathknight
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        internal static int BloodRuneSlotsActive { get { return Me.GetRuneCount(0) + Me.GetRuneCount(1); } }
        internal static int FrostRuneSlotsActive { get { return Me.GetRuneCount(2) + Me.GetRuneCount(3); } }
        internal static int UnholyRuneSlotsActive { get { return Me.GetRuneCount(4) + Me.GetRuneCount(5); } }

        private static bool IsDualWelding
        {
            get { return Me.Inventory.Equipped.MainHand != null && Me.Inventory.Equipped.OffHand != null; }
        }

        [Behavior(BehaviorType.PreCombatBuffs, WoWClass.DeathKnight, WoWSpec.DeathKnightFrost)]
        public static Composite FrostDKPreCombatBuffs()
        {
            return new PrioritySelector(
                Spell.Cast("Horn of Winter", ret => !Me.HasAura("Horn of Winter")));

        }

        [Behavior(BehaviorType.Combat, WoWClass.DeathKnight, WoWSpec.DeathKnightFrost)]
        public static Composite FrostDKCombat()
        {
            return new PrioritySelector(
                Spell.WaitForCastOrChannel(),
                // Interrupt please.
                Spell.Cast("Mind Freeze", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast),
                Spell.Cast("Strangulate", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast),

                //Staying Alive
                Spell.Cast("Conversion",
                    ret => Me.HealthPercent < 50 && Me.RunicPowerPercent >= 20 && !Me.HasAura("Conversion")),
                Spell.Cast("Conversion",
                    ret => Me.HealthPercent > 65 && Me.HasAura("Conversion")),
                Spell.Cast("Death Pact",
                    ret => Me.HealthPercent < 45),
                Spell.Cast("Death Siphon",
                    ret => Me.HealthPercent < 50),
                Spell.Cast("Icebound Fortiude",
                    ret => Me.HealthPercent < 40),
                Spell.Cast("Death Strike",
                    ret => Me.GotTarget &&
                        Me.HealthPercent < 15),
                Spell.Cast("Lichborne",
                    ret => // use it to heal with deathcoils.
                            (Me.HealthPercent < 25
                            && Me.CurrentRunicPower >= 60)),
                Spell.Cast("Death Coil", at => Me,
                    ret => Me.HealthPercent < 50 &&
                                Me.HasAura("Lichborne")),

                //Common with DW and 2H
                Spell.Cast("Blood Tap",
                        ret => Me.HasAura("Blood Charge", 10) &&
                        (Me.UnholyRuneCount == 0 || Me.DeathRuneCount == 0 || Me.FrostRuneCount == 0)),

                new Decorator(ret => Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 10 * 10) >= 5 && AdvancedAI.Aoe,
                    CreateAoe()),
                   
                //Cooldowns
                new Decorator(ret => AdvancedAI.Burst,
                    new PrioritySelector(
                new Action(ret => { Item.UseTrinkets(); return RunStatus.Failure; }),
                Spell.Cast("Pillar of Frost"),
                Spell.Cast("Raise Dead"))),
                new Action(ret => { Item.UseWaist(); return RunStatus.Failure; }),
                new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),


                //Normal
                new Decorator(ctx => IsDualWelding,
                    new PrioritySelector(
                        //Plague Leech is kinda hard to get to work with max dps rotations, have to have both Diseases up to make it work!   
                        Spell.Cast("Plague Leech", ret =>
                            SpellManager.Spells["Outbreak"].CooldownTimeLeft.Seconds <= 1 && Me.CurrentTarget.HasMyAura("Blood Plague") ||
                                Me.CurrentTarget.GetAuraTimeLeft("Blood Plague", true).TotalSeconds <= 3 && Me.CurrentTarget.HasMyAura("Frost Fever") && Me.CurrentTarget.HasMyAura("Blood Plague") ||
                                Me.CurrentTarget.GetAuraTimeLeft("Frost Fever", true).TotalSeconds <= 3 && Me.CurrentTarget.HasMyAura("Blood Plague") && Me.CurrentTarget.HasMyAura("Frost Fever")),
                        Spell.Cast("Outbreak", ret =>
                                Me.CurrentTarget.GetAuraTimeLeft("Blood Plague", true).TotalSeconds <= 3 ||
                                Me.CurrentTarget.GetAuraTimeLeft("Frost Fever", true).TotalSeconds <= 3),
                        Spell.Cast("Unholy Blight", ret =>
                                Me.CurrentTarget.GetAuraTimeLeft("Blood Plague", true).TotalSeconds <= 3 ||
                                Me.CurrentTarget.GetAuraTimeLeft("Frost Fever", true).TotalSeconds <= 3),
                        Spell.Cast("Soul Reaper", ret =>
                                Me.CurrentTarget.HealthPercent <= 35 || Me.HasAura(138347) && Me.CurrentTarget.HealthPercent <= 45),
                        Spell.Cast("Howling Blast", ret =>
                                !Me.CurrentTarget.HasMyAura("Frost Fever")),
                        Spell.Cast("Plague Strike", ret =>
                                !Me.CurrentTarget.HasMyAura("Blood Plague")),
                        Spell.Cast("Frost Strike", ret =>
                                Me.HasAura("Killing Machine")),
                        Spell.Cast("Howling Blast", ret =>
                                Me.HasAura("Freezing Fog")),
                        Spell.Cast("Death Siphon", ret =>
                                Me.CurrentTarget.IsPlayer),
                        Spell.Cast("Frost Strike", ret =>
                                Me.RunicPowerPercent >= 76),
                        Spell.Cast("Howling Blast", ret =>
                                Me.DeathRuneCount > 1 || Me.FrostRuneCount > 1),
                        Spell.CastOnGround("Death and Decay", ret => Me.CurrentTarget.Location, ret => true, false),
                        Spell.Cast("Blood Tap", ret =>
                                Me.HasAura("Blood Charge", 5)
                                && (BloodRuneSlotsActive == 0 || FrostRuneSlotsActive == 0 || UnholyRuneSlotsActive == 0)),
                        Spell.Cast("Horn of Winter", ret =>
                                Me.RunicPowerPercent <= 76),
                        Spell.Cast("Frost Strike"),
                        Spell.Cast("Obliterate", ret =>
                                Me.UnholyRuneCount > 0),
                        Spell.Cast("Howling Blast"),
                        Spell.Cast("Empower Rune Weapon", ret =>
                                AdvancedAI.Burst && Me.UnholyRuneCount == 0 && Me.DeathRuneCount == 0 && Me.FrostRuneCount == 0))),

                // *** 2 Hand Single Target Priority
                new Decorator(ctx => !IsDualWelding,
                    new PrioritySelector(
                        //Plague Leech is kinda hard to get to work with max dps rotations, have to have both Diseases up to make it work! 
                        //Spell.Cast("Plague Leech", ret =>
                        //        SpellManager.Spells["Outbreak"].CooldownTimeLeft.Seconds <= 1 && Me.CurrentTarget.HasAura("Blood Plague") && Me.CurrentTarget.HasAura("Frost Fever") ||
                        //        Me.HasAura("Freezing Fog") && StyxWoW.Me.CurrentTarget.GetAuraTimeLeft("Blood Plague", true).TotalSeconds <= 3 && Me.CurrentTarget.HasAura("Frost Fever") && Me.UnholyRuneCount >= 1 ||
                        //        Me.HasAura("Freezing Fog") && StyxWoW.Me.CurrentTarget.GetAuraTimeLeft("Blood Plague", true).TotalSeconds <= 3 && Me.CurrentTarget.HasAura("Frost Fever") && Me.DeathRuneCount >= 1),
                        Spell.Cast("Plague Leech", ret => Me.CurrentTarget.GetAuraTimeLeft("Blood Plague", true).TotalSeconds < 1 && Me.CurrentTarget.HasMyAura("Frost Fever")),
                        Spell.Cast("Outbreak", ret =>
                                Me.CurrentTarget.GetAuraTimeLeft("Blood Plague", true).TotalSeconds <= 3 ||
                                Me.CurrentTarget.GetAuraTimeLeft("Frost Fever", true).TotalSeconds <= 3),
                        Spell.Cast("Unholy Blight", ret =>
                                Me.CurrentTarget.GetAuraTimeLeft("Blood Plague", true).TotalSeconds <= 3 ||
                                Me.CurrentTarget.GetAuraTimeLeft("Frost Fever", true).TotalSeconds <= 3),
                        Spell.Cast("Soul Reaper", ret =>
                                Me.CurrentTarget.HealthPercent <= 35 || Me.HasAura(138347) && Me.CurrentTarget.HealthPercent <= 45),
                        Spell.Cast("Blood Tap", ret =>
                                Me.HasAura("Blood Charge", 5)
                                && (BloodRuneSlotsActive == 0 || FrostRuneSlotsActive == 0 || UnholyRuneSlotsActive == 0)),
                        Spell.Cast("Howling Blast", ret =>
                                !Me.CurrentTarget.HasMyAura("Frost Fever")),
                        Spell.Cast("Plague Strike", ret =>
                                !Me.CurrentTarget.HasMyAura("Blood Plague")),
                        Spell.Cast("Howling Blast", ret =>
                                Me.HasAura("Freezing Fog")),
                new Decorator(ctx => Me.CurrentTarget.HasMyAura("Frost Fever") && Me.CurrentTarget.HasMyAura("Blood Plague"),
                    new PrioritySelector(
                        Spell.Cast("Obliterate", ret =>
                                Me.HasAura("Killing Machine")),
                        Spell.Cast("Frost Strike", ret =>
                                Me.RunicPowerPercent >= 76),
                        Spell.Cast("Obliterate", ret =>
                                Me.UnholyRuneCount >= 1 && Me.DeathRuneCount >= 1 ||
                                Me.FrostRuneCount >= 1 && Me.DeathRuneCount >= 1 ||
                                Me.UnholyRuneCount >= 1 && Me.FrostRuneCount >= 1),
                        Spell.Cast("Plague Leech", ret => Me.CurrentTarget.GetAuraTimeLeft("Blood Plague", true).TotalSeconds < 3 && Me.CurrentTarget.HasMyAura("Frost Fever")),

                        Spell.Cast("Frost Strike", ret =>
                                !Me.HasAura("Killing Machine") && Me.UnholyRuneCount == 0 || Me.DeathRuneCount == 0 || Me.FrostRuneCount == 0),
                        Spell.Cast("Obliterate", ret =>
                                Me.RunicPowerPercent <= 76),
                        Spell.Cast("Horn of Winter", ret =>
                                Me.RunicPowerPercent <= 76),
                        Spell.Cast("Frost Strike"),
                        Spell.Cast("Plague Leech", ret => Me.CurrentTarget.HasMyAura("Frost Fever") && Me.CurrentTarget.HasMyAura("Blood Plague")),
                        Spell.Cast("Empower Rune Weapon", ret =>
                                AdvancedAI.Burst && Me.UnholyRuneCount == 0 && Me.DeathRuneCount == 0 && Me.FrostRuneCount == 0))))));
        }

        private static Composite CreateAoe()
        {
            return new PrioritySelector(
//        actions.aoe
//F	0.00	unholy_blight,if=talent.unholy_blight.enabled
                      new Throttle(1, 2,
                        new PrioritySelector(
                            Spell.Cast("Unholy Blight",
                                ret => TalentManager.IsSelected((int)DeathKnightTalents.UnholyBlight) &&
                                        Me.CurrentTarget.DistanceSqr <= 10 * 10 &&
                                        !StyxWoW.Me.HasAura("Unholy Blight")))),
//G	14.11	pestilence,if=dot.blood_plague.ticking&talent.plague_leech.enabled,line_cd=28
                      new Throttle(1, 2,
                        new PrioritySelector(
                            Spell.Cast("Blood Boil",
                                ret => TalentManager.IsSelected((int)DeathKnightTalents.RoillingBlood) &&
                                        !Me.HasAura("Unholy Blight") &&
                                        ShouldSpreadDiseases))),
                      new Throttle(1, 2,
                        new PrioritySelector(
                            Spell.Cast("Pestilence",
                                 ret => !TalentManager.IsSelected((int)DeathKnightTalents.RoillingBlood) && 
                                        !Me.HasAura("Unholy Blight") &&
                                        ShouldSpreadDiseases))),
//H	0.00	pestilence,if=dot.blood_plague.ticking&talent.unholy_blight.enabled&cooldown.unholy_blight.remains<49,line_cd=28

                            Spell.Cast("Soul Reaper", ret =>
                                    Me.CurrentTarget.HealthPercent <= 35 || Me.HasAura(138347) && Me.CurrentTarget.HealthPercent <= 45),
//I	174.96	howling_blast
                            Spell.Cast("Howling Blast"),
//J	0.00	blood_tap,if=talent.blood_tap.enabled&buff.blood_charge.stack>10
                            Spell.Cast("Blood Tap", ret =>
                                    Me.HasAura("Blood Charge", 10)
                                    && (BloodRuneSlotsActive == 0 || FrostRuneSlotsActive == 0 || UnholyRuneSlotsActive == 0)),
//K	22.21	frost_strike,if=runic_power>76
                            Spell.Cast("Frost Strike", ret =>
                                    Me.RunicPowerPercent >= 76),
//L	14.13	death_and_decay,if=unholy=1
                            Spell.CastOnGround("Death and Decay", on => Me.CurrentTarget.Location, ret => Me.UnholyRuneCount == 1),
//M	34.90	plague_strike,if=unholy=2
                            Spell.Cast("Plague Strike", ret => Me.UnholyRuneCount == 2),
//N	0.00	blood_tap,if=talent.blood_tap.enabled
                            Spell.Cast("Blood Tap", ret =>
                                    Me.HasAura("Blood Charge", 5)
                                    && (BloodRuneSlotsActive == 0 || FrostRuneSlotsActive == 0 || UnholyRuneSlotsActive == 0)),
//O	139.66	frost_strike
                            Spell.Cast("Frost Strike"),    
//P	11.37	horn_of_winter
                            Spell.Cast("Horn of Winter"),
//Q	7.92	plague_leech,if=talent.plague_leech.enabled&unholy=1
                            Spell.Cast("Plague Leech", ret =>
                                    Me.CurrentTarget.HasAura("Frost Fever") && Me.CurrentTarget.HasAura("Blood Plague") &&
                                    Me.UnholyRuneCount == 1),
//R	9.05	plague_strike,if=unholy=1
                            Spell.Cast("Plague Strike", ret => Me.UnholyRuneCount == 1),
//S	1.17	empower_rune_weapon
                            Spell.Cast("Empower Rune Weapon", ret =>
                                    AdvancedAI.Burst && Me.UnholyRuneCount == 0 && Me.DeathRuneCount == 0 && Me.FrostRuneCount == 0)
                );
        }

        Composite CreateExecuteRange()
        {
            return new PrioritySelector(
                );
        }

        internal static bool ShouldSpreadDiseases
        {
            get
            {
                int radius = TalentManager.HasGlyph("Pestilence") ? 15 : 10;
                return !Me.CurrentTarget.HasAuraExpired("Blood Plague")
                    && !Me.CurrentTarget.HasAuraExpired("Frost Fever")
                    && Unit.NearbyUnfriendlyUnits.Any(u => Me.SpellDistance(u) < radius && u.HasAuraExpired("Blood Plague"));
            }
        }

        #region DeathKnightTalents
        public enum DeathKnightTalents
        {
            RoillingBlood = 1,//Tier 1
            PlagueLeech,
            UnholyBlight,
            LichBorne,//Tier 2
            AntiMagicZone,
            Purgatory,
            DeathsAdvance,//Tier 3
            Chilblains,
            Asphyxiate,
            DeathPact,//Tier 4
            DeathSiphon,
            Conversion,
            BloodTap,//Tier 5
            RunicEmpowerment,
            RunicCorruption,
            GorefiendsGrasp,//Tier 6
            RemoreselessWinter,
            DesecratedGround
        }
        #endregion
    }
}
