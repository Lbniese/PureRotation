﻿using System;
using System.Linq;


using Styx;
using Styx.Common;
using Styx.Common.Helpers;
using Styx.CommonBot;
using Styx.Helpers;
using Styx.TreeSharp;
using Action = Styx.TreeSharp.Action;
using Styx.WoWInternals;
using CommonBehaviors.Actions;
using AdvancedAI.Managers;

using Styx.WoWInternals.WoWObjects;

using System.Drawing;

namespace AdvancedAI.Helpers
{
    internal static class Common
    {
        private static LocalPlayer Me { get { return StyxWoW.Me; } }

        /// <summary>
        /// 
        /// </summary>
        public static bool UseLongCoolDownAbility
        {
            get
            {
                if (!Me.GotTarget)
                    return false;

                if (Me.GotTarget)
                    return Me.CurrentTarget.IsBoss();

                if (Me.CurrentTarget.IsPlayer)
                    return Me.CurrentTarget.TimeToDeath() > 3;

                if (Me.CurrentTarget.TimeToDeath() > 30)
                    return true;

                return Unit.NearbyUnitsInCombatWithMe.Count(u => u.Guid != Me.CurrentTargetGuid) >= 3;
            }
        }               

        /// <summary>
        ///  Creates a behavior to start shooting current target with the wand.
        /// </summary>
        /// <remarks>
        ///  Created 23/05/2011
        /// </remarks>
        /// <returns></returns>
        public static Composite CreateUseWand()
        {
            return CreateUseWand(ret => true);
        }

        /// <summary>
        ///  Creates a behavior to start shooting current target with the wand if extra conditions are met.
        /// </summary>
        /// <param name="extra"> Extra conditions to check to start shooting. </param>
        /// <returns></returns>
        public static Composite CreateUseWand(SimpleBooleanDelegate extra)
        {
#if USE_WANDS
            return new PrioritySelector(
                new Decorator(
                    ret => Item.HasWand && !StyxWoW.Me.IsWanding() && extra(ret),
                    new Action(ret => SpellManager.Cast("Shoot")))
                );
#else
            return new ActionAlwaysFail();
#endif
        }

        private static WoWUnit _unitInterrupt = null;

