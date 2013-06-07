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
using System.Threading.Tasks;

namespace AdvancedAI.Spec
{
    class RetributionPaladin
    {

        private static LocalPlayer Me { get { return StyxWoW.Me; } }
        public override WoWClass Class { get { return WoWClass.Paladin; } }


        protected override Composite CreateCombat()
        {
            return new PrioritySelector(


            Spell.Cast("Rebuke", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast),

            Spell.Cast("Inquisition", ret => (!Me.HasAura("Inquisition") || Me.HasAuraExpired("Inquisition", 2)) && (Me.CurrentHolyPower >= 3 || Me.ActiveAuras.ContainsKey("Divine Purpose"))),

            Spell.Cast("Avenging Wrath", ret => Me.HasAura("Inquisition") && Me.CurrentTarget.IsBoss),

            Spell.Cast("Holy Avenger", ret => Me.HasAura("Inquisition") && Me.CurrentTarget.IsBoss),

            Spell.Cast("Guardian of Ancient Kings", ret => Me.HasAura("Inquisition") && Me.CurrentTarget.IsBoss),

                new Decorator(ret => Me.HasAura("Inquisition"),
                    new PrioritySelector(
                        new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),
                        new Action(ret => { Item.UseTrinkets(); return RunStatus.Failure; }))),

            Spell.Cast("Execution Sentence", ret => Me.HasAura("Inquisition") && Me.CurrentTarget.IsBoss),
            Spell.Cast("Holy Prism", ret => Me.HasAura("Inquisition")),
            Spell.CastOnGround("Light's Hammer", ret => Me.CurrentTarget.Location, ret => Me.HasAura("Inquisition") && Me.CurrentTarget.IsBoss),

            Spell.Cast("Divine Storm", ret => Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8 * 8) >= 2 && Me.HasAura("Inquisition") && (Me.CurrentHolyPower == 5 || Me.ActiveAuras.ContainsKey("Divine Purpose"))),

            Spell.Cast("Templar's Verdict", ret => Me.HasAura("Inquisition") && (Me.CurrentHolyPower == 5 || Me.ActiveAuras.ContainsKey("Divine Purpose"))),

            Spell.Cast("Hammer of Wrath", ret => Me.CurrentHolyPower <= 4),
                //J	40.04	wait,sec=cooldown.hammer_of_wrath.remains,if=cooldown.hammer_of_wrath.remains>0&cooldown.hammer_of_wrath.remains<=0.2


            Spell.Cast("Exorcism", ret => Me.CurrentHolyPower <= 4),
                //M	0.00	judgment,if=!(set_bonus.tier15_4pc_melee)&(target.health.pct<=20|buff.avenging_wrath.up)&active_enemies<2


            Spell.Cast("Hammer of the Righteous", ret => Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8 * 8) >= 4),

            Spell.Cast("Crusader Strike", ret => Me.CurrentHolyPower <= 4),

            Spell.Cast("Judgment", on => SecTar, ret => Clusters.GetClusterCount(Me, Unit.NearbyUnfriendlyUnits, ClusterType.Radius, 8f) >= 2 && Me.HasAura("Glyph of Double Jeopardy")),

            Spell.Cast("Judgment", ret => Me.CurrentHolyPower <= 4),

            Spell.Cast("Divine Storm", ret => Me.HasAura("Inquisition") && Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8 * 8) >= 2 && Me.GetAuraTimeLeft("Inquisition", true).TotalSeconds > 4),

            Spell.Cast("Templar's Verdict", ret => Me.HasAura("Inquisition") && Me.GetAuraTimeLeft("Inquisition", true).TotalSeconds > 4),

            Spell.Cast("Sacred Shield", on => Me, ret => !Me.HasAura("Sacred Shield"))

                    );
        }



        #region SecTar
        public static WoWUnit SecTar
        {
            get
            {
                if (!StyxWoW.Me.GroupInfo.IsInParty)
                    return null;
                if (StyxWoW.Me.GroupInfo.IsInParty)
                {
                    var secondTarget = (from unit in ObjectManager.GetObjectsOfType<WoWUnit>(false)
                                        where unit.IsAlive
                                        where unit.IsHostile
                                        where unit.Distance < 30
                                        where unit.IsTargetingMyPartyMember || unit.IsTargetingMyRaidMember
                                        where unit.InLineOfSight
                                        where unit.Guid != Me.CurrentTarget.Guid
                                        select unit).FirstOrDefault();
                    return secondTarget;
                }
                return null;
            }
        }
        #endregion
    }
}
