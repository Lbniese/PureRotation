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
        private static uint Orbs { get { return Me.GetCurrentPower(WoWPowerType.ShadowOrbs); } }

        protected override Composite CreateCombat()
        {
            return new PrioritySelector(
                new Decorator(ret => Unit.UnfriendlyMeleeUnits.Count() > 2,
                    CreateAOE()),

                Spell.BuffSelf("Shadowform"),
                Spell.BuffSelf("Inner Fire"),
                Spell.Cast("Devouring Plague", ret => Orbs == 3),
                Spell.Cast("Mind Blast", ret => Orbs < 3),
                new Throttle(1,
                    new PrioritySelector(
                        Spell.Cast("Shadow Word: Death", ret => Orbs < 3))),
                Spell.Cast("Mind Flay", ret => Me.CurrentTarget.HasAura("Devouring Plague")),
                Spell.Buff("Shadow Word: Pain", ret => Me.CurrentTarget.HasAuraExpired("Shadow Word: Pain", 2, true)),
                Spell.Buff("Vampiric Touch", ret => Me.CurrentTarget.HasAuraExpired("Vampiric Touch", 2, true)),
                Spell.Cast("Mind Flay"),
                Spell.Cast("Shadow Word: Death", ret => Me.IsMoving),
                Spell.Cast("Shadow Word: Pain", ret => Me.IsMoving)

                );
        }

        private Composite CreateAOE()
        {
            return new PrioritySelector(
                Spell.Cast("Mind Sear", on => SearTarget),
                Spell.Cast("Shadow Word: Pain", on => PainMobs),
                Spell.Cast("Vampiric Touch", on => TouchMobs),
                Spell.Cast("Mind Flay", ret => Me.CurrentTarget.HasAura("Devouring Plague")),
                Spell.Cast("Mind Blast", ret => Orbs < 3),
                Spell.Cast("Devouring Plague", ret => Orbs == 3),
                Spell.Cast("Mind Flay"));
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


    }
}