        /// <summary>Creates an interrupt spell cast composite. This attempts to use spells in order of range (shortest to longest).  
        /// behavior consists only of spells that apply to current toon based upon class, spec, and race
        /// </summary>
        public static Composite CreateInterruptBehavior()
        {
            Composite actionSelectTarget;
            if (Me.CurrentTarget != null)
                actionSelectTarget = new Action( 
                    ret => {
                        WoWUnit u = Me.CurrentTarget;
                        _unitInterrupt = IsInterruptTarget(u) ? u : null;
                        if (_unitInterrupt != null)
                            Logging.WriteDiagnostic("Possible Interrupt Target: {0} @ {1:F1} yds casting {2} #{3} for {4} ms", _unitInterrupt.SafeName(), _unitInterrupt.Distance, _unitInterrupt.CastingSpell.Name, _unitInterrupt.CastingSpell.Id, _unitInterrupt.CurrentCastTimeLeft.TotalMilliseconds );
                    }
                    );
            else // if ( SingularSettings.Instance.InterruptTarget == InterruptType.All )
            {
                actionSelectTarget = new Action( 
                    ret => { 
                        _unitInterrupt = Unit.NearbyUnfriendlyUnits.Where(u => IsInterruptTarget(u)).OrderBy( u => u.Distance ).FirstOrDefault();
                        if (_unitInterrupt != null)
                            Logging.WriteDiagnostic("Possible Interrupt Target: {0} @ {1:F1} yds casting {2} #{3} for {4} ms", _unitInterrupt.SafeName(), _unitInterrupt.Distance, _unitInterrupt.CastingSpell.Name, _unitInterrupt.CastingSpell.Id, _unitInterrupt.CurrentCastTimeLeft.TotalMilliseconds);
                        }
                    );
            }

            PrioritySelector prioSpell = new PrioritySelector();

            #region Pet Spells First!

            if (Me.Class == WoWClass.Warlock)
            {
                // this will be either a Optical Blast or Spell Lock
                prioSpell.AddChild( 
                    Spell.Cast( 
                        "Command Demon", 
                        on => _unitInterrupt, 
                        ret => _unitInterrupt != null 
                            && _unitInterrupt.Distance < 40                             
                        )
                    );
            }

            #endregion

            #region Melee Range

            if ( Me.Class == WoWClass.Paladin )
                prioSpell.AddChild( Spell.Cast("Rebuke", ctx => _unitInterrupt));

            if ( Me.Class == WoWClass.Rogue)
            {
                prioSpell.AddChild( Spell.Cast("Kick", ctx => _unitInterrupt));
                prioSpell.AddChild( Spell.Cast("Gouge", ctx => _unitInterrupt, ret => !_unitInterrupt.IsBoss() && Me.IsSafelyFacing(_unitInterrupt)));
            }

            if ( Me.Class == WoWClass.Warrior)
                prioSpell.AddChild( Spell.Cast("Pummel", ctx => _unitInterrupt));

            if ( Me.Class == WoWClass.Monk )
                prioSpell.AddChild( Spell.Cast("Spear Hand Strike", ctx => _unitInterrupt));

            if ( Me.Class == WoWClass.Druid)
            {
                // Spell.Cast("Skull Bash (Cat)", ctx => _unitInterrupt, ret => StyxWoW.Me.Shapeshift == ShapeshiftForm.Cat));
                // Spell.Cast("Skull Bash (Bear)", ctx => _unitInterrupt, ret => StyxWoW.Me.Shapeshift == ShapeshiftForm.Bear));
                prioSpell.AddChild( Spell.Cast("Skull Bash", ctx => _unitInterrupt, ret => StyxWoW.Me.Shapeshift == ShapeshiftForm.Bear || StyxWoW.Me.Shapeshift == ShapeshiftForm.Cat));
                prioSpell.AddChild( Spell.Cast("Mighty Bash", ctx => _unitInterrupt, ret => !_unitInterrupt.IsBoss() && _unitInterrupt.IsWithinMeleeRange));
            }

            if ( Me.Class == WoWClass.DeathKnight)
                prioSpell.AddChild( Spell.Cast("Mind Freeze", ctx => _unitInterrupt));

            if ( Me.Race == WoWRace.Pandaren )
                prioSpell.AddChild( Spell.Cast("Quaking Palm", ctx => _unitInterrupt));

            #endregion

            #region 8 Yard Range

            if ( Me.Race == WoWRace.BloodElf )
                prioSpell.AddChild(Spell.Cast("Arcane Torrent", ctx => _unitInterrupt, req => _unitInterrupt.Distance < 8 && !Unit.NearbyUnfriendlyUnits.Any(u => u.IsSensitiveDamage( 8f))));

            if ( Me.Race == WoWRace.Tauren)
                prioSpell.AddChild(Spell.Cast("War Stomp", ctx => _unitInterrupt, ret => _unitInterrupt.Distance < 8 && !_unitInterrupt.IsBoss() && !Unit.NearbyUnfriendlyUnits.Any(u => u.IsSensitiveDamage( 8f))));

            #endregion

            #region 10 Yards

            if (Me.Class == WoWClass.Paladin)
                prioSpell.AddChild( Spell.Cast("Hammer of Justice", ctx => _unitInterrupt));

            if (Me.Specialization == WoWSpec.DruidBalance )
                prioSpell.AddChild( Spell.Cast("Hammer of Justice", ctx => _unitInterrupt));

            if (Me.Class == WoWClass.Warrior) 
                prioSpell.AddChild( Spell.Cast("Disrupting Shout", ctx => _unitInterrupt));

            #endregion

            #region 25 yards

            if ( Me.Class == WoWClass.Shaman)
                prioSpell.AddChild( Spell.Cast("Wind Shear", ctx => _unitInterrupt, req => Me.IsSafelyFacing(_unitInterrupt)));

            #endregion

            #region 30 yards

            if (Me.Specialization == WoWSpec.PaladinProtection)
                prioSpell.AddChild( Spell.Cast("Avenger's Shield", ctx => _unitInterrupt));

            if (Me.Class == WoWClass.Shaman)
                // Gag Order only works on non-bosses due to it being a silence, not an interrupt!
                prioSpell.AddChild( Spell.Cast("Heroic Throw", ctx => _unitInterrupt, ret => TalentManager.HasGlyph("Gag Order") && !_unitInterrupt.IsBoss()));

            if ( Me.Class == WoWClass.Priest ) 
                prioSpell.AddChild( Spell.Cast("Silence", ctx => _unitInterrupt));

            if (Me.Class == WoWClass.DeathKnight)
                prioSpell.AddChild(Spell.Cast("Strangulate", ctx => _unitInterrupt));

            if (Me.Class == WoWClass.Mage)
                prioSpell.AddChild(Spell.Cast("Frostjaw", ctx => _unitInterrupt));

            #endregion

            #region 40 yards

            if ( Me.Class == WoWClass.Mage)
                prioSpell.AddChild( Spell.Cast("Counterspell", ctx => _unitInterrupt));

            if ( Me.Class == WoWClass.Hunter)
                prioSpell.AddChild( Spell.Cast("Silencing Shot", ctx => _unitInterrupt));

            if ( Me.Class == WoWClass.Druid)
                prioSpell.AddChild( Spell.Cast("Solar Beam", ctx => _unitInterrupt, ret => StyxWoW.Me.Shapeshift == ShapeshiftForm.Moonkin));

            if (Me.Specialization == WoWSpec.ShamanElemental || Me.Specialization == WoWSpec.ShamanEnhancement )
                prioSpell.AddChild( Spell.Cast("Solar Beam", ctx => _unitInterrupt, ret => true));

            #endregion

            return new Sequence(
                actionSelectTarget,               
                new Decorator(
                    ret => _unitInterrupt != null,
                    // majority of these are off GCD, so throttle all to avoid most fail messages
                    new Throttle( TimeSpan.FromMilliseconds(500), prioSpell )
                    )
                );
        }

