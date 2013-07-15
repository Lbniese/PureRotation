using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvancedAI.Helpers;
using Styx;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Action = System.Action;

namespace AdvancedAI.Class
{
    internal static class BossMechs
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        //private const string pinkname = "Training Dummy";
        private const string pinkname = "Direhorn Spirit";

        #region Horridon Mechanics
        public static WoWUnit PinkDino
        {
            get
            {
                var direhornspirit = (from unit in ObjectManager.GetObjectsOfType<WoWUnit>(false)
                                      where unit.IsAlive
                                      where unit.InLineOfSight
                                      where unit.Distance < 40
                                      where unit.Name == pinkname
                                      select unit).FirstOrDefault();
                return direhornspirit;
            }
        }
        #endregion

        public static Composite SetFocus()
        {
            if (Me.FocusedUnit == null || Me.FocusedUnit.Name != pinkname)
            {
                Me.SetFocus(PinkDino);
            }
            return null;
        }

        public static Composite HorridonHeroic()
        {
            switch (StyxWoW.Me.Class)
            {
                case WoWClass.DeathKnight:
                    return new PrioritySelector(
                            SetFocus(),
                            Spell.Cast("Howling Blast", on => Me.FocusedUnit),
                            Spell.Cast("Death Coil", on => Me.FocusedUnit),
                            Spell.Cast("Icy Touch", on => Me.FocusedUnit));
                case WoWClass.Druid:
                    return new PrioritySelector(
                            SetFocus(),
                            Spell.Cast("Growl", on => Me.FocusedUnit));
                case WoWClass.Hunter:
                    return new PrioritySelector(
                            SetFocus(),
                            Spell.Cast("Chi Wave", on => Me.FocusedUnit),
                            Spell.Cast("Faerie Fire", on => Me.FocusedUnit));
                case WoWClass.Mage:
                    return new Throttle(1, 1,
                        new PrioritySelector(
                            SetFocus(),
                            Spell.Cast("Ice Lance", on => Me.FocusedUnit)));
                case WoWClass.Monk:
                    return new PrioritySelector(
                            SetFocus(),
                            Spell.Cast("Provoke", on => Me.FocusedUnit),
                            Spell.Cast("Chi Wave", on => Me.FocusedUnit));
                case WoWClass.Paladin:
                    return new PrioritySelector(
                            SetFocus(),
                            Spell.Cast("Judgement", on => Me.FocusedUnit),
                            Spell.Cast("Avenger's Shield", on => Me.FocusedUnit));
                case WoWClass.Priest:
                    return new PrioritySelector(
                            SetFocus(),
                            Spell.Cast("Shadow Word: Pain", on => Me.FocusedUnit, ret => Me.Specialization == WoWSpec.PriestShadow && Me.FocusedUnit.HasMyAura("Shadow Word: Pain")),
                            Spell.Cast("Vampiric Touch", on => Me.FocusedUnit, ret => Me.Specialization == WoWSpec.PriestShadow && Me.FocusedUnit.HasMyAura("Shadow Word: Pain")),
                            Spell.Cast("Holy Fire", on => Me.FocusedUnit),
                            Spell.Cast("Power Word: Solace", on => Me.FocusedUnit),
                            Spell.Cast("Smite", on => Me.FocusedUnit),
                            Spell.Cast("Chi Wave", on => Me.FocusedUnit));
                case WoWClass.Rogue:
                    return new PrioritySelector(
                            SetFocus(),
                            Spell.Cast("Throw", on => Me.FocusedUnit));
                case WoWClass.Shaman:
                    return new PrioritySelector(
                            SetFocus(),
                            Spell.Cast("Purge", on => Me.FocusedUnit),
                            Spell.Cast("Unleashed Elements", on => Me.FocusedUnit),
                            Spell.Cast("Lightning Bolt", on => Me.FocusedUnit));
                case WoWClass.Warlock:
                    return new PrioritySelector(
                            SetFocus(),
                            Spell.Cast("Fel Flame", on => Me.FocusedUnit));
                case WoWClass.Warrior:
                    return new PrioritySelector(
                            SetFocus(),
                            Spell.Cast("Throw", on => Me.FocusedUnit));
            }
            return null;
        }


    }
}
