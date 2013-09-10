using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvancedAI.Helpers;
using CommonBehaviors.Actions;
using Styx;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Action = System.Action;

namespace AdvancedAI.Class.Monk.PvE
{
    class WindwalkerMonk
    {

        static LocalPlayer Me { get { return StyxWoW.Me; } }

        [Behavior(BehaviorType.Combat, WoWClass.Monk, WoWSpec.MonkWindwalker)]
        public static Composite WindwalkerCombat()
        {
            return new PrioritySelector(
                //new Decorator(ret => AdvancedAI.PvPRot,
                //    BrewmasterMonkPvP.CreateBMPvPCombat),
                                    /*Things to fix
                     * energy capping
                     * need to check healing spheres 
                     * chi capping? need to do more checking
                    */
                    Spell.Cast("Spear Hand Strike", ret => StyxWoW.Me.CurrentTarget.IsCasting && StyxWoW.Me.CurrentTarget.CanInterruptCurrentSpellCast),

                    Spell.WaitForCastOrChannel(),

                    //Detox
                    CreateDispelBehavior(),
                    //Healing Spheres need to work on
                    Spell.CastOnGround("Healing Sphere", on => Me.Location, ret => Me.HealthPercent <= 50),

                    //Tigerseye
                    Spell.Cast("Tigereye Brew", ret => Me.CachedHasAura("Tigereye Brew", 10)),

                    Spell.Cast("Energizing Brew", ret => Me.CurrentEnergy < 25),

                    // Execute if we can
                    Spell.Cast("Touch of Death", ret => Me.CurrentChi >= 3 && Me.CachedHasAura("Death Note")),

                    Spell.Cast("Tiger Palm", ret => Me.CurrentChi > 0 &&
                              (!Me.CachedHasAura("Tiger Power") || Me.CachedHasAura("Tiger Power") && Me.CachedGetAuraTimeLeft("Tiger Power") <= 3)),

                    //Need to do some Thinking on Adding Detox to My Self might use this pre fight not for sure yet
                    //Spell.Cast("Detox", on => Me, ret => 

                    Spell.Cast("Invoke Xuen, the White Tiger", ret => AdvancedAI.Burst),

                    Spell.Cast("Rising Sun Kick"),

                    //Spell ID 116740 = Tigerseye Brew the dmg buff part not the brewing
                    Spell.Cast("Fists of Fury", ret => Unit.NearbyUnfriendlyUnits.Count(u => u.IsWithinMeleeRange && Me.IsSafelyFacing(u)) >= 1 &&
                              !Me.HasAura("Energizing Brew") && Me.HasAura(116740) && Me.GetAuraTimeLeft(116740).TotalSeconds >= 4 &&
                              Me.EnergyPercent <= 65 && !PartyBuff.WeHaveBloodlust && !Me.IsMoving),

                    //Chi Talents
                    Spell.Cast("Chi Wave", ret => Me.EnergyPercent < 40),
                    //need to do math here and make it use 2 if im going to use it
                    //Spell.Cast("Zen Sphere", ret => !Me.HasAura("Zen Sphere")),

                    // free Tiger Palm or Blackout Kick... do before Jab
                    Spell.Cast("Blackout Kick", ret => Me.CachedHasAura("Combo Breaker: Blackout Kick")),

                    Spell.Cast("Tiger Palm", ret => Me.CachedHasAura("Combo Breaker: Tiger Palm")),

                    Spell.Cast("Spinning Crane Kick", ret => Unit.UnfriendlyUnits(8).Count() >= 4 && AdvancedAI.Aoe),

                    Spell.Cast("Expel Harm", ret => Me.CurrentChi <= 2 && Me.HealthPercent < 80),

                    Spell.Cast("Jab", ret => Me.CurrentChi <= 2),

                    // chi dump
                    Spell.Cast("Blackout Kick", ret => Me.CurrentChi >= 2 && SpellManager.Spells["Rising Sun Kick"].CooldownTimeLeft.TotalSeconds > 1),



                    new ActionAlwaysSucceed());
        }

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
