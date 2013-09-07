using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CommonBehaviors.Actions;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.CommonBot.POI;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Styx.TreeSharp;

using Action = Styx.TreeSharp.Action;
using Styx.Helpers;
using System;

namespace AdvancedAI.Helpers
{
    class TargetingGeneral
    {
        private static Color targetColor = Color.FromRgb(240,128,128);
        private static LocalPlayer Me { get { return StyxWoW.Me; } }
        private static DateTime _timeNextInvalidTargetMessage = DateTime.MinValue;

        internal static Composite TargetingPulse()
        {
            return new Decorator(ret => AdvancedAI.Movement,
                new PrioritySelector(ctx =>
                    {
                        // If we have a RaF leader, then use its target.
                        var rafLeader = RaFHelper.Leader;

                        if (rafLeader != null && rafLeader.IsValid && !rafLeader.IsMe && rafLeader.Combat && rafLeader.CurrentTarget != null &&
                            rafLeader.CurrentTarget.IsAlive && !Blacklist.Contains(rafLeader.CurrentTarget, BlacklistFlags.Combat))
                        {
                            Logging.Write(targetColor, "Current target invalid. Switching to Tanks target " + rafLeader.CurrentTarget.SafeName() + "!");
                            return rafLeader.CurrentTarget;
                        }

                        // if we have BotPoi then try it
                        if (AdvancedAI.CurrentWoWContext != WoWContext.Normal && BotPoi.Current.Type == PoiType.Kill)
                        {   
                            var unit = BotPoi.Current.AsObject as WoWUnit;
                            if (unit == null)
                            {
                                Logging.Write(targetColor, "Current Kill POI invalid. Clearing POI!");
                                BotPoi.Clear("AdvancedAI detected null POI");
                            }
                            else if (!unit.IsAlive)
                            {
                                Logging.Write(targetColor, "Current Kill POI dead. Clearing POI " + unit.SafeName() + "!");
                                BotPoi.Clear("AdvancedAI detected Unit is dead");
                            }
                            else if (Blacklist.Contains(unit, BlacklistFlags.Combat))
                            {
                                Logging.Write(targetColor, "Current Kill POI is blacklisted. Clearing POI " + unit.SafeName() + "!");
                                BotPoi.Clear("AdvancedAI detected Unit is Blacklisted");
                            }
                            else
                            {
                                Logging.Write(targetColor, "Current target invalid. Switching to POI " + unit.SafeName() + "!");
                                return unit;
                            }
                        }   

                        // Look for agrroed mobs next. prioritize by IsPlayer, Relative Distance, then Health
                        var target = Targeting.Instance.TargetList.Where(p => !Blacklist.Contains(p, BlacklistFlags.Combat) && Unit.ValidUnit(p) &&
                            (p.Aggro || p.PetAggro || (p.Combat && p.GotTarget && (p.IsTargetingMeOrPet || p.IsTargetingMyRaidMember))))
                            .OrderBy(u => u.IsPlayer)
                            .ThenBy(CalcDistancePriority)
                            .ThenBy(u => u.HealthPercent)
                            .FirstOrDefault();

                        if (target != null)
                        {
                            // Return the closest one to us
                            Logging.Write(targetColor, "Current target invalid. Switching to aggroed mob " + target.SafeName() + "!");
                            return target;
                        }

                        // if we have BotPoi then try it
                        if (AdvancedAI.CurrentWoWContext == WoWContext.Normal && BotPoi.Current.Type == PoiType.Kill)
                        {
                            var unit = BotPoi.Current.AsObject as WoWUnit;
                            
                            if (unit == null)
                            {
                                Logging.Write(targetColor, "Current Kill POI invalid. Clearing POI!");
                                BotPoi.Clear("AdvancedAI detected null POI");
                            }
                            else if (!unit.IsAlive)
                            {
                                Logging.Write(targetColor, "Current Kill POI dead. Clearing POI " + unit.SafeName() + "!");
                                BotPoi.Clear("AdvancedAI detected Unit is dead");
                            }
                            else if (Blacklist.Contains(unit, BlacklistFlags.Combat))
                            {
                                Logging.Write(targetColor, "Current Kill POI is blacklisted. Clearing POI " + unit.SafeName() + "!");
                                BotPoi.Clear("AdvancedAI detected Unit is Blacklisted");
                            }
                            else
                            {
                                Logging.Write(targetColor, "Current target invalid. Switching to POI " + unit.SafeName() + "!");
                                return unit;
                            }
                        }

                        // now anything in the target list or a Player
                        target = Targeting.Instance.TargetList.Where(p => !Blacklist.Contains(p, BlacklistFlags.Combat) && p.IsAlive)
                            .OrderBy(u => u.IsPlayer)
                            .ThenBy(u => u.DistanceSqr)
                            .FirstOrDefault();
                        
                        if (target != null)
                        {
                            // Return the closest one to us
                            Logging.Write(targetColor, "Current target invalid. Switching to TargetList mob " + target.SafeName() + "!");
                            return target;
                        }

                        #region Commented
                        //// Cache this query, since we'll be using it for 2 checks. No need to re-query it.
                        //var agroMob =
                        //    ObjectManager.GetObjectsOfType<WoWUnit>(false, false)
                        //        .Where(p => !Blacklist.Contains(p, BlacklistFlags.Combat) && p.IsHostile && !p.IsDead
                        //                && !p.Mounted && p.DistanceSqr <= 70 * 70 && p.IsPlayer && p.Combat && (p.IsTargetingMeOrPet || p.IsTargetingMyRaidMember))
                        //        .OrderBy(u => u.DistanceSqr)
                        //        .FirstOrDefault();

                        //if (agroMob != null)
                        //{
                        //    if (!agroMob.IsPet || agroMob.SummonedByUnit == null)
                        //    {
                        //        Logger.Write(targetColor, "Current target invalid. Switching to player attacking us " + agroMob.SafeName() + "!");
                        //    }
                        //    else
                        //    {
                        //        Logger.Write(targetColor, "Current target invalid. Enemy player pet {0} attacking us, switching to player {1}!", agroMob.SafeName(), agroMob.SummonedByUnit.SafeName());
                        //        agroMob = agroMob.SummonedByUnit;
                        //    }

                        //    return agroMob;
                        //}
                        #endregion

                        // And there's nothing left, so just return null, kthx.
                        // ... but show a message about botbase still calling our Combat behavior with nothing to kill
                        if (DateTime.Now >= _timeNextInvalidTargetMessage)
                        {
                            _timeNextInvalidTargetMessage = DateTime.Now + TimeSpan.FromSeconds(1);
                            Logging.Write(targetColor, "Bot TargetList is empty, no targets available");
                        }
                    return null;
                    },

                    // Make sure the target is VALID. If not, then ignore this next part. (Resolves some silly issues!)
                    new Decorator(ret => ret != null && ((WoWUnit) ret).Guid != StyxWoW.Me.CurrentTargetGuid,
                        new Sequence(
                            CreateClearPendingCursorSpell(RunStatus.Success),
                            new Action(ret => 
                                Logging.WriteDiagnostic(targetColor, "EnsureTarget: set target to chosen target {0}", ((WoWUnit) ret).SafeName())),
                            new Action(ret => ((WoWUnit) ret).Target()),
                            new WaitContinue(2, ret => StyxWoW.Me.CurrentTarget != null && StyxWoW.Me.CurrentTargetGuid == ((WoWUnit) ret).Guid,
                                new ActionAlwaysSucceed()))),
                    // looks like no success, so don't continue to spell priorities
                    new Decorator(ret => !Me.GotTarget || Me.CurrentTarget.IsDead,
                        new ActionAlwaysSucceed()),
                    // otherwise, we are here if current target is valid or we set a good one, either way... fall through
                    new ActionAlwaysFail()));
        }

