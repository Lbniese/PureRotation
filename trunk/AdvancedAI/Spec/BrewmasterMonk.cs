﻿using CommonBehaviors.Actions;
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
    class BrewmasterMonk
    {
        #region Initialize
        /// <summary>
        /// The name of this CombatRoutine
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name { get { return "The Truffle Shuffle by AI"; } }
        private static LocalPlayer Me { get { return StyxWoW.Me; } }




        /// <summary>
        /// The <see cref="T:Styx.WoWClass"/> to be used with this routine
        /// </summary>
        /// <value>
        /// The class.
        /// </value>
        public override WoWClass Class { get { return WoWClass.Monk; } }
        private Composite _combat, _buffs, _pull;
        public override Composite CombatBehavior { get { return _combat; } }
        public override Composite PreCombatBuffBehavior { get { return _buffs; } }
        public override Composite CombatBuffBehavior { get { return _buffs; } }
        public override Composite PullBehavior { get { return _combat; } }

        public override void Initialize()
        {
            _combat = CreateCombat();
            _buffs = CreateBuffs();
            _pull = CreateCombat();
        }
        #endregion

        #region Buffs
        Composite CreateBuffs()
        {
            return new Decorator(
                    ret => !Spell.IsCasting() && !Spell.IsGlobalCooldown(),
                    new PrioritySelector(

                                        ));
        }
        #endregion

        #region Combat
        Composite CreateCombat()
        {

            return new PrioritySelector(
                /*Things to fix
                 * energy capping
                 * need to check healing spheres 
                 * need to work on chi wave to get more dmg/healing out it
                 * chi capping? need to do more checking
                */
                Spell.Cast("Spear Hand Strike", ret => StyxWoW.Me.CurrentTarget.IsCasting && StyxWoW.Me.CurrentTarget.CanInterruptCurrentSpellCast),

                new Decorator(ret => Me.CurrentTarget.IsBoss && IsCurrentTank(),
                    new PrioritySelector(
                //hands and trinks
                //new Action(ret => { UseTrinkets(); return RunStatus.Failure; }),
                new Action(ret => { Item.UseBelt(); return RunStatus.Failure; }),
                new Action(ret => { Item.UseHands(); return RunStatus.Failure; }))),




                // Execute if we can
                Spell.Cast("Touch of Death", ret => Me.CurrentChi >= 3 && Me.HasAura("Death Note")),

                //// apply the Weakened Blows debuff. Keg Smash also generates allot of threat 
                Spell.Cast("Keg Smash", ctx => Me.CurrentChi <= 2
                    && Clusters.GetCluster(Me, Unit.NearbyUnfriendlyUnits, ClusterType.Radius, 8).Any(u => !u.HasAura("Weakened Blows"))),

                //Spell.Cast("Jab", ret => Me.CurrentEnergy >= 80 && Me.HealthPercent >= 35),

                //PB, EB, and Guard are off the GCD
                //!!!!!!!Purifying Brew !!!!!!!
                Spell.Cast("Purifying Brew", ret => Me.CurrentChi > 0 && Me.HasAura("Heavy Stagger")),
                Spell.Cast("Purifying Brew", ret => Me.CurrentChi > 0 && Me.HasAura("Moderate Stagger") && Me.HealthPercent <= 70 && (Me.GetAuraTimeLeft("Shuffle").TotalSeconds >= 6 || Me.CurrentChi > 2)),
                Spell.Cast("Purifying Brew", ret => Me.CurrentChi > 0 && Me.HasAura("Light Stagger") && Me.HealthPercent <= 40 && (Me.GetAuraTimeLeft("Shuffle").TotalSeconds >= 6 || Me.CurrentChi > 2)),

                //Elusive Brew will made autoit at lower stacks when I can keep up 80 to 90% up time this is just to keep from capping
                Spell.Cast("Elusive Brew", ret => Me.HasAura("Elusive Brew", 12)),
                //Guard
                Spell.Cast("Guard", ret => Me.CurrentChi >= 2 && Me.HasAura("Power Guard") && IsCurrentTank()),

                //Blackout Kick might have to add guard back but i think its better to open with BK and get shuffle to build AP for Guard
                Spell.Cast("Blackout Kick", ret => Me.CurrentChi >= 2 && !Me.HasAura("Shuffle")),

                Spell.Cast("Expel Harm", ret => Me.HealthPercent <= 35),

                Spell.Cast("Tiger Palm", ret => Me.CurrentChi >= 2 && !Me.HasAura("Power Guard")),

                Spell.Cast("Breath of Fire", ret => Me.CurrentChi >= 3 && Me.HasAura("Shuffle") && Me.GetAuraTimeLeft("Shuffle").TotalSeconds >= 6 && Me.CurrentTarget.HasMyAura("Dizzying Haze")),

                //Detox
                Spell.Cast("Detox", on => Me, ret => Dispelling.CanDispel(Me, DispelCapabilities.Disease) || Dispelling.CanDispel(Me, DispelCapabilities.Poison)),

                Spell.Cast("Blackout Kick", ret => Me.CurrentChi >= 3),

                // apply the Weakened Blows debuff. Keg Smash also generates allot of threat 
                Spell.Cast("Keg Smash", ctx => Me.CurrentChi <= 2),

                Spell.Cast("Jab", ret => Me.CurrentEnergy >= 76 && Me.HealthPercent >= 35 && Me.CurrentChi <= 3),

                //Chi Talents
                //need to do math here and make it use 2 if im going to use it
                //Spell.Cast("Zen Sphere", ret => !Me.HasAura("Zen Sphere")),
                Spell.Cast("Chi Wave", on => Me, ret => Me.HealthPercent <= 85),
                Spell.Cast("Zen Sphere", on => _tanking),

                Spell.Cast("Expel Harm", ret => Me.HealthPercent <= 90),

                //Healing Spheres need to work on
                Spell.CastOnGround("Healing Sphere", on => Me.Location, ret => Me.HealthPercent <= 50 && Me.CurrentEnergy >= 75),

                Spell.Cast("Spinning Crane Kick", ret => Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8 * 8) >= 5 && SpellManager.Spells["Keg Smash"].CooldownTimeLeft.TotalSeconds > 2),

                Spell.Cast("Jab", ret => SpellManager.Spells["Keg Smash"].CooldownTimeLeft.TotalSeconds > 3 && Me.CurrentChi <= 3),

                //dont like using this in auto to many probs with it
                //Spell.Cast("Invoke Xuen, the White Tiger", ret => Me.CurrentTarget.IsBoss && IsCurrentTank()),

                Spell.Cast("Tiger Palm", ret => Me.CurrentChi <= 3 && Me.CurrentEnergy <= 75 && SpellManager.Spells["Keg Smash"].CooldownTimeLeft.TotalSeconds > 1)
                    );
        }
        #endregion

        #region Target Tank Tracking
        // So, this code is just to track who the current tank is on the mob we're looking at.
        // Sometimes using threat is fine, sometimes the boss switches targets to cast an ability.
        // We want to ensure that we're the ones with threat. 
        static bool IsCurrentTank()
        {
            return StyxWoW.Me.CurrentTarget.ThreatInfo.TargetGuid == StyxWoW.Me.Guid;
        }

        static readonly HashSet<uint> IgnoreInterruptMobs = new HashSet<uint>
        {

        };
        #endregion



        #region Zen Heals
        public static WoWUnit _tanking
        {
            get
            {
                var _tank = Group.Tanks.FirstOrDefault(u => StyxWoW.Me.CurrentTarget.ThreatInfo.TargetGuid == u.Guid && u.HealthPercent < 90 && u.Distance < 40);
                return _tank;
            }
        }
        #endregion
    }
}