        private static bool IsInterruptTarget(WoWUnit u)
        {
            if (u == null || !u.IsCasting)
                return false;

            if (!u.CanInterruptCurrentSpellCast)
                ;   // Logger.WriteDebug("IsInterruptTarget: {0} casting {1} but CanInterruptCurrentSpellCast == false", u.SafeName(), (u.CastingSpell == null ? "(null)" : u.CastingSpell.Name));
            else if (!u.InLineOfSpellSight)
                ;   // Logger.WriteDebug("IsInterruptTarget: {0} casting {1} but LoSS == false", u.SafeName(), (u.CastingSpell == null ? "(null)" : u.CastingSpell.Name));
            else if (u.CurrentCastTimeLeft.TotalMilliseconds < 250)
                ;
            else
                return true;

            return false;
        }


        /// <summary>
        /// Creates a dismount composite that only stops if we are flying.
        /// </summary>
        /// <param name="reason">The reason to dismount</param>
        /// <returns></returns>
        public static Composite CreateDismount(string reason)
        {
            return new Decorator(
                ret => StyxWoW.Me.Mounted,
                new Sequence(
                    new DecoratorContinue(ret => StyxWoW.Me.IsFlying,
                        new Sequence(
                            new DecoratorContinue(ret => StyxWoW.Me.IsMoving,
                                new Sequence(
                                    new Action(ret => Logging.WriteDiagnostic("Stopping to descend..." + (!string.IsNullOrEmpty(reason) ? (" Reason: " + reason) : string.Empty))),
                                    new Action(ret => WoWMovement.MoveStop()),
                                    new Wait( 1, ret => !StyxWoW.Me.IsMoving, new ActionAlwaysSucceed())
                                    )
                                ),
                            new Action( ret => Logging.WriteDiagnostic( "Descending to land..." + (!string.IsNullOrEmpty(reason) ? (" Reason: " + reason) : string.Empty))),
                            new Action(ret => WoWMovement.Move(WoWMovement.MovementDirection.Descend)),
                            new PrioritySelector(
                                new Wait( 1, ret => StyxWoW.Me.IsMoving, new ActionAlwaysSucceed()),
                                new Action( ret => Logging.WriteDiagnostic( "warning -- tried to descend but IsMoving == false ....!"))
                                ),
                            new WaitContinue(30, ret => !StyxWoW.Me.IsFlying, new ActionAlwaysSucceed()),
                            new DecoratorContinue( 
                                ret => StyxWoW.Me.IsFlying, 
                                new Action( ret => Logging.WriteDiagnostic( "error -- still flying -- descend appears to have failed....!"))
                                ),
                            new Action(ret => WoWMovement.MoveStop(WoWMovement.MovementDirection.Descend))
                            )
                        ), // and finally dismount. 
                    new Action(r => {
                        Logging.WriteDiagnostic( "Dismounting..." + (!string.IsNullOrEmpty(reason) ? (" Reason: " + reason) : string.Empty));
                        ShapeshiftForm shapeshift = StyxWoW.Me.Shapeshift;
                        if (StyxWoW.Me.Class == WoWClass.Druid && (shapeshift == ShapeshiftForm.FlightForm || shapeshift == ShapeshiftForm.EpicFlightForm))
                            Lua.DoString("RunMacroText('/cancelform')");
                        else
                            Lua.DoString("Dismount()");
                        })
                    )
                );
        }


        /// <summary>
        /// This is meant to replace the 'SleepForLagDuration()' method. Should only be used in a Sequence
        /// </summary>
        /// <returns></returns>
        public static Composite CreateWaitForLagDuration()
        {
            // return new WaitContinue(TimeSpan.FromMilliseconds((StyxWoW.WoWClient.Latency * 2) + 150), ret => false, new ActionAlwaysSucceed());
            return CreateWaitForLagDuration(ret => false);
        }

        /// <summary>
        /// Allows waiting for SleepForLagDuration() but ending sooner if condition is met
        /// </summary>
        /// <param name="orUntil">if true will stop waiting sooner than lag maximum</param>
        /// <returns></returns>
        public static Composite CreateWaitForLagDuration( CanRunDecoratorDelegate orUntil)
        {
            return new WaitContinue(TimeSpan.FromMilliseconds((StyxWoW.WoWClient.Latency * 2) + 150), orUntil, new ActionAlwaysSucceed());
        }

        #region Wait for Rez Sickness

        public static Composite CreateWaitForRessSickness()
        {
            return new Decorator(
                ret => StyxWoW.Me.HasAura("Resurrection Sickness"),
                new PrioritySelector(
                    new Throttle(TimeSpan.FromMinutes(1), new Action(r => Logging.Write("Waiting out Resurrection Sickness (expires in {0:F0} seconds", StyxWoW.Me.GetAuraTimeLeft("Resurrection Sickness", false).TotalSeconds))),
                    new Action(ret => { })
                    )
                );
        }

        #endregion      

    }
}
