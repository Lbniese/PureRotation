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
    class UnholyDeathknight// : AdvancedAI
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
                            ret => Me.CurrentTarget.HealthPercent < 37),

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
                                        TalentManager.IsSelected((int)DeathKnightTalents.RollingBlood) &&
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

                            Spell.CastOnGround("Death and Decay", ret => StyxWoW.Me.CurrentTarget.Location, ret => true, false),

                            Spell.Cast("Scourge Strike",
                                ret => Me.UnholyRuneCount == 2 || Me.DeathRuneCount > 0),

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

                            new ActionAlwaysSucceed()
                    );
  

                    //9	1.00	auto_attack
                    //A	3.33	blood_fury,if=time>=2
                    //B	1.00	mogu_power_potion,if=buff.dark_transformation.up&target.time_to_die<=35
                    //C	3.00	unholy_frenzy,if=time>=4
                    //D	7.90	use_item,slot=hands,if=time>=4
                    //E	0.00	run_action_list,name=aoe,if=active_enemies>=5
                    //F	0.00	run_action_list,name=single_target,if=active_enemies<5
                    //actions.single_target
                    //#	count	action,conditions
                    //c	3.82	outbreak,if=stat.attack_power>(dot.blood_plague.attack_power*1.1)&time>15&!(cooldown.unholy_blight.remains>79)
                    //d	8.48	plague_strike,if=stat.attack_power>(dot.blood_plague.attack_power*1.1)&time>15&!(cooldown.unholy_blight.remains>79)
                    //e	14.10	blood_tap,if=talent.blood_tap.enabled&buff.blood_charge.stack>10&runic_power>=32
                    //f	3.68	unholy_blight,if=talent.unholy_blight.enabled&(dot.frost_fever.remains<3|dot.blood_plague.remains<3)
                    //g	1.20	outbreak,if=dot.frost_fever.remains<3|dot.blood_plague.remains<3
                    //h	32.28	soul_reaper,if=target.health.pct-3*(target.health.pct%target.time_to_die)<=45
                    //i	12.47	blood_tap,if=talent.blood_tap.enabled&(target.health.pct-3*(target.health.pct%target.time_to_die)<=45&cooldown.soul_reaper.remains=0)
                    //j	0.00	plague_strike,if=!dot.blood_plague.ticking|!dot.frost_fever.ticking
                    //k	3.03	summon_gargoyle
                    //l	10.66	dark_transformation
                    //m	3.69	blood_tap,if=talent.blood_tap.enabled&buff.shadow_infusion.stack=5
                    //n	21.90	death_coil,if=runic_power>90
                    //o	1.14	death_and_decay,if=unholy=2
                    //p	0.00	blood_tap,if=talent.blood_tap.enabled&unholy=2&cooldown.death_and_decay.remains=0
                    //q	2.57	scourge_strike,if=unholy=2
                    //r	6.95	festering_strike,if=blood=2&frost=2
                    //s	13.56	death_and_decay
                    //t	6.20	blood_tap,if=talent.blood_tap.enabled&cooldown.death_and_decay.remains=0
                    //u	70.03	death_coil,if=buff.sudden_doom.react|(buff.dark_transformation.down&rune.unholy<=1)
                    //v	122.75	scourge_strike
                    //w	0.00	plague_leech,if=talent.plague_leech.enabled&cooldown.outbreak.remains<1
                    //x	35.36	festering_strike
                    //y	17.99	horn_of_winter
                    //z	40.24	death_coil,if=buff.dark_transformation.down|(cooldown.summon_gargoyle.remains>8&buff.dark_transformation.remains>8)
                    //{	14.84	blood_tap,if=talent.blood_tap.enabled&buff.blood_charge.stack>=8
                    //|	1.80	empower_rune_weapon
                    
            }
        }

        public static Composite CreateUDKBuffs { get; set; }

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
        #region Nested type: DeathKnightTalents

        public enum DeathKnightTalents
        {
            RollingBlood = 1,
            PlagueLeech,
            UnholyBlight,
            LichBorne,
            AntiMagicZone,
            Purgatory,
            DeathsAdvance,
            Chilblains,
            Asphyxiate,
            DeathPact,
            DeathSiphon,
            Conversion,
            BloodTap,
            RunicEmpowerment,
            RunicCorruption,
            GorefiendsGrasp,
            RemoreselessWinter,
            DesecratedGround
        }

        #endregion
    }
}
