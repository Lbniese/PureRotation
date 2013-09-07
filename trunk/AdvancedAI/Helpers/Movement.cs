﻿
using System.Threading;
using System.Windows.Forms;
using AdvancedAI.Lists;
using Styx;
using Styx.Helpers;
using Styx.Pathing;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Action = Styx.TreeSharp.Action;
using System;
using Styx.WoWInternals.WoWObjects;
using Styx.CommonBot.POI;
using CommonBehaviors.Actions;
using Styx.Common;
using AdvancedAI.Utilities;

namespace AdvancedAI.Helpers
{
    internal static class Movement
    {

        public static bool MoveTo(WoWUnit Unit)
        {
            //StopMovement();
            if (Navigator.CanNavigateFully(StyxWoW.Me.Location, StyxWoW.Me.CurrentTarget.Location))
            {
                if (StyxWoW.Me.CurrentTarget.Distance < 10)
                {
                    if (!StyxWoW.Me.MovementInfo.MovingForward)
                    {
                        StopMovement(true, true, true, true);
                        WoWMovement.Move(WoWMovement.MovementDirection.Forward);
                    }
                }
                else
                {
                    Navigator.MoveTo(StyxWoW.Me.CurrentTarget.Location);
                }
                return false;
            }
            return true;
        }

        public static void StopMovement(bool forward, bool backward, bool left, bool right)
        {
            if (StyxWoW.Me.MovementInfo.MovingStrafeRight)
            {
                WoWMovement.MoveStop(WoWMovement.MovementDirection.StrafeRight);
                //Styx.Helpers.KeyboardManager.KeyUpDown((char)Keys.E);
            }

            if (StyxWoW.Me.MovementInfo.MovingStrafeLeft)
            {
                WoWMovement.MoveStop(WoWMovement.MovementDirection.StrafeLeft);
                //Styx.Helpers.KeyboardManager.KeyUpDown((char)Keys.Q);
            }

            if (StyxWoW.Me.MovementInfo.MovingForward)
            {
                WoWMovement.MoveStop(WoWMovement.MovementDirection.Forward);
                //Styx.Helpers.KeyboardManager.KeyUpDown((char)Keys.W);
            }

            if (StyxWoW.Me.MovementInfo.MovingBackward)
            {
                WoWMovement.MoveStop(WoWMovement.MovementDirection.Backwards);
                //Styx.Helpers.KeyboardManager.KeyUpDown((char)Keys.D);
            }
        }
        /// <summary>
        ///  Creates a behavior that does nothing more than check if we're in Line of Sight of the target; and if not, move towards the target.
        /// </summary>
        /// <remarks>
        ///  Created 23/5/2011
        /// </remarks>
        /// <returns>.</returns>
        public static Composite CreateMoveToLosBehavior()
        {
            return CreateMoveToLosBehavior(ret => StyxWoW.Me.CurrentTarget);
        }

        public static Composite CreateMoveToLosBehavior(UnitSelectionDelegate toUnit)
        {
            return new Decorator(
                ret => toUnit != null
                    && toUnit(ret) != null
                    && !toUnit(ret).IsMe
                    && !InLineOfSpellSight(toUnit(ret)),
                new Sequence(
                    new Action(ret => Logging.WriteDiagnostic("MoveToLoss: moving to LoSS of {0} @ {1:F1} yds", toUnit(ret).SafeName(), toUnit(ret).Distance)),
                        //Logger.WriteDebug(Color.White, "MoveToLoss: moving to LoSS of {0} @ {1:F1} yds", toUnit(ret).SafeName(), toUnit(ret).Distance)),
                    new Action(ret => Navigator.MoveTo(toUnit(ret).Location)),
                    new Action(ret => StopMoving.InLosOfUnit(toUnit(ret)))
                    )
                );
        }

