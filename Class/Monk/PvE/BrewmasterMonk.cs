using System.Linq;
using AdvancedAI.Helpers;
using AdvancedAI.Managers;
using CommonBehaviors.Actions;
using Styx;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;

namespace AdvancedAI.Class.Monk.PvE
{
    static class BrewmasterMonk
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        private static double? _time_to_max;
        private static double? _EnergyRegen;
        private static double? _energy;
        private const int KegSmash = 121253;
        private const int ElusiveBrew = 115308;

        public static Composite BrewmasterCombat()
        {
            return new PrioritySelector(
                new Throttle(1,
                    new Action(context => ResetVariables())),
                /*Things to fix
                 * using glyph of expel harm to heal ppl dont want to have to page heal manger if i dont have to to keep it faster i guess
                */
                new Decorator(ret => !Me.Combat,
                    new ActionAlwaysSucceed()),
                Spell.Cast("Spear Hand Strike", ret => StyxWoW.Me.CurrentTarget.IsCasting && StyxWoW.Me.CurrentTarget.CanInterruptCurrentSpellCast),
                Spell.WaitForCastOrChannel(),
                Item.UsePotionAndHealthstone(40),
                new Action(ret => { Item.UseWaist(); return RunStatus.Failure; }),
                new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),

                // Execute if we can
                Spell.Cast("Touch of Death", ret => Me.CurrentChi >= 3 && Me.CachedHasAura("Death Note")),
                //stance stuff need to work on it more
                Spell.Cast("Stance of the Sturdy Ox", ret => IsCurrentTank() && !Me.HasAura("Stance of the Sturdy Ox")),

                new Decorator(ret => Me.HasAura("Stance of the Fierce Tiger"),
                    new PrioritySelector(
                    HealingSphereTank(),
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

                //// apply the Weakened Blows debuff. Keg Smash also generates allot of threat 
                Spell.Cast(KegSmash, ret => Me.CurrentChi <= 3 && Clusters.GetCluster(Me, Unit.NearbyUnfriendlyUnits, ClusterType.Radius, 8).Any(u => !u.CachedHasAura("Weakened Blows"))),

                OxStatue(),
                //Spell.CastOnGround("Summon Black Ox Statue", on => Me.Location, ret => !Me.HasAura("Sanctuary of the Ox") && AdvancedAI.UsefulStuff),

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

                Item.UsePotionAndHealthstone(40),

                //Elusive Brew will made auto at lower stacks when I can keep up 80 to 90% up time this is just to keep from capping
                Spell.Cast("Elusive Brew", ret => Me.CachedHasAura("Elusive Brew", 12) && !Me.CachedHasAura(ElusiveBrew)),

                //Guard
                Spell.Cast("Guard", ret => Me.CurrentChi >= 2 && Me.CachedHasAura("Power Guard")),
                //Blackout Kick might have to add guard back but i think its better to open with BK and get shuffle to build AP for Guard
                Spell.Cast("Blackout Kick", ret => Me.CurrentChi >= 2 && !Me.CachedHasAura("Shuffle") || Me.CachedHasAura("Shuffle") && Me.CachedGetAuraTimeLeft("Shuffle") < 6),
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
                Spell.Cast("Zen Sphere", on => Tanking),
                
                Spell.Cast("Expel Harm", on => EHtar, ret => Me.HealthPercent > 70 && TalentManager.HasGlyph("Targeted Expulsion")),
                Spell.Cast("Expel Harm", ret => Me.HealthPercent <= 70 && TalentManager.HasGlyph("Targeted Expulsion") || Me.HealthPercent < 85 && !TalentManager.HasGlyph("Targeted Expulsion")),

                //Healing Spheres need to work on not happy with this atm
                HealingSphere(),
                HealingSphereTank(),
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
                new ActionAlwaysSucceed());
        }

        public static Composite BrewmasterPreCombatBuffs()
        {
            return new PrioritySelector(
                PartyBuff.BuffGroup("Legacy of the Emperor"));
        }

        #region Zen Heals

        private static WoWUnit Tanking
        {
            get
            {
                var _tank = Group.Tanks.FirstOrDefault(u => StyxWoW.Me.CurrentTarget.ThreatInfo.TargetGuid == u.Guid && u.Distance < 40);
                return _tank;
            }
        }
        #endregion

        #region Energy Crap

        private static double EnergyRegen
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

        private static double energy
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

        private static double time_to_max
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

        #region OxStatue
        private static Composite OxStatue()
        {
            return new Decorator(ret => !Me.HasAura("Sanctuary of the Ox") && Me.IsInGroup() && AdvancedAI.UsefulStuff,
                new Action(ret =>
                {
                    var tpos = Me.CurrentTarget.Location;
                    var mpos = Me.Location;

                    SpellManager.Cast("Summon Black Ox Statue");
                    SpellManager.ClickRemoteLocation(mpos);
                }));
        }
        #endregion

        #region Healing Sphere
        private static Composite HealingSphere()
        {
            return new Decorator(ret => Me.HealthPercent <= 50 && Me.CurrentEnergy >= 60,
                new Action(ret =>
                {
                    var mpos = Me.Location;
                    
                    SpellManager.Cast("Healing Sphere");
                    SpellManager.ClickRemoteLocation(mpos);
                }));
        }
        #endregion

        #region Healing Sphere Other tank
        private static Composite HealingSphereTank()
        {
            return new Decorator(ret => !IsCurrentTank() && Tanking.HealthPercent <= 50 && AdvancedAI.UsefulStuff,
                new Action(ret =>
                {
                    var otpos = Tanking.Location;

                    SpellManager.Cast("Healing Sphere");
                    SpellManager.ClickRemoteLocation(otpos);
                }));
        }
        #endregion

        #region Is Tank
        static bool IsCurrentTank()
        {
            return StyxWoW.Me.CurrentTarget.CurrentTargetGuid == StyxWoW.Me.Guid;
        }
        #endregion

        #region Dispelling

        private static WoWUnit Dispeltar
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
        #endregion

        #region Expel Harm

        private static WoWUnit EHtar
        {
            get
            {
                var EHheal = (from unit in ObjectManager.GetObjectsOfTypeFast<WoWPlayer>()
                                    where unit.IsAlive
                                    where unit.Distance < 40
                                    where unit.HealthPercent < 80
                                    select unit).OrderByDescending(u => u.HealthPercent).LastOrDefault();
                return EHheal;
            }
        }

        private static Composite CreateDispelBehavior()
        {
            return new PrioritySelector(
                Spell.Cast("Detox", on => Me, ret => Dispelling.CanDispel(Me)),
                Spell.Cast("Detox", on => Dispeltar, ret => Dispelling.CanDispel(Dispeltar)));
        }
        #endregion
    }
}