        /// <summary>
        /// assigns a priority based upon bands of distance.  allows treating all mobs within melee range the same
        /// rather than sorting purely by distance
        /// </summary>
        /// <param name="unit">unit</param>
        /// <returns>relative distance priority, where 1 is closest. 2 further away, etc</returns>
        private static int CalcDistancePriority(WoWUnit unit)
        {
            int prio = (int)Me.SpellDistance(unit);
            if (prio <= 5)
                prio = 1;
            else if (prio <= 10)
                prio = 2;
            else if (prio <= 20)
                prio = 3;
            else
                prio = 4;

            return prio;
        }

        /// <summary>
        /// targeting is blocked if pending spell on cursor, so this routine checks if a spell is on cursor
        /// awaiting target and if so clears
        /// </summary>
        /// <param name="finalResult">what result should be regardless of clearing spell</param>
        /// <returns>always finalResult</returns>
        private static Composite CreateClearPendingCursorSpell(RunStatus finalResult)
        {
            Sequence seq = new Sequence(
                new Action(r => Logging.WriteDiagnostic(targetColor, "EnsureTarget: /cancel Pending Spell {0}", Spell.GetPendingCursorSpell.Name)),
                new Action(ctx => Lua.DoString("SpellStopTargeting()"))
                );

            if (finalResult == RunStatus.Success)
                return new DecoratorContinue(ret => Spell.GetPendingCursorSpell != null, seq);

            seq.AddChild(new ActionAlwaysFail());
            return new Decorator(ret => Spell.GetPendingCursorSpell != null, seq);
        }
    }
}
