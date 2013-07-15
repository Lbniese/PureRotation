﻿using CommonBehaviors.Actions;
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

        public static Composite CreateFDKBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        FrostDeathknightPvP.CreateFDKPvPBuffs),
                    Spell.Cast("Horn of Winter", ret => !Me.HasAura("Horn of Winter")));
            }
        }

        public static Composite CreateFDKCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        FrostDeathknightPvP.CreateFDKPvPCombat),

                    // Interrupt please.
                    Spell.Cast("Mind Freeze", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast),
                    Spell.Cast("Strangulate", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast),

                    //Staying Alive
                    Spell.Cast("Raise Dead",
                    ret => Me.HealthPercent < 30),
                    Spell.Cast("Death Pact",
                    ret => Me.HealthPercent < 29),
                    Spell.Cast("Death Siphon",
                    ret => Me.HealthPercent < 50),
                    Spell.Cast("Icebound Fortiude",
                    ret => Me.HealthPercent < 30),
                    Spell.Cast("Death Strike",
                    ret => Me.GotTarget &&
                            Me.HealthPercent < 15),
                    Spell.Cast("Lichborne",
                            ret => // use it to heal with deathcoils.
                                (Me.HealthPercent < 25
                                && Me.CurrentRunicPower >= 60)),
                    Spell.BuffSelf("Death Coil",
                            ret => Me.HealthPercent < 25 &&
                                    Me.HasAura("Lichborne")),

                    //Common with DW and 2H
                    new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),
                    new Action(ret => { Item.UseTrinkets(); return RunStatus.Failure; }),
                    Spell.Cast("Blood Tap",
                            ret => Me.HasAura("Blood Charge", 10) &&
                            (Me.UnholyRuneCount == 0 || Me.DeathRuneCount == 0 || Me.FrostRuneCount == 0)),

                    new Decorator(ret => Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8 * 8) >= 5,
                        CreateAoe()),

                    //CoolDowns 
                    Spell.Cast("Pillar of Frost"),
                    Spell.Cast("Raise Dead"),

                    // AoE
                    //new Decorator(ret => UnfriendlyUnits.Count() >= 2,
                    //    CreateAoe()),

                    //Normal
                    new Decorator(ctx => IsDualWelding,
                        new PrioritySelector(
                            //Plague Leech is kinda hard to get to work with max dps rotations, have to have both Diseases up to make it work!   
                            Spell.Cast("Plague Leech", ret =>
                                SpellManager.Spells["Outbreak"].CooldownTimeLeft.Seconds <= 1 && Me.CurrentTarget.HasAura("Blood Plague") ||
                                Me.CurrentTarget.GetAuraTimeLeft("Blood Plague", true).TotalSeconds <= 3 && Me.CurrentTarget.HasAura("Frost Fever") && Me.CurrentTarget.HasAura("Blood Plague") ||
                                Me.CurrentTarget.GetAuraTimeLeft("Frost Fever", true).TotalSeconds <= 3 && Me.CurrentTarget.HasAura("Blood Plague") && Me.CurrentTarget.HasAura("Frost Fever")),
                            Spell.Cast("Outbreak", ret =>
                                Me.CurrentTarget.GetAuraTimeLeft("Blood Plague", true).TotalSeconds <= 3 ||
                                Me.CurrentTarget.GetAuraTimeLeft("Frost Fever", true).TotalSeconds <= 3),
                            Spell.Cast("Unholy Blight", ret =>
                                Me.CurrentTarget.GetAuraTimeLeft("Blood Plague", true).TotalSeconds <= 3 ||
                                Me.CurrentTarget.GetAuraTimeLeft("Frost Fever", true).TotalSeconds <= 3),
                            Spell.Cast("Soul Reaper", ret =>
                                Me.CurrentTarget.HealthPercent <= 35),
                            Spell.Cast("Howling Blast", ret =>
                                !Me.CurrentTarget.IsImmune(WoWSpellSchool.Frost) &&
                                !Me.CurrentTarget.HasMyAura("Frost Fever")),
                            Spell.Cast("Plague Strike", ret =>
                                !Me.CurrentTarget.HasAura("Blood Plague")),
                            Spell.Cast("Frost Strike", ret =>
                                !Me.CurrentTarget.IsImmune(WoWSpellSchool.Frost) && Me.HasAura("Killing Machine")),
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
                                Me.UnholyRuneCount == 0 && Me.DeathRuneCount == 0 && Me.FrostRuneCount == 0))),

                    // *** 2 Hand Single Target Priority
                    new Decorator(ctx => !IsDualWelding,
                        new PrioritySelector(
                            //Plague Leech is kinda hard to get to work with max dps rotations, have to have both Diseases up to make it work! 
                            Spell.Cast("Plague Leech", ret =>
                                    SpellManager.Spells["Outbreak"].CooldownTimeLeft.Seconds <= 1 && Me.CurrentTarget.HasAura("Blood Plague") && Me.CurrentTarget.HasAura("Frost Fever") ||
                                    Me.HasAura("Freezing Fog") && StyxWoW.Me.CurrentTarget.GetAuraTimeLeft("Blood Plague", true).TotalSeconds <= 3 && Me.CurrentTarget.HasAura("Frost Fever") && Me.UnholyRuneCount >= 1 ||
                                    Me.HasAura("Freezing Fog") && StyxWoW.Me.CurrentTarget.GetAuraTimeLeft("Blood Plague", true).TotalSeconds <= 3 && Me.CurrentTarget.HasAura("Frost Fever") && Me.DeathRuneCount >= 1),
                            Spell.Cast("Outbreak", ret =>
                                    Me.CurrentTarget.GetAuraTimeLeft("Blood Plague", true).TotalSeconds <= 3 ||
                                    Me.CurrentTarget.GetAuraTimeLeft("Frost Fever", true).TotalSeconds <= 3),
                            Spell.Cast("Unholy Blight", ret =>
                                    Me.CurrentTarget.GetAuraTimeLeft("Blood Plague", true).TotalSeconds <= 3 ||
                                    Me.CurrentTarget.GetAuraTimeLeft("Frost Fever", true).TotalSeconds <= 3),
                            Spell.Cast("Soul Reaper", ret => StyxWoW.Me.CurrentTarget.HealthPercent <= 35),
                            Spell.Cast("Blood Tap", ret =>
                                Me.HasAura("Blood Charge", 5)
                                && (BloodRuneSlotsActive == 0 || FrostRuneSlotsActive == 0 || UnholyRuneSlotsActive == 0)),
                            Spell.Cast("Howling Blast", ret =>
                                !Me.CurrentTarget.IsImmune(WoWSpellSchool.Frost) &&
                                !Me.CurrentTarget.HasMyAura("Frost Fever")),
                            Spell.Cast("Plague Strike", ret =>
                                !Me.CurrentTarget.HasAura("Blood Plague")),
                            Spell.Cast("Howling Blast", ret =>
                                Me.HasAura("Freezing Fog")),
                            Spell.Cast("Obliterate", ret =>
                                Me.UnholyRuneCount >= 1 && Me.DeathRuneCount >= 1 ||
                                Me.FrostRuneCount >= 1 && Me.DeathRuneCount >= 1 ||
                                Me.UnholyRuneCount >= 1 && Me.FrostRuneCount >= 1),
                            Spell.Cast("Obliterate", ret =>
                                Me.HasAura("Killing Machine")),
                            Spell.Cast("Frost Strike", ret =>
                                !Me.CurrentTarget.IsImmune(WoWSpellSchool.Frost) && !Me.HasAura("Killing Machine") && Me.UnholyRuneCount == 0 || Me.DeathRuneCount == 0 || Me.FrostRuneCount == 0),
                            Spell.Cast("Obliterate", ret =>
                                Me.RunicPowerPercent <= 76),
                            Spell.Cast("Horn of Winter", ret =>
                                Me.RunicPowerPercent <= 76),
                            Spell.Cast("Frost Strike"),
                            Spell.Cast("Empower Rune Weapon", ret =>
                                Me.UnholyRuneCount == 0 && Me.DeathRuneCount == 0 && Me.FrostRuneCount == 0))));
            }
        }

        private static Composite CreateAoe()
        {
            return new PrioritySelector(
//        actions.aoe
//F	0.00	unholy_blight,if=talent.unholy_blight.enabled
                            Spell.Cast("Unholy Blight", ret =>
                                    Me.CurrentTarget.GetAuraTimeLeft("Blood Plague", true).TotalSeconds <= 3 ||
                                    Me.CurrentTarget.GetAuraTimeLeft("Frost Fever", true).TotalSeconds <= 3),
//G	14.11	pestilence,if=dot.blood_plague.ticking&talent.plague_leech.enabled,line_cd=28
                    new PrioritySelector(
                        Spell.Cast("Pestilence",
                            ret => Unit.UnfriendlyUnitsNearTarget(12f).Count() >= 2 &&
                            !Me.HasAura("Unholy Blight") &&
                            ShouldSpreadDiseases)),
//H	0.00	pestilence,if=dot.blood_plague.ticking&talent.unholy_blight.enabled&cooldown.unholy_blight.remains<49,line_cd=28
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
                                Me.UnholyRuneCount == 0 && Me.DeathRuneCount == 0 && Me.FrostRuneCount == 0)
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
                    && Unit.NearbyUnfriendlyUnits.Any(u => Me.SpellDistance(u) < radius && u.HasAuraExpired("Blood Plague") && u.HasAuraExpired("Frost Fever"));
            }
        }

        #region DeathKnightTalents
        public enum DeathKnightTalents
        {
            RollingBlood = 1,//Tier 1
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
