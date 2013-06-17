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

namespace AdvancedAI.Spec
{
    class ShadowPriest : AdvancedAI
    {
        public override WoWClass Class { get { return WoWClass.Priest; } }
        //public override WoWSpec Spec { get { return WoWSpec.PriestShadow; } }
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        private const int MindFlay = 15407;
        private const int Insanity = 129197;
        private static uint Orbs { get { return Me.GetCurrentPower(WoWPowerType.ShadowOrbs); } }

        protected override Composite CreateCombat()
        {
            return new PrioritySelector(
                //new Decorator(ret => Me.IsCasting && (Me.ChanneledSpell == null || Me.ChanneledSpell.Id != MindFlay), new Action(ret => { return RunStatus.Success; })),
                Spell.Cast("Shadowfiend", ret => Me.CurrentTarget.IsBoss && SpellManager.HasSpell("Shadowfiend") && SpellManager.Spells["Shadowfiend"].CooldownTimeLeft.TotalMilliseconds < 10),
                Spell.Cast("Mindbender", ret => Me.CurrentTarget.IsBoss),
                Spell.Cast("Power Infusion", ret => Me.CurrentTarget.IsBoss && SpellManager.HasSpell("Power Infusion") && SpellManager.Spells["Power Infusion"].CooldownTimeLeft.TotalMilliseconds < 10),
                Spell.Cast("Void Shift", on => VoidTank),
                new Decorator(ret => Unit.UnfriendlyUnitsNearTarget(10f).Count() > 2,
                    CreateAOE()),
                Spell.Cast("Shadow Word: Pain", ret => Orbs == 3 && Me.CurrentTarget.HasMyAura("Shadow Word: Pain") && Me.CurrentTarget.GetAuraTimeLeft("Shadow Word: Pain", true).TotalSeconds <= 6),
                Spell.Cast("Vampiric Touch", ret => Orbs == 3 && Me.CurrentTarget.HasMyAura("Vampiric Touch") && Me.CurrentTarget.GetAuraTimeLeft("Shadow Word: Pain", true).TotalSeconds <= 6),
                Spell.Cast("Devouring Plague", ret => Orbs == 3 &&
                 Me.CurrentTarget.GetAuraTimeLeft("Shadow Word: Pain", true).TotalSeconds >= 6 &&
                 Me.CurrentTarget.GetAuraTimeLeft("Vampiric Touch", true).TotalSeconds >= 6),

                //Spell.Cast("Devouring Plague", ret => Orbs == 3),
                Spell.Cast("Mind Blast", ret => Orbs < 3),
                new Throttle(1, 2,
                    new PrioritySelector(
                        Spell.Cast("Shadow Word: Death", ret => Orbs < 3))),
                new Throttle(1, 3,
                    new PrioritySelector(
                        Spell.Cast("Mind Flay", ret => Me.CurrentTarget.HasMyAura("Devouring Plague")))),
                Spell.Cast("Shadow Word: Pain", ret => !Me.CurrentTarget.HasMyAura("Shadow Word: Pain") || Me.CurrentTarget.HasAuraExpired("Shadow Word: Pain", 2, true)),
                new Throttle(1, 2,
                    new PrioritySelector(
                        Spell.Cast("Vampiric Touch", ret => !Me.CurrentTarget.HasAura("Vampiric Touch") || Me.CurrentTarget.HasAuraExpired("Vampiric Touch", 4, true)))),
                new Throttle(1, 2,
                    new PrioritySelector(
                        Spell.Cast("Mind Flay", on => Me.CurrentTarget, ret => Me.CurrentTarget.HasMyAura("Shadow Word: Pain") && Me.CurrentTarget.HasMyAura("Vampiric Touch")))),
                Spell.Cast("Shadow Word: Death", ret => Me.IsMoving),
                Spell.Cast("Shadow Word: Pain", ret => Me.IsMoving)

                );
        }

        protected override Composite CreateBuffs()
        {
            return new PrioritySelector(
                PartyBuff.BuffGroup("Power Word: Fortitude"),
                Spell.Cast("Shadowform", ret => !Me.HasAura("Shadowform")),
                Spell.Cast("Inner Fire", ret => !Me.HasAura("Inner Fire")));
        }

        private Composite CreateAOE()
        {
            return new PrioritySelector(
                Spell.Cast("Mind Sear", on => SearTarget),
                Spell.Cast("Cascade"),
                Spell.Cast("Divine Star"),
                Spell.Cast("Halo", ret => Me.CurrentTarget.Distance < 30),
                Spell.Cast("Shadow Word: Pain", on => PainMobs),
                Spell.Cast("Vampiric Touch", on => TouchMobs),
                Spell.Cast("Mind Flay", ret => Me.CurrentTarget.HasAura("Devouring Plague")),
                Spell.Cast("Mind Blast", ret => Orbs < 3),
                Spell.Cast("Devouring Plague", ret => Orbs == 3),
                Spell.Cast("Mind Flay", on => Me.CurrentTarget, ret => Me.CurrentTarget.HasAura("Shadow Word: Pain") && Me.CurrentTarget.HasAura("Vampiric Touch") && Me.ChanneledCastingSpellId != MindFlay));
        }


        public static WoWUnit PainMobs
        {
            get
            {
                var PainOn = (from unit in ObjectManager.GetObjectsOfType<WoWUnit>(false)
                               where unit.IsAlive
                               where unit.IsHostile
                               where unit.InLineOfSight
                               where !unit.HasAura("Shadow Word: Pain")
                               where unit.Distance < 40
                               where unit.IsTargetingUs() || unit.IsTargetingMyRaidMember
                               select unit).FirstOrDefault();
                return PainOn;
            }
        }

        public static WoWUnit TouchMobs
        {
            get
            {
                var PainOn = (from unit in ObjectManager.GetObjectsOfType<WoWUnit>(false)
                              where unit.IsAlive
                              where unit.IsHostile
                              where unit.InLineOfSight
                              where !unit.HasAura("Vampiric Touch")
                              where unit.Distance < 40
                              where unit.IsTargetingUs() || unit.IsTargetingMyRaidMember
                              select unit).FirstOrDefault();
                return PainOn;
            }
        }

        public static WoWUnit SearTarget
        {
            get
            {
                var bestTank = Group.Tanks.FirstOrDefault(t => t.IsAlive && Clusters.GetClusterCount(t, Unit.NearbyUnfriendlyUnits, ClusterType.Radius, 10f) >= 5);
                if (bestTank != null)
                    return bestTank;
                var SearMob = (from unit in ObjectManager.GetObjectsOfType<WoWUnit>(false)
                                where unit.IsAlive
                                where !unit.IsHostile
                                where unit.InLineOfSight
                                where Clusters.GetClusterCount(Me.CurrentTarget, Unit.NearbyUnfriendlyUnits, ClusterType.Radius, 10f) >= 4
                                select unit).FirstOrDefault();
                return SearMob;
            }
        }

        public static WoWUnit VoidTank
        {
            get
            {
                var VoidOn = (from unit in ObjectManager.GetObjectsOfType<WoWPlayer>(false)
                                where unit.IsAlive
                                where Group.Tanks.Any()
                                where unit.HealthPercent <= 30 && Me.HealthPercent > 70
                                where unit.IsPlayer
                                where !unit.IsHostile
                                where unit.InLineOfSight
                                select unit).FirstOrDefault();
                return VoidOn;
            }
        }


    }
}
