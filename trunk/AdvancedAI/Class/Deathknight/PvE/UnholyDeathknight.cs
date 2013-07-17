using AdvancedAI.Managers;
using CommonBehaviors.Actions;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.Helpers;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using AdvancedAI.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = Styx.TreeSharp.Action;
using System.Windows.Forms;

namespace AdvancedAI.Spec
{
    class UnholyDeathknight
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        private const int SuddenDoom = 81340;
        internal static int BloodRuneSlotsActive { get { return Me.GetRuneCount(0) + Me.GetRuneCount(1); } }
        internal static int FrostRuneSlotsActive { get { return Me.GetRuneCount(2) + Me.GetRuneCount(3); } }
        internal static int UnholyRuneSlotsActive { get { return Me.GetRuneCount(4) + Me.GetRuneCount(5); } }

        public static Composite CreateUDKCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        UnholyDeathknightPvP.CreateUDKPvPCombat),

                    Spell.WaitForCastOrChannel(),
                    // Interrupt please.
                    Spell.Cast("Mind Freeze", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast),
                    Spell.Cast("Strangulate", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast),

                    //Staying Alive
                         //Item.CreateUsePotionAndHealthstone(60, 40),

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
                    new Throttle(2,
                        new PrioritySelector(
                            Spell.Cast("Blood Tap",
                                ret => Me.HasAura("Blood Charge", 10) &&
                                (Me.UnholyRuneCount == 0 || Me.DeathRuneCount == 0 || Me.FrostRuneCount == 0)))),

                    //Dispells if we ever need to add it in for pve but you have to be glyphed for it
                   //new Decorator(ret => Me.CurrentTarget.HasAnyAura(),
                   //     new PrioritySelector(
                   //         Spell.Cast("Icy Touch"))),

                    // AOE
                    //new Decorator (ret => UnfriendlyUnits.Count() >= 2, CreateAoe()),

                    // Execute
                    Spell.Cast("Soul Reaper",
                        ret => Me.CurrentTarget.HealthPercent < 36),
                    // Diseases
                    Spell.Cast("Outbreak",
                        ret => !Me.CurrentTarget.HasMyAura("Frost Fever") ||
                                !Me.CurrentTarget.HasMyAura("Blood Plague")),
                    Spell.Cast("Plague Strike",
                        ret => !StyxWoW.Me.CurrentTarget.HasMyAura("Blood Plague") || !StyxWoW.Me.CurrentTarget.HasMyAura("Frost Fever")),
                      new Throttle(1, 2,
                        new PrioritySelector(
                            Spell.Cast("Unholy Blight",
                                ret => Unit.UnfriendlyUnitsNearTarget(12f).Count() >= 2 &&
                                        TalentManager.IsSelected((int)DeathKnightTalents.UnholyBlight) &&
                                        Me.CurrentTarget.DistanceSqr <= 10 * 10 &&
                                        !StyxWoW.Me.HasAura("Unholy Blight")))),
                      new Throttle(1, 2,
                        new PrioritySelector(
                            Spell.Cast("Blood Boil",
                                ret => Unit.UnfriendlyUnitsNearTarget(12f).Count() >= 2 &&
                                        TalentManager.IsSelected((int)DeathKnightTalents.RoillingBlood) &&
                                        !Me.HasAura("Unholy Blight") &&
                                        //StyxWoW.Me.CurrentTarget.DistanceSqr <= 15 * 15 && 
                                        ShouldSpreadDiseases))),

                    //Cooldowns
                    new Decorator(ret => AdvancedAI.Burst,
                        new PrioritySelector(
                            Spell.Cast("Unholy Frenzy",
                                ret => Me.CurrentTarget.IsWithinMeleeRange &&
                                      !PartyBuff.WeHaveBloodlust),
                    new Action(ret => { Item.UseTrinkets(); return RunStatus.Failure; }),
                    new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),
                    Spell.Cast("Summon Gargoyle"))),
                    new Throttle(1, 2,
                    new PrioritySelector(
                        Spell.Cast("Pestilence",
                            ret => Unit.UnfriendlyUnitsNearTarget(12f).Count() >= 2 &&
                            !Me.HasAura("Unholy Blight") &&
                            ShouldSpreadDiseases))),

                    //Kill Time
                    Spell.Cast("Dark Transformation",
                        ret => Me.GotAlivePet &&
                                !Me.Pet.ActiveAuras.ContainsKey("Dark Transformation") &&
                                Me.HasAura("Shadow Infusion", 5)),
                    Spell.CastOnGround("Death and Decay", ret => Me.CurrentTarget.Location, ret => true, false),
                    Spell.Cast("Scourge Strike",
                        ret => Me.UnholyRuneCount == 2),
                    Spell.Cast("Festering Strike",
                        ret => Me.BloodRuneCount == 2 && Me.FrostRuneCount == 2),

                    Spell.Cast("Death Coil",
                        ret => (Me.HasAura(SuddenDoom) || Me.CurrentRunicPower >= 90)),// && StyxWoW.Me.Auras["Shadow Infusion"].StackCount < 5),
                    Spell.Cast("Scourge Strike"),
                    Spell.Cast("Plague Leech",
                        ret => SpellManager.Spells["Outbreak"].CooldownTimeLeft.Seconds <= 1),
                    Spell.Cast("Festering Strike"),
                    //Blood Tap
                    Spell.Cast("Blood Tap", ret =>
                        Me.HasAura("Blood Charge", 5)
                        && (BloodRuneSlotsActive == 0 || FrostRuneSlotsActive == 0 || UnholyRuneSlotsActive == 0)),
                    Spell.Cast("Death Coil",
                        ret => SpellManager.Spells["Lichborne"].CooldownTimeLeft.Seconds >= 4 && Me.CurrentRunicPower < 60 || !Me.HasAura("Conversion")), // || StyxWoW.Me.Auras["Shadow Infusion"].StackCount == 5),
                    Spell.Cast("Horn of Winter"),
                    Spell.Cast("Empower Rune Weapon",
                                ret => AdvancedAI.Burst && (Me.BloodRuneCount == 0 && Me.FrostRuneCount == 0 && Me.UnholyRuneCount == 0)),
                    new ActionAlwaysSucceed());
            }
        }

        public static Composite CreateUDKBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        UnholyDeathknightPvP.CreateUDKPvPBuffs));
            }
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
            None = 0,
            RoillingBlood,//Tier 1
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