        /// <summary>
        /// true if Target in line of spell sight AND a spell hasn't just failed due
        /// to a line of sight error.  This test is required for the unit we are moving
        /// towards because WoWUnit.InLineOfSpellSight will return true while the
        /// WOW Game Client fails the spell cast.  See EventHandler.cs for setting
        /// LastLineOfSightError
        /// 
        /// Only use this for unit we are moving towards.  Not to be used when checking
        /// ability to cast spells on various mobs
        /// </summary>
        /// <param name="unit">target we are moving towards</param>
        /// <returns></returns>
        public static bool InLineOfSpellSight(WoWUnit unit)
        {
            if (unit.InLineOfSpellSight)
            {
                if ((DateTime.Now - EventHandlers.LastLineOfSightFailure).TotalMilliseconds < 1000)
                {
                    if (unit.IsWithinMeleeRange)
                    {
                        Logging.WriteDiagnostic("InLineOfSpellSight: last LoS error < 1 sec but in melee range, pretending we are in LoS");
                        //Logger.WriteDebug( Color.White, "InLineOfSpellSight: last LoS error < 1 sec but in melee range, pretending we are in LoS");
                        return true;
                    }

                    Logging.WriteDiagnostic("InLineOfSpellSight: last LoS error < 1 sec, pretending still not in LoS");
                    //Logger.WriteDebug( Color.White, "InLineOfSpellSight: last LoS error < 1 sec, pretending still not in LoS");
                    return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        ///   Creates the ensure movement stopped behavior. if no range specified, will stop immediately.  if range given, will stop if within range yds of current target
        /// </summary>
        /// <remarks>
        ///   Created 5/1/2011.
        /// </remarks>
        /// <returns>.</returns>
        public static Composite CreateEnsureMovementStoppedBehavior(float range = float.MaxValue, UnitSelectionDelegate onUnit = null, string reason = null)
        {
            if (range == float.MaxValue)
            {
                return new Decorator(
                    ret => StyxWoW.Me.IsMoving,
                    new Sequence(
                        new Action(ret => Logging.WriteDiagnostic("EnsureMovementStopped: stopping! {0}", reason ?? "")),
                            //Logger.WriteDebug(Color.White, "EnsureMovementStopped: stopping! {0}", reason ?? "")),
                        new Action(ret => StopMoving.Now())
                        )
                    );
            }

            if (onUnit == null)
                onUnit = del => StyxWoW.Me.CurrentTarget;

            return new Decorator(
                ret => StyxWoW.Me.IsMoving
                    && (onUnit(ret) == null || onUnit(ret).Distance < range),
                new Sequence(
                    new Action(ret => Logging.WriteDiagnostic("EnsureMovementStopped: stopping because {0}", onUnit(ret) == null ? "No CurrentTarget" : string.Format("target @ {0:F1} yds, stop range: {1:F1}", onUnit(ret).Distance, range))),
                        //Logger.WriteDebug(Color.White, "EnsureMovementStopped: stopping because {0}", onUnit(ret) == null ? "No CurrentTarget" : string.Format("target @ {0:F1} yds, stop range: {1:F1}", onUnit(ret).Distance, range))),
                    new Action(ret => StopMoving.Now())
                    )
                );
        }

        /// <summary>
        /// Creates ensure movement stopped if within melee range behavior.
        /// </summary>
        /// <returns></returns>
        public static Composite CreateEnsureMovementStoppedWithinMelee()
        {
            return new Decorator(
                ret => StyxWoW.Me.IsMoving
                    && InMoveToMeleeStopRange(StyxWoW.Me.CurrentTarget),
                new Sequence(
                    new Action(ret => Logging.WriteDiagnostic("EnsureMovementStoppedWithinMelee: stopping because {0}", !StyxWoW.Me.GotTarget ? "No CurrentTarget" : string.Format("target at {0:F1} yds", StyxWoW.Me.CurrentTarget.Distance))),
                        //Logger.WriteDebug(Color.White, "EnsureMovementStoppedWithinMelee: stopping because {0}", !StyxWoW.Me.GotTarget ? "No CurrentTarget" : string.Format("target at {0:F1} yds", StyxWoW.Me.CurrentTarget.Distance))),
                    new Action(ret => StopMoving.Now())
                    )
                );
        }

        /// <summary>
        ///   Creates a behavior that does nothing more than check if we're facing the target; and if not, faces the target. (Uses a hard-coded 70degree frontal cone)
        /// </summary>
        /// <remarks>
        ///   Created 5/1/2011.
        /// </remarks>
        /// <returns>.</returns>
        public static Composite CreateFaceTargetBehavior(float viewDegrees = 70f, bool waitForFacing = true)
        {
            return CreateFaceTargetBehavior(ret => StyxWoW.Me.CurrentTarget, viewDegrees, waitForFacing );
        }

        public static Composite CreateFaceTargetBehavior(UnitSelectionDelegate toUnit, float viewDegrees = 70f, bool waitForFacing = true)
        {
            return new Decorator(
                ret => toUnit != null && toUnit(ret) != null 
                    && !StyxWoW.Me.IsMoving 
                    && !toUnit(ret).IsMe 
                    && !StyxWoW.Me.IsSafelyFacing(toUnit(ret), viewDegrees),
                new Action( ret => 
                {
                    toUnit(ret).Face();

                    // check only if standard facing for wait, not value given by viewDegrees
                    // .. this optimizes for combat... cases where facing within viewDegrees
                    // .. must be enforced should use a different behavior

                    if (waitForFacing && !StyxWoW.Me.IsSafelyFacing(toUnit(ret)))
                        return RunStatus.Success;
                    
                    return RunStatus.Failure;
                })
                );
        }

        /// <summary>
        ///   Creates a move to target behavior. Will return RunStatus.Success if it has reached the location, or stopped in range. Best used at the end of a rotation.
        /// </summary>
        /// <remarks>
        ///   Created 5/1/2011.
        /// </remarks>
        /// <param name = "stopInRange">true to stop in range.</param>
        /// <param name = "range">The range.</param>
        /// <returns>.</returns>
        public static Composite CreateMoveToTargetBehavior(bool stopInRange, float range)
        {
            return CreateMoveToTargetBehavior(stopInRange, range, ret => StyxWoW.Me.CurrentTarget);
        }

        /// <summary>
        ///   Creates a move to target behavior. Will return RunStatus.Success if it has reached the location, or stopped in range. Best used at the end of a rotation.
        /// </summary>
        /// <remarks>
        ///   Created 5/1/2011.
        /// </remarks>
        /// <param name = "stopInRange">true to stop in range.</param>
        /// <param name = "range">The range.</param>
        /// <param name="onUnit">The unit to move to.</param>
        /// <returns>.</returns>
        public static Composite CreateMoveToTargetBehavior(bool stopInRange, float range, UnitSelectionDelegate onUnit)
        {
            return
                new Decorator(
                    ret => onUnit != null && onUnit(ret) != null && onUnit(ret) != StyxWoW.Me && !Spell.IsCastingOrChannelling(),
                    CreateMoveToLocationBehavior(ret => onUnit(ret).Location, stopInRange, ret => onUnit(ret).SpellRange(range)));
        }

        public static Composite CreateMoveToUnitBehavior(float range, UnitSelectionDelegate onUnit)
        {
            return CreateMoveToUnitBehavior(onUnit, range);
        }

        public static Composite CreateMoveToUnitBehavior(UnitSelectionDelegate onUnit, float range, float stopAt = float.MinValue )
        {
            return new Decorator(
                ret => onUnit != null && onUnit(ret) != null && onUnit(ret).Distance > range,
                new Sequence(
                    new Action(ret => Logging.WriteDiagnostic("MoveToUnit: moving within {0:F1} yds of {1} @ {2:F1} yds", range, onUnit(ret).SafeName(), onUnit(ret).Distance)),
                        //Logger.WriteDebug(Color.White, "MoveToUnit: moving within {0:F1} yds of {1} @ {2:F1} yds", range, onUnit(ret).SafeName(), onUnit(ret).Distance)),
                    new Action(ret => Navigator.MoveTo(onUnit(ret).Location)),
                    new Action(ret => StopMoving.InRangeOfUnit(onUnit(ret), stopAt == float.MinValue ? range : stopAt)),
                    new ActionAlwaysFail()
                    )
                );
        }

#if USE_OLD_VERSION
        /// <summary>
        ///   Creates a move to melee range behavior. Will return RunStatus.Success if it has reached the location, or stopped in range. Best used at the end of a rotation.
        /// </summary>
        /// <remarks>
        ///   Created 5/1/2011.
        /// </remarks>
        /// <param name = "stopInRange">true to stop in range.</param>
        /// <param name = "range">The range.</param>
        /// <returns>.</returns>
        public static Composite CreateMoveToMeleeBehavior(bool stopInRange)
        {
            return CreateMoveToMeleeBehavior(ret => StyxWoW.Me.CurrentTarget.Location, stopInRange);
        }
#else

        /// <summary>
        /// Creates a move to melee range behavior.  Tests .IsWithinMeleeRange so we know whether WoW thinks
        /// we are within range, which is more important than our distance calc.  For players keep moving 
        /// until 2 yds away so we stick to them in pvp
        /// </summary>
        /// <param name="stopInRange"></param>
        /// <returns></returns>
        public static Composite CreateMoveToMeleeBehavior(bool stopInRange)
        {
#if OLD_MELEE_MOVE
            return new Decorator(
                ret => !MovementManager.IsMovementDisabled,
                new PrioritySelector(
                    new Decorator(
                        ret => stopInRange && InMoveToMeleeStopRange(StyxWoW.Me.CurrentTarget),
                        new PrioritySelector(
                            CreateEnsureMovementStoppedWithinMelee(),
                            new Action(ret => RunStatus.Success)
                            )
                        ),
                    new Decorator(
                        ret => StyxWoW.Me.CurrentTarget != null && StyxWoW.Me.CurrentTarget.IsValid,
                        new Sequence(
                            new Action(ret => Logger.WriteDebug(Color.White, "MoveToMelee: towards {0} @ {1:F1} yds", StyxWoW.Me.CurrentTarget.SafeName(), StyxWoW.Me.CurrentTarget.Distance)),
                            new Action(ret => Navigator.MoveTo(StyxWoW.Me.CurrentTarget.Location)),
                            new Action(ret => StopMoving.InMeleeRangeOfUnit(StyxWoW.Me.CurrentTarget, and => InMoveToMeleeStopRange(StyxWoW.Me.CurrentTarget))),
                            new ActionAlwaysFail()
                            )
                        )
                    )
                );
#else
            return new Decorator(
                ret => StyxWoW.Me.CurrentTarget != null && !StyxWoW.Me.CurrentTarget.IsWithinMeleeRange,
                new Sequence(
                    new Action(ret =>
                    {
                        MoveResult res = Navigator.MoveTo(StyxWoW.Me.CurrentTarget.Location);
                        Logging.WriteDiagnostic("MoveToMelee({0}): towards {1} @ {2:F1} yds", res.ToString(), StyxWoW.Me.CurrentTarget.SafeName(), StyxWoW.Me.CurrentTarget.Distance);
                        //Logger.WriteDebug(Color.White, "MoveToMelee({0}): towards {1} @ {2:F1} yds", res.ToString(), StyxWoW.Me.CurrentTarget.SafeName(), StyxWoW.Me.CurrentTarget.Distance);
                    }),
                    new Action(ret => StopMoving.InMeleeRangeOfUnit(StyxWoW.Me.CurrentTarget)),
                    new ActionAlwaysFail()
                    )
                );
#endif
        }

        public static bool InMoveToMeleeStopRange( WoWUnit unit)
        {
            if (unit == null || !unit.IsValid)
                return true;

            if (unit.IsPlayer)
            {
                if (unit.DistanceSqr < (2 * 2))
                    return true;

            }
            else
            {
                var preferredDistance = unit.MeleeDistance() - 1f;
                if (unit.Distance <= preferredDistance)
                    return true;
            }

            return !unit.IsMoving && unit.IsWithinMeleeRange;
        }
#endif


        #region Move Behind

        /// <summary>
        ///   Creates a move behind target behavior. If it cannot fully navigate will move to target location
        /// </summary>
        /// <remarks>
        ///   Created 2/12/2011.
        /// </remarks>
        /// <returns>.</returns>
        public static Composite CreateMoveBehindTargetBehavior()
        {
            return CreateMoveBehindTargetBehavior(ret => true);
        }

        /// <summary>
        ///   Creates a move behind target behavior. If it cannot fully navigate will move to target location
        /// </summary>
        /// <remarks>
        ///   Created 2/12/2011.
        /// </remarks>
        /// <param name="requirements">Aditional requirments.</param>
        /// <returns>.</returns>
        public static Composite CreateMoveBehindTargetBehavior(SimpleBooleanDelegate requirements)
        {
            return new Decorator(
                ret =>
                {
                    if (StyxWoW.Me.CurrentMap.IsBattleground || !requirements(ret) || Spell.IsCastingOrChannelling() || Group.MeIsTank)
                        return false;
                    var currentTarget = StyxWoW.Me.CurrentTarget;
                    if (currentTarget == null || currentTarget.MeIsSafelyBehind || !currentTarget.IsAlive || BossList.AvoidRearBosses.Contains(currentTarget.Entry))
                        return false;
                    return currentTarget.Stunned || currentTarget.CurrentTarget != StyxWoW.Me;
                },
                new PrioritySelector(
                    ctx => CalculatePointBehindTarget(),
                    new Decorator(
                        req => Navigator.CanNavigateFully(StyxWoW.Me.Location, (WoWPoint)req, 4),
                        new Sequence(
                            new Action(ret => Logging.WriteDiagnostic("MoveBehind: behind {0} @ {1:F1} yds", StyxWoW.Me.CurrentTarget.SafeName(), StyxWoW.Me.CurrentTarget.Distance)),
                                //Logger.WriteDebug(Color.White, "MoveBehind: behind {0} @ {1:F1} yds", StyxWoW.Me.CurrentTarget.SafeName(), StyxWoW.Me.CurrentTarget.Distance)),
                            new Action(behindPoint => Navigator.MoveTo((WoWPoint)behindPoint)),
                            new Action(behindPoint => StopMoving.AtLocation((WoWPoint)behindPoint))
                            )
                        )
                    )
                );
        }

        public static WoWPoint CalculatePointBehindTarget()
        {
            float facing = StyxWoW.Me.CurrentTarget.Rotation;
            facing += WoWMathHelper.DegreesToRadians(180); // was 150 ?
            facing = WoWMathHelper.NormalizeRadian(facing);

            return StyxWoW.Me.CurrentTarget.Location.RayCast(facing, Spell.MeleeRange - 2f);
        }

        #endregion

        #region Root Move To Location

        /// <summary>
        ///   Creates a move to location behavior. Will return RunStatus.Success if it has reached the location, or stopped in range. Best used at the end of a rotation.
        /// </summary>
        /// <remarks>
        ///   Created 5/1/2011.
        /// </remarks>
        /// <param name = "location">The location.</param>
        /// <param name = "stopInRange">true to stop in range.</param>
        /// <param name = "range">The range.</param>
        /// <returns>.</returns>
        public static Composite CreateMoveToLocationBehavior(LocationRetriever location, bool stopInRange, DynamicRangeRetriever range)
        {
            // Do not fuck with this. It will ensure we stop in range if we're supposed to.
            // Otherwise it'll stick to the targets ass like flies on dog shit.
            // Specifying a range of, 2 or so, will ensure we're constantly running to the target. Specifying 0 will cause us to spin in circles around the target
            // or chase it down like mad. (PVP oriented behavior)
            return new PrioritySelector(
                        new Decorator(
                // Give it a little more than 1/2 a yard buffer to get it right. CTM is never 'exact' on where we land. So don't expect it to be.
                            ret => stopInRange && StyxWoW.Me.Location.Distance(location(ret)) < range(ret),
                            new PrioritySelector(
                                CreateEnsureMovementStoppedBehavior(),
                // In short; if we're not moving, just 'succeed' here, so we break the tree.
                                new Action(ret => RunStatus.Success)
                                )
                            ),
                        new Action(ret => Navigator.MoveTo(location(ret))),
                        new Action(ret => StopMoving.InRangeOfLocation(location(ret), range(ret)))
                        );
        }

        #endregion

/*
        private static WoWPoint lastMoveToRangeSpot = WoWPoint.Empty;
        private static bool inRange = false;
        /// <summary>
        ///   Movement for Ranged Classes or Ranged Pulls.  Move to Unit at range behavior 
        ///   that stops in line of spell sight in range of target. Moves a maximum of 
        ///   10 yds at a time to minimize run past. will also only move towards unit if
        ///   not currently moving (to allow bot/human momvement precendence.)
        /// </summary>
        /// <remarks>
        ///   Created 9/25/2012.
        /// </remarks>
        /// <param name = "toUnit">unit to move towards</param>
        /// <param name = "range">The range.</param>
        /// <returns>.</returns>       
        private static float _range;
        public static Composite CreateMoveToRangeAndStopBehavior(UnitSelectionDelegate toUnit, DynamicRangeRetriever range)
        {
            return new Sequence(
                new Action(r => _range = range(r)),
                CreateMoveToUnitBehavior(toUnit, _range)
                );
        }
*/

        public static Composite CreateWorgenDarkFlightBehavior()
        {
            return new Decorator(
                ret => StyxWoW.Me.IsAlive
                    && StyxWoW.Me.IsMoving
                    && !StyxWoW.Me.Mounted
                    && !StyxWoW.Me.IsOnTransport
                    && !StyxWoW.Me.OnTaxi
                    && StyxWoW.Me.Race == WoWRace.Worgen
                    && !StyxWoW.Me.HasAnyAura("Darkflight")
                    && (BotPoi.Current == null || BotPoi.Current.Type == PoiType.None || BotPoi.Current.Location.Distance(StyxWoW.Me.Location) > 10)
                    && !StyxWoW.Me.IsAboveTheGround(),

                new PrioritySelector(
                    Spell.WaitForCast(),
                    new Decorator(
                        ret => !Spell.IsGlobalCooldown(),
                        Spell.BuffSelf("Darkflight")
                        )
                    )
                );
        }

        //Swiny Code

        private static WoWPlayer Me = StyxWoW.Me;
        private static WoWUnit Target;
        private static int Cone = 40;
        internal static void PulseMovement()
        {
            try
            {
                // Experimenting with Facing
                //if (StyxWoW.Me.CurrentTarget == null)
                //{
                //    //WoWMovement.StopFace();
                //    StopMovement();
                //}

                // bunch of fuckoff checks to make sure we can even do the movements.
                if (StyxWoW.IsInGame == false) return;
                if (Me.IsValid == false) return;
                if (Me.CurrentTarget == null) return;
                if (Me.GotTarget == false) return;
                if (Me.Mounted) return;
                if (Me.IsDead) return;
                if (Me.CurrentTarget.IsPlayer == false) return;

                // Target stuff
                Target = Me.CurrentTarget;
                if (Target.Distance > 30) return;
                if (Target.IsDead) return;
                if (Target.IsFriendly) return;
                if (!Target.Attackable) return;

                // Do our movement stuff
                CheckFace();
                if (CheckMoving()) return;
                if (CheckStop()) return;
                CheckStrafe();
            }
            catch (System.Exception) { }
        }

        private static void CheckFace()
        {
            if (!WoWMovement.IsFacing)
            {
                WoWMovement.Face(Target.Guid);
            }
        }

        private static bool CheckMoving()
        {
            if (Target.Distance >= 3 && Target.IsMoving && !Me.MovementInfo.MovingForward)
            {
                //WoWMovement.Move(WoWMovement.MovementDirection.Forward);
                Navigator.MoveTo(StyxWoW.Me.CurrentTarget.Location);
                return true;
            }


            if (Target.Distance < 3 && Target.IsMoving && Me.MovementInfo.MovingForward)
            {
                //WoWMovement.MoveStop(WoWMovement.MovementDirection.Forward);
                StopMoving.InMeleeRangeOfUnit(StyxWoW.Me.CurrentTarget);
                return true;
            }
            
            if ((Me.MovementInfo.MovingStrafeRight || Me.MovementInfo.MovingStrafeLeft) && Me.CurrentTarget.Distance > 5)
                StopMovement(false, false, true, true);

            return false;
        }

        private static bool CheckStop()
        {

            if (Target.IsMoving) return false;
            float Distance = 3.2f;

            if (Target.Distance >= Distance && !Me.MovementInfo.MovingForward)
            {
                WoWMovement.Move(WoWMovement.MovementDirection.Forward, new TimeSpan(99, 99, 99));
                return true;
            }

            // To stop from spinning
            if (Target.Distance < 2 && Me.IsMoving && Me.MovementInfo.MovingForward)
            {
                WoWMovement.MoveStop(WoWMovement.MovementDirection.Forward);
            }

            return false;
        }

        private static void StopMovement()
        {
            if (Me.MovementInfo.MovingStrafeRight && !KeyboardPolling.IsKeyDown(Keys.D))
                WoWMovement.MoveStop(WoWMovement.MovementDirection.StrafeRight);

            if (Me.MovementInfo.MovingStrafeLeft && !KeyboardPolling.IsKeyDown(Keys.A))
                WoWMovement.MoveStop(WoWMovement.MovementDirection.StrafeLeft);

            if (Me.MovementInfo.MovingForward && !KeyboardPolling.IsKeyDown(Keys.W))
                WoWMovement.MoveStop(WoWMovement.MovementDirection.Forward);
        }

        private static void CheckStrafe()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                // Test
                //if (Me.Stunned) return;

                // Cancel all strafes - distance
                if (Me.MovementInfo.MovingStrafeRight && Target.Distance >= 2.5)
                {
                    WoWMovement.MoveStop(WoWMovement.MovementDirection.StrafeRight);
                    return;
                }

                if (Me.MovementInfo.MovingStrafeLeft && Target.Distance >= 2.5)
                {
                    WoWMovement.MoveStop(WoWMovement.MovementDirection.StrafeLeft);
                    return;
                }

                // Cancel all strafes - Angle out of range
                if (Me.MovementInfo.MovingStrafeRight && GetDegree <= 180 && GetDegree >= Cone)
                {
                    WoWMovement.MoveStop(WoWMovement.MovementDirection.StrafeRight);
                    return;
                }
                if (Me.MovementInfo.MovingStrafeLeft && GetDegree >= 180 && GetDegree <= (360 - Cone))
                {
                    WoWMovement.MoveStop(WoWMovement.MovementDirection.StrafeLeft);
                    return;
                }

                // Dont strafe if we are not close enough
                if (Target.Distance >= 5) return;


                // 180 > strafe right
                if (GetDegree >= 180 && GetDegree <= (360 - Cone) && !Me.MovementInfo.MovingStrafeRight)
                {
                    WoWMovement.Move(WoWMovement.MovementDirection.StrafeRight, new TimeSpan(99, 99, 99));
                    return;
                }

                // 180 < strafe left
                if (GetDegree <= 180 && GetDegree >= Cone && !Me.MovementInfo.MovingStrafeLeft)
                {
                    WoWMovement.Move(WoWMovement.MovementDirection.StrafeLeft, new TimeSpan(99, 99, 99));
                    return;
                }
            }
        }

        internal static double GetDegree
        {
            get
            {
                double d = Math.Atan2((Target.Y - Me.Y), (Target.X - Me.X));

                double r = d - Target.Rotation; 	  // substracting object rotation from absolute rotation
                if (r < 0)
                    r += (Math.PI * 2);

                return WoWMathHelper.RadiansToDegrees((float)r);
            }
        }

    }

