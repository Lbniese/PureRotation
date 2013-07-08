
using Styx;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals.WoWObjects;
using AdvancedAI.Helpers;
using System.Collections.Generic;
using System.Linq;
using Action = Styx.TreeSharp.Action;
using Styx.WoWInternals;
using AdvancedAI.Managers;

namespace AdvancedAI.Spec
{
    class BrewmasterMonk
    {       
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        private static double? _time_to_max;
        private static double? _EnergyRegen;
        private static double? _energy;

        public static Composite CreateBMCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        BrewmasterMonkPvP.CreateBMPvPCombat),
                    new Throttle(1,
                        new Action(context => ResetVariables())),
                    /*Things to fix
                     * energy capping - fixed (stole alex's code)
                     * need to check healing spheres 
                     * need to work on chi wave to get more dmg/healing out it
                     * chi capping? need to do more checking - fixed as far as i know
                     * IsCurrentTank() code does not work
                    */
                    Spell.Cast("Spear Hand Strike", ret => StyxWoW.Me.CurrentTarget.IsCasting && StyxWoW.Me.CurrentTarget.CanInterruptCurrentSpellCast),

                    new Decorator(ret => Me.CurrentTarget.IsBoss,
                        new PrioritySelector(
                    //hands and trinks
                    //new Action(ret => { UseTrinkets(); return RunStatus.Failure; }),
                    new Action(ret => { Item.UseWaist(); return RunStatus.Failure; }),
                    new Action(ret => { Item.UseHands(); return RunStatus.Failure; }))),

                    // Execute if we can
                    Spell.Cast("Touch of Death", ret => Me.CurrentChi >= 3 && Me.HasAura("Death Note")),

                    //// apply the Weakened Blows debuff. Keg Smash also generates allot of threat 
                    Spell.Cast(121253, ctx => Me.CurrentChi <= 3
                        && Clusters.GetCluster(Me, Unit.NearbyUnfriendlyUnits, ClusterType.Radius, 8).Any(u => !u.HasAura("Weakened Blows"))),

                    //PB, EB, and Guard are off the GCD
                    //!!!!!!!Purifying Brew !!!!!!!
                    Spell.Cast("Purifying Brew", ret => Me.CurrentChi > 0 && Me.HasAura("Heavy Stagger")),
                    Spell.Cast("Purifying Brew", ret => Me.CurrentChi > 0 && Me.HasAura("Moderate Stagger") && Me.HealthPercent <= 70 && (Me.GetAuraTimeLeft("Shuffle").TotalSeconds >= 6 || Me.CurrentChi > 2)),
                    Spell.Cast("Purifying Brew", ret => Me.CurrentChi > 0 && Me.HasAura("Light Stagger") && Me.HealthPercent < 40 && (Me.GetAuraTimeLeft("Shuffle").TotalSeconds >= 6 || Me.CurrentChi > 2)),

                    //Elusive Brew will made auto at lower stacks when I can keep up 80 to 90% up time this is just to keep from capping
                    Spell.Cast("Elusive Brew", ret => Me.HasAura("Elusive Brew", 12) && !Me.HasAura(115308)),
                    //Guard
                    Spell.Cast("Guard", ret => Me.CurrentChi >= 2 && Me.HasAura("Power Guard")),

                    //Blackout Kick might have to add guard back but i think its better to open with BK and get shuffle to build AP for Guard
                    Spell.Cast("Blackout Kick", ret => Me.CurrentChi >= 2 && !Me.HasAura("Shuffle")),

                    Spell.Cast("Tiger Palm", ret => Me.CurrentChi >= 2 && !Me.HasAura("Power Guard")),

                    Spell.Cast("Expel Harm", ret => Me.HealthPercent <= 35),

                    Spell.Cast("Breath of Fire", ret => Me.CurrentChi >= 3 && Me.HasAura("Shuffle") && Me.GetAuraTimeLeft("Shuffle").TotalSeconds > 6.5 && Me.CurrentTarget.HasMyAura("Dizzying Haze")),

                    //Detox
                    //Spell.Cast("Detox", on => DispelMe),
                    //Dispelling.CreateDispelBehavior(),
                    CreateDispelBehavior(),
                    //Spell.Cast("Detox", on => Me, ret => Dispelling.CanDispel(Me, DispelCapabilities.Disease) || Dispelling.CanDispel(Me, DispelCapabilities.Poison)),

