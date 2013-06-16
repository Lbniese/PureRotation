﻿using CommonBehaviors.Actions;
using AdvancedAI.Managers;

using Styx;
using Styx.CommonBot;
using Styx.TreeSharp;
using Action = Styx.TreeSharp.Action;
using System;
using Styx.Helpers;
using Styx.WoWInternals.World;
using Styx.WoWInternals;
using Styx.Common;

namespace AdvancedAI.Helpers
{
    internal static class Pet
    {

        public static Composite CreateSummonPet(string petName)
        {
            return new Decorator(
                ret => !StyxWoW.Me.GotAlivePet && PetManager.PetSummonAfterDismountTimer.IsFinished,
                new Sequence(
                    new Action(ret => PetManager.CallPet(petName)),
                    Helpers.Common.CreateWaitForLagDuration(),
                    new Wait(
                        5,
                        ret => StyxWoW.Me.GotAlivePet || !StyxWoW.Me.IsCasting,
                        new PrioritySelector(
                            new Decorator(
                                ret => StyxWoW.Me.IsCasting,
                                new Action(ret => SpellManager.StopCasting())),
                            new ActionAlwaysSucceed()))));
        }

        /// <summary>
        ///  Creates a behavior to cast a pet action by name of the pet spell on current target.
        /// </summary>
        /// <param name="action"> The name of the pet spell that will be casted. </param>
        /// <returns></returns>
        public static Composite CreateCastPetAction(string action)
        {
            return CreateCastPetAction(action, ret => true);
        }

        /// <summary>
        ///  Creates a behavior to cast a pet action by name of the pet spell on current target, if the extra conditions are met.
        /// </summary>
        /// <param name="action"> The name of the pet spell that will be casted. </param>
        /// <param name="extra"> Extra conditions that will be checked. </param>
        /// <returns></returns>
        public static Composite CreateCastPetAction(string action, SimpleBooleanDelegate extra)
        {
            return CreateCastPetActionOn(action, ret => StyxWoW.Me.CurrentTarget, extra);
        }

        /// <summary>
        /// Creates a behavior to cast a pet action by name of the pet spell on the specified unit.
        /// </summary>
        /// <param name="action"> The name of the pet spell that will be casted. </param>
        /// <param name="onUnit"> The unit to cast the spell on. </param>
        /// <returns></returns>
        public static Composite CreateCastPetActionOn(string action, UnitSelectionDelegate onUnit)
        {
            return CreateCastPetActionOn(action, onUnit, ret => true);
        }

        /// <summary>
        ///  Creates a behavior to cast a pet action by name of the pet spell on the specified unit, if the extra conditions are met.
        /// </summary>
        /// <param name="action"> The name of the pet spell that will be casted. </param>
        /// <param name="onUnit"> The unit to cast the spell on. </param>
        /// <param name="extra"> Extra conditions that will be checked. </param>
        /// <returns></returns>
        public static Composite CreateCastPetActionOn(string action, UnitSelectionDelegate onUnit, SimpleBooleanDelegate extra)
        {
            return new Decorator(
                ret => extra(ret) && PetManager.CanCastPetAction(action),
                new Action(ret => PetManager.CastPetAction(action, onUnit(ret))));
        }

        /// <summary>
        ///  Creates a behavior to cast a pet action by name of the pet spell on current target's location. (like Freeze of Water Elemental)
        /// </summary>
        /// <param name="action"> The name of the pet spell that will be casted. </param>
        /// <returns></returns>
        public static Composite CreateCastPetActionOnLocation(string action)
        {
            return CreateCastPetActionOnLocation(action, ret => true);
        }

        /// <summary>
        ///  Creates a behavior to cast a pet action by name of the pet spell on current target's location, if extra conditions are met
        ///  (like Freeze of Water Elemental)
        /// </summary>
        /// <param name="action"> The name of the pet spell that will be casted. </param>
        /// <param name="extra"> Extra conditions that will be checked. </param>
        /// <returns></returns>
        public static Composite CreateCastPetActionOnLocation(string action, SimpleBooleanDelegate extra)
        {
            return CreateCastPetActionOnLocation(action, ret => StyxWoW.Me.CurrentTarget.Location, extra);
        }

        /// <summary>
        ///  Creates a behavior to cast a pet action by name of the pet spell on specified location.  (like Freeze of Water Elemental)
        /// </summary>
        /// <param name="action"> The name of the pet spell that will be casted. </param>
        /// <param name="location"> The point to click. </param>
        /// <returns></returns>
        public static Composite CreateCastPetActionOnLocation(string action, LocationRetriever location)
        {
            return CreateCastPetActionOnLocation(action, location, ret => true);
        }

        /// <summary>
        ///  Creates a behavior to cast a pet action by name of the pet spell on specified location, if extra conditions are met
        ///  (like Freeze of Water Elemental)
        /// </summary>
        /// <param name="action"> The name of the pet spell that will be casted. </param>
        /// <param name="location"> The point to click. </param>
        /// <param name="extra"> Extra conditions that will be checked. </param>
        /// <returns></returns>
        public static Composite CreateCastPetActionOnLocation(string action, LocationRetriever location, SimpleBooleanDelegate extra)
        {
            return new Decorator(
                ret => StyxWoW.Me.GotAlivePet && extra(ret) && PetManager.CanCastPetAction(action),
                new Sequence(
                    new Action(ret => PetManager.CastPetAction(action)),

                    new WaitContinue(TimeSpan.FromMilliseconds(500),
                        ret => Spell.GetPendingCursorSpell != null, // && Spell.GetPendingOnCursor().Name == spell,
                        new ActionAlwaysSucceed()
                        ),

                    new Action(ret => SpellManager.ClickRemoteLocation(location(ret))),

                    // check for we are done via either success (no spell on cursor) or failure (cursor remains targeting)
                    new PrioritySelector(

                        // wait and if cursor clears, then Success!!!!
                        new Wait(TimeSpan.FromMilliseconds(500),
                            ret => Spell.GetPendingCursorSpell == null,
                            new ActionAlwaysSucceed()
                            ),

                        // otherwise cancel spell and fail ----
                        new Action(ret =>
                        {
                            Logging.Write("pet:/cancel {0} - click {1} failed?  distance={2:F1} yds, loss={3}, face={4}",
                                action,
                                location(ret),
                                StyxWoW.Me.Location.Distance(location(ret)),
                                GameWorld.IsInLineOfSpellSight(StyxWoW.Me.Pet.GetTraceLinePos(), location(ret)),
                                StyxWoW.Me.Pet.IsSafelyFacing(location(ret))
                                );
                            Lua.DoString("SpellStopTargeting()");
                            return RunStatus.Failure;
                        })
                        )
                    )
                );
        }
    }
}