    public static class StopMoving
    {
        internal static StopType Type { get; set; }
        internal static WoWPoint Point { get; set; }
        internal static WoWUnit Unit { get; set; }
        internal static double Range { get; set; }
        internal static SimpleBooleanDelegate StopNow { get; set; }

        public enum StopType
        {
            None = 0,
            AsSoonAsPossible,
            Location,
            RangeOfLocation,
            RangeOfUnit,
            MeleeRangeOfUnit,
            LosOfUnit
        }

        static StopMoving()
        {
            Clear();
        }

        public static void Clear()
        {
            Set(StopType.None, null, WoWPoint.Empty, 0, stop => false, null);
        }

        public static void Pulse()
        {
            if ( Type == StopType.None )
                return;

            bool stopMovingNow;
            try
            {
                stopMovingNow = StopNow(null);
            }
            catch
            {
                stopMovingNow = true;
            }

            if (stopMovingNow )
            {
                if (!StyxWoW.Me.IsMoving)
                    Logging.WriteDiagnostic("StopMoving: character already stopped, clearing stop {0} request", Type);
                    //Logger.WriteDebug(Color.White, "StopMoving: character already stopped, clearing stop {0} request", Type);
                else
                {
                    Navigator.PlayerMover.MoveStop();

                    string line = string.Format("StopMoving: {0}", Type);
                    if ( Type == StopType.Location)
                        line += string.Format(", within {0:F1} yds of {1}", StyxWoW.Me.Location.Distance(Point), Point);
                    else if ( Type == StopType.RangeOfLocation )
                        line += string.Format(", within {0:F1} yds of {1} @ {2:F1} yds", Range, Point, StyxWoW.Me.Location.Distance(Point));
                    else if ( Unit ==  null || !Unit.IsValid )
                        line += ", unit == null";
                    else if ( Type == StopType.LosOfUnit)
                        line += string.Format(", have LoSS of {0} @ {1:F1} yds", Unit.SafeName(), Unit.Distance );
                    else if ( Type == StopType.MeleeRangeOfUnit)
                        line += string.Format(", within melee range of {0} @ {1:F1} yds", Unit.SafeName(), Unit.Distance);
                    else if ( Type == StopType.RangeOfUnit)
                        line += string.Format(", within {0:F1} yds of {1} @ {2:F1} yds", Range, Unit.SafeName(), Unit.Distance);

                    Logging.WriteDiagnostic(line);
                    //Logger.WriteDebug(Color.White, line);
                }

                Clear();
            }
        }

