using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvancedAI.Helpers;
using CommonBehaviors.Actions;
using Styx;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;

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

        //public static Composite SetFocusDino()
        //{
        //    if (!Me.FocusedUnit.IsValid)
        //    {
        //        Me.SetFocus(PinkDino);
        //    }
        //    return null;
        //}

        public static Composite SetFocusDino()
        {
            return new Action(ret => { Me.SetFocus(PinkDino); return RunStatus.Failure; });
        }

        public static Composite HorridonHeroic()
        {
            switch (StyxWoW.Me.Class)
            {
                case WoWClass.DeathKnight:
                    return new PrioritySelector(
                        new Decorator(ret => Me.FocusedUnitGuid == 0,
                                SetFocusDino()),
                        Spell.Cast("Howling Blast", on => Me.FocusedUnit),
                        Spell.Cast("Death Coil", on => Me.FocusedUnit),
                        Spell.Cast("Icy Touch", on => Me.FocusedUnit));
                    break;
                case WoWClass.Druid:
                    return new PrioritySelector(
                            new Decorator(ret => Me.FocusedUnitGuid == 0,
                                SetFocusDino()),
                            Spell.Cast("Growl", on => Me.FocusedUnit),
                            Spell.Cast("Faerie Fire", on => Me.FocusedUnit));
                    break;
                case WoWClass.Hunter:
                    return new PrioritySelector(
                            new Decorator(ret => Me.FocusedUnitGuid == 0,
                                SetFocusDino()),
                            Spell.Cast("Arcane Shot", on => Me.FocusedUnit));
                    break;
                case WoWClass.Mage:
                    return new Throttle(1, 1,
                        new PrioritySelector(
                            new Decorator(ret => Me.FocusedUnitGuid == 0,
                                SetFocusDino()),
                            Spell.Cast("Ice Lance", on => Me.FocusedUnit)));
                    break;
                case WoWClass.Monk:
                    return new PrioritySelector(
                            new Decorator(ret => Me.FocusedUnitGuid == 0,
                                SetFocusDino()),
                            Spell.Cast("Provoke", on => Me.FocusedUnit),
                            Spell.Cast("Chi Wave", on => Me.FocusedUnit));
                    break;
                case WoWClass.Paladin:
                    return new PrioritySelector(
                            new Decorator(ret => Me.FocusedUnitGuid == 0,
                                SetFocusDino()),
                            Spell.Cast("Judgement", on => Me.FocusedUnit),
                            Spell.Cast("Avenger's Shield", on => Me.FocusedUnit));
                    break;
                case WoWClass.Priest:
                    return new PrioritySelector(
                            new Decorator(ret => Me.FocusedUnitGuid == 0,
                                SetFocusDino()),
                            Spell.Cast("Shadow Word: Pain", on => Me.FocusedUnit, ret => Me.Specialization == WoWSpec.PriestShadow && Me.FocusedUnit.HasMyAura("Shadow Word: Pain")),
                            Spell.Cast("Vampiric Touch", on => Me.FocusedUnit, ret => Me.Specialization == WoWSpec.PriestShadow && Me.FocusedUnit.HasMyAura("Shadow Word: Pain")),
                            Spell.Cast("Holy Fire", on => Me.FocusedUnit),
                            Spell.Cast("Power Word: Solace", on => Me.FocusedUnit),
                            Spell.Cast("Smite", on => Me.FocusedUnit),
                            Spell.Cast("Chi Wave", on => Me.FocusedUnit));
                    break;
                case WoWClass.Rogue:
                    return new PrioritySelector(
                            new Decorator(ret => Me.FocusedUnitGuid == 0,
                                SetFocusDino()),
                            Spell.Cast("Throw", on => Me.FocusedUnit));
                    break;
                case WoWClass.Shaman:
                    return new PrioritySelector(
                            new Decorator(ret => Me.FocusedUnitGuid == 0,
                                SetFocusDino()),
                            Spell.Cast("Purge", on => Me.FocusedUnit),
                            Spell.Cast("Unleashed Elements", on => Me.FocusedUnit),
                            Spell.Cast("Lightning Bolt", on => Me.FocusedUnit));
                    break;
                case WoWClass.Warlock:
                    return new PrioritySelector(
                        new Decorator(ret => Me.FocusedUnitGuid == 0,
                            SetFocusDino()),
                        Spell.Cast("Fel Flame", on => Me.FocusedUnit));
                    break;
                case WoWClass.Warrior:
                    return new PrioritySelector(
                        new Decorator(ret => Me.FocusedUnitGuid == 0,
                            SetFocusDino()),
                        Spell.Cast("Throw", on => Me.FocusedUnit));
                    break;
            }
            return null;
        }

        // Fight Specific Dispelling
        public static WoWUnit _unitDispel;
        public static Composite MechDispell()
        {
            PrioritySelector prio = new PrioritySelector();
            switch (StyxWoW.Me.Class)
            {
                case WoWClass.Paladin:
                    prio.AddChild(Spell.Cast("Cleanse", on => _unitDispel));
                    break;
                case WoWClass.Monk:
                    prio.AddChild(Spell.Cast("Detox", on => _unitDispel));
                    break;
                case WoWClass.Priest:
                    if (StyxWoW.Me.Specialization == WoWSpec.PriestHoly || StyxWoW.Me.Specialization == WoWSpec.PriestDiscipline)
                        prio.AddChild(Spell.Cast("Purify", on => _unitDispel));
                    break;
                case WoWClass.Druid:
                    if (StyxWoW.Me.Specialization == WoWSpec.DruidRestoration)
                        prio.AddChild(Spell.Cast("Nature's Cure", on => _unitDispel));
                    else
                        prio.AddChild(Spell.Cast("Remove Corruption", on => _unitDispel));
                    break;
                case WoWClass.Shaman:
                    if (StyxWoW.Me.Specialization == WoWSpec.ShamanRestoration)
                        prio.AddChild(Spell.Cast("Purify Spirit", on => _unitDispel, ret => !_unitDispel.HasAura("Reshape Life") && 
                                                                                            !_unitDispel.HasAura("Corrupted Waters") && 
                                                                                            (_unitDispel.HasAura("Matter Swap") && _unitDispel.GetAuraTimeLeft("Matter Swap", false).TotalSeconds < 5)));
                    else
                        prio.AddChild(Spell.Cast("Cleanse Spirit", on => _unitDispel));
                    break;
                case WoWClass.Mage:
                    prio.AddChild(Spell.Cast("Remove Curse", on => _unitDispel));
                    break;
            }

            return new Sequence(
                new Action(r => _unitDispel = (from unit in ObjectManager.GetObjectsOfType<WoWPlayer>(false)
                                               where unit.IsAlive
                                               where Dispelling.CanDispel(unit)
                                               select unit).OrderByDescending(u => u.HealthPercent).LastOrDefault()),
                //HealerManager.Instance.TargetList.FirstOrDefault(u => u.IsAlive && CanDispel(u))),
                prio
                );
        }




    }
}
