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
using System.Threading.Tasks;

namespace AdvancedAI.Spec
{
    class FrostDeathknight
    {
        /// <summary>
        /// The name of this CombatRoutine
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name { get { return "Freezing Fog"; } }


        /// <summary>
        /// The <see cref="T:Styx.WoWClass"/> to be used with this routine
        /// </summary>
        /// <value>
        /// The class.
        /// </value>
        public override WoWClass Class { get { return WoWClass.DeathKnight; } }
        private static LocalPlayer Me { get { return StyxWoW.Me; } }

        private Composite _combat, _buffs;


        public override Composite CombatBehavior { get { return _combat; } }
        public override Composite PreCombatBuffBehavior { get { return _buffs; } }
        public override Composite CombatBuffBehavior { get { return _buffs; } }

        internal static int BloodRuneSlotsActive { get { return Me.GetRuneCount(0) + Me.GetRuneCount(1); } }
        internal static int FrostRuneSlotsActive { get { return Me.GetRuneCount(2) + Me.GetRuneCount(3); } }
        internal static int UnholyRuneSlotsActive { get { return Me.GetRuneCount(4) + Me.GetRuneCount(5); } }

        private static bool IsDualWelding
        {
            get { return Me.Inventory.Equipped.MainHand != null && Me.Inventory.Equipped.OffHand != null; }
        }

        public override void Initialize()
        {
            _combat = CreateCombat();
            _buffs = CreateBuffs();
        }


        Composite CreateBuffs()
        {
            return new PrioritySelector(
                Spell.BuffSelf("Horn of Winter")


                );

        }


        Composite CreateCombat()
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
                    new Action(ret => { UseHands(); return RunStatus.Failure; }),
                    new Action(ret => { UseTrinkets(); return RunStatus.Failure; }),

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




        void UseHands()
        {
            var hands = StyxWoW.Me.Inventory.Equipped.Hands;

            if (hands != null && CanUseEquippedItem(hands))
                hands.Use();

        }


        void UseTrinkets()
        {
            var firstTrinket = StyxWoW.Me.Inventory.Equipped.Trinket1;
            var secondTrinket = StyxWoW.Me.Inventory.Equipped.Trinket2;


            if (firstTrinket != null && CanUseEquippedItem(firstTrinket))
                firstTrinket.Use();


            if (secondTrinket != null && CanUseEquippedItem(secondTrinket))
                secondTrinket.Use();


        }
        private static bool CanUseEquippedItem(WoWItem item)
        {
            // Check for engineering tinkers!
            string itemSpell = Lua.GetReturnVal<string>("return GetItemSpell(" + item.Entry + ")", 0);
            if (string.IsNullOrEmpty(itemSpell))
                return false;


            return item.Usable && item.Cooldown <= 0;
        }


        IEnumerable<WoWUnit> UnfriendlyUnits
        {
            get { return ObjectManager.GetObjectsOfType<WoWUnit>(true, false).Where(u => !u.IsDead && u.CanSelect && u.Attackable && !u.IsFriendly && u.IsWithinMeleeRange); }
        }


        private delegate T Selection<out T>(object context);
        Composite Cast(string spell, Selection<bool> reqs = null)
        {
            return
                new Decorator(
                    ret => ((reqs != null && reqs(ret)) || (reqs == null)) && SpellManager.CanCast(spell),
                    new Action(ret => SpellManager.Cast(spell)));
        }


        public static TimeSpan GetSpellCooldown(string spell)
        {
            SpellFindResults results;
            if (SpellManager.FindSpell(spell, out results))
            {
                if (results.Override != null)
                    return results.Override.CooldownTimeLeft;
                return results.Original.CooldownTimeLeft;
            }


            return TimeSpan.MaxValue;
        }
    }
}