                    Spell.Cast("Blackout Kick", ret => Me.CurrentChi >= 3),

                    //keg smash
                    Spell.Cast(121253),
                    //ret => Me.CurrentChi <= 3),

                    //Chi Talents
                    //need to do math here and make it use 2 if im going to use it
                    Spell.Cast("Chi Wave", on => Me, ret => Me.HealthPercent <= 85),
                    Spell.Cast("Zen Sphere", on => _tanking),

                    Spell.Cast("Expel Harm", ret => Me.HealthPercent <= 90),

                    //Healing Spheres need to work on not happy with this atm
                    //Spell.CastOnGround("Healing Sphere", on => Me.Location, ret => Me.HealthPercent <= 50 && Me.CurrentEnergy >= 60),

                    Spell.Cast("Spinning Crane Kick", ret => Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8 * 8) >= 5 && Spell.GetSpellCooldown("Keg Smash").TotalSeconds >= 2),

                    Spell.Cast("Jab", ret => ((Me.CurrentEnergy - 40) + (Spell.GetSpellCooldown("Keg Smash").TotalSeconds * EnergyRegen)) > 40),

                    //Spell.Cast("Jab", ret => Spell.GetSpellCooldown("Keg Smash").TotalSeconds >= (((40 - 0) * (1.0 / EnergyRegen)) / 1.6)),

                    //Spell.Cast("Jab", ret => time_to_max <= 1 || Spell.GetSpellCooldown("Keg Smash").TotalSeconds >= 3),

                    Spell.CastOnGround("Summon Black Ox Statue", on => Me.CurrentTarget.Location, ret => !Me.HasAura("Sanctuary of the Ox") && Me.CurrentTarget.IsBoss),
                    //dont like using this in auto to many probs with it
                    //Spell.Cast("Invoke Xuen, the White Tiger", ret => Me.CurrentTarget.IsBoss && IsCurrentTank()),

                    Spell.Cast("Tiger Palm", ret => Spell.GetSpellCooldown("Keg Smash").TotalSeconds >= 1)
                        );
            }
        }

        #region Target Tank Tracking
        //this code is not working
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

        #region Energy Crap

        protected static double EnergyRegen
        {
            get
            {
                if (!_EnergyRegen.HasValue)
                {
                    _EnergyRegen = Lua.GetReturnVal<float>("return GetPowerRegen()", 1);
                    return _EnergyRegen.Value;
                }
                return _EnergyRegen.Value;
            }
        }
        protected static double energy
        {
            get
            {
                if (!_energy.HasValue)
                {
                    _energy = Lua.GetReturnVal<int>("return UnitPower(\"player\");", 0);
                    return _energy.Value;
                }
                return _energy.Value;
            }
        }
        private static RunStatus ResetVariables()
        {
            _time_to_max = null;
            _energy = null;
            _EnergyRegen = null;
            return RunStatus.Failure;
        }
        protected static double time_to_max
        {
            get
            {
                if (!_time_to_max.HasValue)
                {
                    _time_to_max = (100 - energy) * (1.0 / EnergyRegen);
                    return _time_to_max.Value;
                }
                return _time_to_max.Value;
            }
        }
        #endregion

        #region Dispell
        public static WoWUnit _dispelMe
        {
            get
            {
                var _Dispel = HealerManager.Instance.TargetList.FirstOrDefault(u => u.IsMe && u.IsAlive && Dispelling.CanDispel(u));
                return _Dispel;
            }
        }
        #endregion

        public static  WoWUnit dispeltar
        {
            get
            {
                var dispelothers = (from unit in ObjectManager.GetObjectsOfType<WoWPlayer>(false)
                                    where unit.IsAlive
                                    where Dispelling.CanDispel(unit)
                                    select unit).OrderByDescending(u => u.HealthPercent).LastOrDefault();
                return dispelothers;
            }
        }

        public static Composite CreateDispelBehavior()
        {
            return new PrioritySelector(
                Spell.Cast("Detox", on => Me, ret => Dispelling.CanDispel(Me)),
                Spell.Cast("Detox", on => dispeltar, ret => Dispelling.CanDispel(dispeltar)));
        }

        public static Composite CreateBMBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        BrewmasterMonkPvP.CreateBMPvPBuffs));
            }
        }
    }
}
