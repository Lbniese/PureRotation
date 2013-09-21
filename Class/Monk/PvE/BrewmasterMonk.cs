using System.Linq;
using AdvancedAI.Helpers;
using CommonBehaviors.Actions;
using Styx;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;

namespace AdvancedAI.Class.Monk.PvE
{
    class BrewmasterMonk
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        private static double? _time_to_max;
        private static double? _EnergyRegen;
        private static double? _energy;
        private const int KegSmash = 121253;
        private const int ElusiveBrew = 115308;

        [Behavior(BehaviorType.Combat, WoWClass.Monk, WoWSpec.MonkBrewmaster)]
        public static Composite BrewmasterCombat()
        {
            return new PrioritySelector(
                //new Decorator(ret => AdvancedAI.PvPRot,
                //    BrewmasterMonkPvP.CreateBMPvPCombat),
                new Throttle(1,
                    new Action(context => ResetVariables())),
                /*Things to fix
                 * energy capping 
                 * need to check healing spheres 
                 * IsCurrentTank() code does not work (this would be nice to have working)
                */
                Spell.Cast("Spear Hand Strike", ret => StyxWoW.Me.CurrentTarget.IsCasting && StyxWoW.Me.CurrentTarget.CanInterruptCurrentSpellCast),
                Spell.WaitForCastOrChannel(),
                Item.UsePotionAndHealthstone(40),
                //new Decorator(ret => Me.CurrentTarget.IsBoss(),
                //    new PrioritySelector(
                        //new Action(ret => { Item.UseTrinkets(); return RunStatus.Failure; }),
                        new Action(ret => { Item.UseWaist(); return RunStatus.Failure; }),
                        new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),
                // Execute if we can
                Spell.Cast("Touch of Death", ret => Me.CurrentChi >= 3 && Me.CachedHasAura("Death Note")),
                
                //stance stuff need to work on it more
                // cant get it to see what stance im in
                Spell.Cast("Stance of the Sturdy Ox", ret => IsCurrentTank() && !Me.HasAura("Stance of the Sturdy Ox")),
                new Decorator(ret => Me.HasAura("Stance of the Fierce Tiger"),
                    new PrioritySelector(
                    Spell.Cast("Tiger Palm", ret => !Me.CachedHasAura("Tiger Power")),
                    Spell.Cast("Chi Wave"),
                    Spell.Cast("Blackout Kick"),
                    Spell.Cast("Rushing Jade Wind", ret => Unit.UnfriendlyUnits(8).Count() >= 3),
                    Spell.Cast("Spinning Crane Kick", ret => Unit.UnfriendlyUnits(8).Count() >= 3),
                    Spell.Cast("Expel Harm", ret => Me.HealthPercent <= 35),
                    Spell.Cast("Jab", ret => Me.CurrentChi <= 4),
                    Spell.Cast("Tiger Palm"),
                    new ActionAlwaysSucceed()
                    )),
                new Decorator(ret => Me.HasAura("Stance of the Sturdy Ox"),//115069
                    new PrioritySelector(                  
                //// apply the Weakened Blows debuff. Keg Smash also generates allot of threat 
                Spell.Cast(KegSmash, ret => Me.CurrentChi <= 3 && Clusters.GetCluster(Me, Unit.NearbyUnfriendlyUnits, ClusterType.Radius, 8).Any(u => !u.CachedHasAura("Weakened Blows"))),

                Spell.CastOnGround("Summon Black Ox Statue", on => Me.Location, ret => !Me.HasAura("Sanctuary of the Ox") && AdvancedAI.UsefulStuff),

                //PB, EB, and Guard are off the GCD
                //!!!!!!!Purifying Brew !!!!!!!
                Spell.Cast("Purifying Brew", ret => Me.CachedHasAura("Purifier") && (Me.CachedGetAuraTimeLeft("Purifier") <= 1) || Me.CachedHasAura("Moderate Stagger") || Me.CachedHasAura("Heavy Stagger")),
                new Decorator(ret => Me.CurrentChi > 0,
                    new PrioritySelector(
                        Spell.Cast("Purifying Brew", ret => Me.CachedHasAura("Heavy Stagger")),
                        new Decorator(ret => (Me.CachedGetAuraTimeLeft("Shuffle") >= 6 || Me.CurrentChi > 2),
                            new PrioritySelector(
                                Spell.Cast("Purifying Brew", ret => Me.CachedHasAura("Moderate Stagger") && Me.HealthPercent <= 70),
                                Spell.Cast("Purifying Brew", ret => Me.CachedHasAura("Light Stagger") && Me.HealthPercent < 40))))),
                
                //Elusive Brew will made auto at lower stacks when I can keep up 80 to 90% up time this is just to keep from capping
                Spell.Cast("Elusive Brew", ret => Me.CachedHasAura("Elusive Brew", 12) && !Me.CachedHasAura(ElusiveBrew)),

                //Guard
                Spell.Cast("Guard", ret => Me.CurrentChi >= 2 && Me.CachedHasAura("Power Guard")),
                //Blackout Kick might have to add guard back but i think its better to open with BK and get shuffle to build AP for Guard
                Spell.Cast("Blackout Kick", ret => Me.CurrentChi >= 2 && !Me.CachedHasAura("Shuffle")),
                Spell.Cast("Tiger Palm", ret => Me.CurrentChi >= 2 && !Me.CachedHasAura("Power Guard") || !Me.CachedHasAura("Tiger Power")),
                Spell.Cast("Expel Harm", ret => Me.HealthPercent <= 35),
                Spell.Cast("Breath of Fire", ret => Me.CurrentChi >= 3 && Me.CachedHasAura("Shuffle") && Me.CachedGetAuraTimeLeft("Shuffle") > 6.5 && Me.CurrentTarget.CachedHasAura("Dizzying Haze")),

                //Detox
                CreateDispelBehavior(),
                Spell.Cast("Blackout Kick", ret => Me.CurrentChi >= 3),
                Spell.Cast(KegSmash),

                //Chi Talents
                //need to do math here and make it use 2 if im going to use it
                Spell.Cast("Chi Wave"),
                //Spell.Cast("Chi Wave", on => Me, ret => Me.HealthPercent <= 85),
                Spell.Cast("Zen Sphere", on => _tanking),
                Spell.Cast("Expel Harm", ret => Me.HealthPercent <= 90),

                //Healing Spheres need to work on not happy with this atm
                //Spell.CastOnGround("Healing Sphere", on => Me.Location, ret => Me.HealthPercent <= 50 && Me.CurrentEnergy >= 60),

                new Decorator(ret => AdvancedAI.Aoe && Spell.GetSpellCooldown("Keg Smash").TotalSeconds >= 2,
                    new PrioritySelector(
                Spell.Cast("Rushing Jade Wind", ret => Unit.UnfriendlyUnits(8).Count() >= 3),
                Spell.Cast("Spinning Crane Kick", ret => Unit.UnfriendlyUnits(8).Count() >= 5))),

                Spell.Cast("Jab", ret => ((Me.CurrentEnergy - 40) + (Spell.GetSpellCooldown("Keg Smash").TotalSeconds * EnergyRegen)) > 40),

                //Spell.Cast("Jab", ret => Spell.GetSpellCooldown("Keg Smash").TotalSeconds >= (((40 - 0) * (1.0 / EnergyRegen)) / 1.6)),
                //Spell.Cast("Jab", ret => Me.CurrentEnergy >= 80 || Spell.GetSpellCooldown("Keg Smash").TotalSeconds >= 3),

                //dont like using this in auto to many probs with it
                //Spell.Cast("Invoke Xuen, the White Tiger", ret => Me.CurrentTarget.IsBoss && IsCurrentTank()),
                Spell.Cast("Tiger Palm", ret => Spell.GetSpellCooldown("Keg Smash").TotalSeconds >= 1 && Me.CurrentChi < 3 && Me.CurrentEnergy < 80),

                    new ActionAlwaysSucceed())));
        }

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

        #region Is Tank
        static bool IsCurrentTank()
        {
            return StyxWoW.Me.CurrentTarget.CurrentTargetGuid == StyxWoW.Me.Guid;
        }
        #endregion

        #region Dispelling
        public static WoWUnit dispeltar
        {
            get
            {
                var dispelothers = (from unit in ObjectManager.GetObjectsOfTypeFast<WoWPlayer>()
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
        #endregion
    }
}
