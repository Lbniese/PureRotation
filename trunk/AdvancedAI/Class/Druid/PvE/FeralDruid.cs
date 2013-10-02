using System.Linq;
using AdvancedAI.Helpers;
using AdvancedAI.Managers;
using CommonBehaviors.Actions;
using Styx;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;

namespace AdvancedAI.Class.Druid.PvE
{
    class FeralDruid
    {      
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        static WoWUnit healtarget { get { return HealerManager.FindLowestHealthTarget(); } }
        private const int DreamofCenarius = 108381;
        private const int NaturesSwiftness = 132158;
        private const int HealingTouch = 5185;
        private static double? _EnergyRegen;
        private static double? _time_to_max;
        private static double? _energy;

        /*Had some issues here and had to use spell ids... so heres the id and the spell
         * 132158 = Nature's Swiftness
         * 5185 = Healing Touch
         * 108381 = Dream of Cenarius (Damage part)
        */

        public static Composite FeralCombat()
        {
            return new PrioritySelector(
                // Interrupt please.
                Spell.Cast("Skull Bash", ret => Me.CurrentTarget.IsCasting && StyxWoW.Me.CurrentTarget.CanInterruptCurrentSpellCast),
                Spell.WaitForCastOrChannel(),
                new Decorator(ret => Me.CurrentTarget != null && (!Me.CurrentTarget.IsWithinMeleeRange || Me.IsCasting),
                              new ActionAlwaysSucceed()),

                new Throttle(1,
                    new Action(context => ResetVariables())),

                new Decorator(ret => Me.CachedHasAura("Tiger's Fury"),
                    new PrioritySelector(
                        new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),
                        new Action(ret => { Item.UseTrinkets(); return RunStatus.Failure; }),
                        Spell.Cast("Feral Spirit", ret => AdvancedAI.Burst))),
//~	3.18	ferocious_bite,if=dot.rip.ticking&dot.rip.remains<=3&target.health.pct<=25
                Spell.Cast("Ferocious Bite", ret => Me.CurrentTarget.CachedGetAuraTimeLeft("Rip") <= 3 && Me.CurrentTarget.HealthPercent <= 25),
//!	0.00	faerie_fire,if=debuff.weakened_armor.stack<3
                Spell.Cast("Faerie Fire", ret => !Me.CurrentTarget.CachedHasAura("Weakened Armor", 3)),
//"	33.73	healing_touch,if=talent.dream_of_cenarius.enabled&buff.predatory_swiftness.up&buff.dream_of_cenarius.down&(buff.predatory_swiftness.remains<1.5|combo_points>=4)
                new Throttle(2,
                    Spell.Cast(HealingTouch, /*on => healtarget, */
                                             ret => Me.CachedHasAura("Predatory Swiftness") && !Me.CachedHasAura(DreamofCenarius) && 
                                                    (Me.CachedGetAuraTimeLeft("Predatory Swiftness") <= 1.5 || Me.ComboPoints >= 4))),
//#	16.25	savage_roar,if=buff.savage_roar.remains<3
                Spell.Cast("Savage Roar", ret => Me.CachedGetAuraTimeLeft("Savage Roar") < 3 || !Me.CachedHasAura("Savage Roar")),
//%	14.85	tigers_fury,if=energy<=35&!buff.omen_of_clarity.react
                Spell.Cast("Tiger's Fury", ret => Me.EnergyPercent <= 35 && !Me.ActiveAuras.ContainsKey("Clearcasting")),
//&	2.92	berserk,if=buff.tigers_fury.up
                Spell.Cast("Berserk", ret => Me.CachedHasAura("Tiger's Fury") && AdvancedAI.Burst),
//*	1.00	rip,if=combo_points>=5&target.health.pct<=25&action.rip.tick_damage%dot.rip.tick_dmg>=1.15

//+	4.05	ferocious_bite,if=combo_points>=5&target.health.pct<=25&dot.rip.ticking
                Spell.Cast("Ferocious Bite", ret => Me.ComboPoints >= 5 && Me.CurrentTarget.HealthPercent <= 25 && Me.CurrentTarget.CachedHasAura("Rip")),
//,	14.16	rip,if=combo_points>=5&dot.rip.remains<2
                Spell.Cast("Rip", ret => Me.ComboPoints == 5 && (Me.CurrentTarget.CachedGetAuraTimeLeft("Rip") <= 2 || !Me.CurrentTarget.CachedHasAura("Rip"))),
//-	3.18	thrash_cat,if=buff.omen_of_clarity.react&dot.thrash_cat.remains<3
                Spell.Cast("Thrash", ret => Me.ActiveAuras.ContainsKey("Clearcasting") && (Me.CurrentTarget.CachedGetAuraTimeLeft("Thrash") < 3 || !Me.CurrentTarget.CachedHasAura("Thrash"))),
//:	56.31	rake,if=dot.rake.remains<3|action.rake.tick_damage>dot.rake.tick_dmg
                Spell.Cast("Rake", ret => Me.CurrentTarget.GetAuraTimeLeft("Rake").TotalSeconds <= 3 || !Me.CurrentTarget.CachedHasAura("Rake")),
//;	77.39	pool_resource,for_next=1

//<	22.45	thrash_cat,if=dot.thrash_cat.remains<3&(dot.rip.remains>=8&buff.savage_roar.remains>=12|buff.berserk.up|combo_points>=5)
                Spell.Cast("Thrash", ret => Me.CurrentTarget.CachedGetAuraTimeLeft("Thrash") < 3 && (Me.CurrentTarget.CachedGetAuraTimeLeft("Rip") >= 8 &&
                                           (Me.CachedGetAuraTimeLeft("Savage Roar") >= 12 || Me.CachedHasAura("Berserk") || Me.ComboPoints == 5))),
//=	129.32	pool_resource,if=combo_points>=5&!(energy.time_to_max<=1|(buff.berserk.up&energy>=25))&dot.rip.ticking

//>	6.49	ferocious_bite,if=combo_points>=5&dot.rip.ticking
                Spell.Cast("Ferocious Bite", ret => Me.ComboPoints >= 5 && Me.CurrentTarget.HasMyAura("Rip") && Me.CurrentTarget.CachedGetAuraTimeLeft("Rip") > 7 && Me.CachedGetAuraTimeLeft("Savage Roar") > 6),
//    6.78	rake,if=target.time_to_die-dot.rake.remains>3&action.rake.tick_damage*(dot.rake.ticks_remain+1)-dot.rake.tick_dmg*dot.rake.ticks_remain>action.mangle_cat.hit_damage

//actions.aoe+=/swipe_cat,if=buff.savage_roar.remains<=5.
//actions.aoe+=/swipe_cat,if=buff.tigers_fury.up|buff.berserk.up.
//actions.aoe+=/swipe_cat,if=cooldown.tigers_fury.remains<3
//actions.aoe+=/swipe_cat,if=buff.omen_of_clarity.react.
//actions.aoe+=/swipe_cat,if=energy.time_to_max<=1
                Spell.Cast("Swipe", ret => Unit.UnfriendlyUnits(8).Count() >= 2 &&
                    (Me.CachedGetAuraTimeLeft("Savage Roar") <= 5 || Me.ActiveAuras.ContainsKey("Clearcasting") ||
                    Me.CachedHasAura("Berserk") || Me.CachedHasAura("Tiger's Fury") || Spell.GetSpellCooldown("Tiger's Fury").TotalSeconds <= 3)),

//.	22.37	shred,if=(buff.omen_of_clarity.react|buff.berserk.up|energy.regen>=15)&buff.king_of_the_jungle.down
                Spell.Cast("Shred", ret => Me.CurrentTarget.MeIsSafelyBehind || Me.ActiveAuras.ContainsKey("Clearcasting") || Me.CachedHasAura("Berserk") || EnergyRegen >= 15),
//.	64.76	mangle_cat,if=buff.king_of_the_jungle.down
                Spell.Cast("Mangle"));

#region Old dps
//                new Throttle(Spell.Cast("Nature's Vigil", ret => Me.CachedHasAura("Berserk"))),
//                Spell.Cast("Incarnation", ret => Me.CachedHasAura("Berserk")),
//                Spell.CastOnGround("Force of Nature",
//                                    u => (Me.CurrentTarget ?? Me).Location,
//                                    ret => StyxWoW.Me.CurrentTarget != null
//                                    && StyxWoW.Me.CurrentTarget.Distance < 40),
//                new Throttle(1,1,
//                Spell.Cast(HealingTouch, ret => (Me.CachedHasAura("Predatory Swiftness") && Me.GetAuraTimeLeft("Predatory Swiftness").TotalSeconds <= 1.5 && !Me.CachedHasAura(DreamofCenarius)) ||
//                                                (Me.CachedHasAura("Predatory Swiftness") && Me.ComboPoints >= 4 && (Me.CachedHasAura(DreamofCenarius) && Me.CachedStackCount(DreamofCenarius) <= 1 || !Me.CachedHasAura(DreamofCenarius))))),

//                Spell.Cast("Savage Roar", ret => !Me.CachedHasAura("Savage Roar")),
//                Spell.Cast("Faerie Fire", ret => !Me.CurrentTarget.CachedHasAura("Weakened Armor", 3)),
//                //healing_touch,if=buff.predatory_swiftness.up&(combo_points>=4|(set_bonus.tier15_2pc_melee&combo_points>=3))&buff.dream_of_cenarius_damage.stack<2
//                Spell.Cast(HealingTouch, ret => Me.CachedHasAura("Predatory Swiftness") && Me.ComboPoints >= 4 && (Me.CachedHasAura(DreamofCenarius) && Me.CachedStackCount(DreamofCenarius) <= 1 || !Me.CachedHasAura(DreamofCenarius))),
//                //Spell.Cast(HealingTouch, ret => Me.CachedHasAura("Nature's Swiftness")),
//                //use_item,name=eternal_blossom_grips,sync=tigers_fury
//                new Decorator(ret => Me.CachedHasAura("Tiger's Fury"),
//                    new PrioritySelector(
//                        new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),
//                        new Action(ret => { Item.UseTrinkets(); return RunStatus.Failure; }))),
//                Spell.Cast("Tiger's Fury", ret => Me.EnergyPercent <= 35 && !Me.ActiveAuras.ContainsKey("Clearcasting") && !Me.CachedHasAura("Berserk")),
//                Spell.Cast("Berserk", ret => Me.CachedHasAura("Tiger's Fury") && AdvancedAI.Burst),
//                Spell.Cast("Ferocious Bite", ret => Me.ComboPoints >= 1 && Me.CurrentTarget.CachedHasAura("Rip") && (Me.CurrentTarget.GetAuraTimeLeft("Rip").TotalSeconds <= 3 && Me.CurrentTarget.HealthPercent <= 25)),
//                Spell.Cast("Thrash", ret => Me.CurrentTarget.TimeToDeath() >= 6 && Me.ActiveAuras.ContainsKey("Clearcasting") && Me.CurrentTarget.GetAuraTimeLeft("Thrash").TotalSeconds <= 3),
//                Spell.Cast("Ferocious Bite", ret => Me.ComboPoints >= 5 && Me.CurrentTarget.TimeToDeath() <= 4 || Me.CurrentTarget.TimeToDeath() <= 1 && Me.ComboPoints >= 3),
//                Spell.Cast("Savage Roar", ret => Me.HasAuraExpired("Savage Roar", 3) && Me.ComboPoints == 0 && Me.CurrentTarget.HealthPercent <= 25),
//                //Spell.Cast(NaturesSwiftness, ret => !Me.CachedHasAura(DreamofCenarius) && !Me.CachedHasAura("Predatory Swiftness") && Me.ComboPoints >= 4 && Me.CurrentTarget.HealthPercent <= 25),
//                Spell.Cast("Rip", ret => Me.ComboPoints == 5 && Me.CachedHasAura(DreamofCenarius) && Me.CurrentTarget.HealthPercent <= 25 && Me.CurrentTarget.TimeToDeath() >= 30),
//                //pool_resource,wait=0.25,if=combo_points>=5&dot.rip.ticking&target.health.pct<=25&((energy<50&buff.berserk.down)|(energy<25&buff.berserk.remains>1))
//                //PoolinResources(),
//                // Spell.Cast("Rip", ret => Me.ComboPoints == 5 && !Me.CurrentTarget.HasMyAura("Rip")),
//                Spell.Cast("Ferocius Bite", ret => Me.ComboPoints >= 5 && Me.CurrentTarget.HasMyAura("Rip") && Me.CurrentTarget.HealthPercent <= 25 && (Me.ComboPoints >= 5 && Me.CurrentTarget.HasMyAura("Rip") && Me.CurrentTarget.HealthPercent <= 25 && ((Me.CurrentEnergy < 50 && !Me.CachedHasAura("Berserk")) || (Me.CurrentEnergy < 25 && Me.GetAuraTimeLeft("Berserk").TotalSeconds > 1)))),
//                Spell.Cast("Rip", ret => Me.ComboPoints == 5 && (Me.CurrentTarget.HasMyAura("Rip") && Me.CurrentTarget.GetAuraTimeLeft("Rip").TotalSeconds <= 3 || !Me.CurrentTarget.HasMyAura("Rip")) && Me.CachedHasAura(DreamofCenarius)),
//                //Spell.Cast(NaturesSwiftness, ret => !Me.CachedHasAura(DreamofCenarius) && !Me.CachedHasAura("Predatory Swiftness") && Me.ComboPoints >= 4 && Me.CurrentTarget.GetAuraTimeLeft("Rip").TotalSeconds <= 3),
//                Spell.Cast("Rip", ret => Me.ComboPoints == 5 && Me.CurrentTarget.TimeToDeath() >= 6 && Me.CurrentTarget.HasAuraExpired("Rip", 2) && (Me.CachedHasAura("Berserk") || Me.CurrentTarget.GetAuraTimeLeft("Rip").TotalSeconds + 1.9 <= SpellManager.Spells["Tiger's Fury"].CooldownTimeLeft.TotalSeconds)),
//                Spell.Cast("Savage Roar", ret => Me.HasAuraExpired("Savage Roar", 3) && Me.ComboPoints == 0 && Me.GetAuraTimeLeft("Savage Roar").TotalSeconds + 2 <= Me.CurrentTarget.GetAuraTimeLeft("Rip").TotalSeconds),
//                Spell.Cast("Savage Roar", ret => Me.HasAuraExpired("Savage Roar", 6) && Me.ComboPoints >= 5 && Me.GetAuraTimeLeft("Savage Roar").TotalSeconds + 2 <= Me.CurrentTarget.GetAuraTimeLeft("Rip").TotalSeconds),
//                //pool_resource,wait=0.25,if=combo_points>=5&((energy<50&buff.berserk.down)|(energy<25&buff.berserk.remains>1))&dot.rip.remains>=6.5
//                //PoolResources(),
//                Spell.Cast("Ferocious Bite", ret => Me.ComboPoints >= 5 && Me.CurrentTarget.GetAuraTimeLeft("Rip").TotalSeconds >= 6 && (Me.ComboPoints >= 5 && ((Me.CurrentEnergy < 50 && !Me.CachedHasAura("Berserk")) || (Me.CurrentEnergy < 25 && Me.GetAuraTimeLeft("Berserk").TotalSeconds > 1)) && Me.CurrentTarget.GetAuraTimeLeft("Rip", true).TotalSeconds >= 6.5)),
//                Spell.Cast("Rake", ret => Me.CurrentTarget.GetAuraTimeLeft("Rake").TotalSeconds <= 9 && Me.CachedHasAura(DreamofCenarius)),
//                Spell.Cast("Rake", ret => Me.CurrentTarget.GetAuraTimeLeft("Rake").TotalSeconds <= 3),
//                //pool_resource,wait=0.25,for_next=1
//                Spell.Cast("Thrash", ret => Me.CurrentTarget.GetAuraTimeLeft("Rake").TotalSeconds <= 3 && Me.CurrentTarget.TimeToDeath() >= 6 && (Me.CurrentTarget.GetAuraTimeLeft("Rake").TotalSeconds >= 4 || Me.CachedHasAura("Berserk"))),
//                Spell.Cast("Thrash", ret => Me.CurrentTarget.GetAuraTimeLeft("Rake").TotalSeconds <= 3 && Me.CurrentTarget.TimeToDeath() >= 6 && Me.ComboPoints == 5),
//                Spell.Cast("Shred", ret => Me.ActiveAuras.ContainsKey("Clearcasting") && Me.CurrentTarget.MeIsSafelyBehind || Me.ActiveAuras.ContainsKey("Clearcasting") && Me.HasAnyAura("Tiger's Fury", "Berserk")),
//                Spell.Cast("Shred", ret => Me.CachedHasAura("Berserk")),
//                Spell.Cast("Mangle", ret => Me.ComboPoints <= 5 && Me.CurrentTarget.GetAuraTimeLeft("Rip").TotalSeconds <= 3 || Me.ComboPoints == 0 && Me.HasAuraExpired("Savage Roar", 2)),
//                Spell.Cast("Shred", ret => (Me.CurrentTarget.MeIsSafelyBehind || (TalentManager.HasGlyph("Shred") && (Me.HasAnyAura("Tiger's Fury", "Berserk"))))),
//                Spell.Cast("Mangle", ret => !Me.CurrentTarget.MeIsBehind));
#endregion
        }

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

        private static RunStatus ResetVariables()
        {
            _time_to_max = null;
            _energy = null;
            _EnergyRegen = null;
            return RunStatus.Failure;
        }

        #region DruidTalents
        public enum DruidTalents
        {
            FelineSwiftness = 1,//Tier 1
            DisplacerBeast,
            WildCharge,
            NaturesSwiftness,//Tier 2
            Renewal,
            CenarionWard,
            FaerieSwarm,//Tier 3
            MassEntanglement,
            Typhoon,
            SouloftheForest,//Tier 4
            Incarnation,
            ForceofNature,
            DisorientingRoar,//Tier 5
            UrsolsVortex,
            MightyBash,
            HeartoftheWild,//Tier 6
            DreamofCenarius,
            NaturesVigil
        }
        #endregion
    }
}

