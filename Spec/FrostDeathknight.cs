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
    class FrostDeathknight : AdvancedAI
    {

        public override WoWClass Class { get { return WoWClass.DeathKnight; } }
        static LocalPlayer Me { get { return StyxWoW.Me; } }



        internal static int BloodRuneSlotsActive { get { return Me.GetRuneCount(0) + Me.GetRuneCount(1); } }
        internal static int FrostRuneSlotsActive { get { return Me.GetRuneCount(2) + Me.GetRuneCount(3); } }
        internal static int UnholyRuneSlotsActive { get { return Me.GetRuneCount(4) + Me.GetRuneCount(5); } }

        private static bool IsDualWelding
        {
            get { return Me.Inventory.Equipped.MainHand != null && Me.Inventory.Equipped.OffHand != null; }
        }



        protected override Composite CreateBuffs()
        {
            return Spell.Cast("Horn of Winter", ret => !Me.HasAura("Horn of Winter"));
        }
        


        protected override Composite CreateCombat()
        {
            return new PrioritySelector(


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
                                   Me.UnholyRuneCount == 0 && Me.DeathRuneCount == 0 && Me.FrostRuneCount == 0)))



                );
        }
        Composite CreateAoe()
        {
            return new PrioritySelector(



                );
        }
        Composite CreateExecuteRange()
        {
            return new PrioritySelector(

                );
        }

    }
}