        private static void Set( StopType type, WoWUnit unit, WoWPoint pt, double range, SimpleBooleanDelegate stop, SimpleBooleanDelegate and )
        {
            //if (MovementManager.IsMovementDisabled)
            //    return;

            Type = type;
            Unit = unit;
            Point = pt;
            Range = range;

            if (and == null)
                and = ret => true;

            StopNow = ret => stop(ret) && and(ret);
        }

        public static void AtLocation(WoWPoint pt, SimpleBooleanDelegate and = null)
        {
            Set( StopType.Location, null, pt, 0, at => StyxWoW.Me.Location.Distance(pt) <= 1, and);
        }

        public static void InRangeOfLocation(WoWPoint pt, double range, SimpleBooleanDelegate and = null)
        {
            Set(StopType.RangeOfLocation, null, pt, range, at => StyxWoW.Me.Location.Distance(pt) <= range, and);
        }

        public static void InRangeOfUnit(WoWUnit unit, double range, SimpleBooleanDelegate and = null)
        {
            Set(StopType.RangeOfUnit, unit, WoWPoint.Empty, range, at => Unit == null || !Unit.IsValid || Unit.Distance <= range, and);
        }

        public static void InMeleeRangeOfUnit(WoWUnit unit, SimpleBooleanDelegate and = null)
        {
            Set(StopType.RangeOfUnit, unit, WoWPoint.Empty, 0, at => Unit == null || !Unit.IsValid || Movement.InMoveToMeleeStopRange(Unit), and);
        }

        public static void InLosOfUnit(WoWUnit unit, SimpleBooleanDelegate and = null)
        {
            Set(StopType.LosOfUnit, unit, WoWPoint.Empty, 0, at => Unit == null || !Unit.IsValid || Movement.InLineOfSpellSight(Unit), and);
        }

        public static void Now()
        {
            Clear();
            if (StyxWoW.Me.IsMoving)
            {
                Logging.WriteDiagnostic("StopMoving.Now: character already stopped, clearing stop {0} request", Type);
                //Logger.WriteDebug(Color.White, "StopMoving.Now: character already stopped, clearing stop {0} request", Type);
                Navigator.PlayerMover.MoveStop();
            }
        }

        public static void AsSoonAsPossible( SimpleBooleanDelegate and = null)
        {
            Set(StopType.AsSoonAsPossible, null, WoWPoint.Empty, 0, at => true, and);
        }
    }

    public delegate WoWPoint LocationRetriever(object context);

    public delegate float DynamicRangeRetriever(object context);
}