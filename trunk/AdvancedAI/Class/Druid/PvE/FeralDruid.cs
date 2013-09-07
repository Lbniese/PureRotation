using CommonBehaviors.Actions;
using Styx;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals.WoWObjects;
using AdvancedAI.Helpers;
using AdvancedAI.Managers;
using Action = Styx.TreeSharp.Action;

namespace AdvancedAI.Spec
{
    class FeralDruid
    {      
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        static WoWUnit healtarget { get { return HealerManager.FindLowestHealthTarget(); } }
        private const int DreamofCenarius = 108381;
        private const int NaturesSwiftness = 132158;
        private const int HealingTouch = 5185;
        /*Had some issues here and had to use spell ids... so heres the id and the spell
         * 132158 = Nature's Swiftness
         * 5185 = Healing Touch
         * 108381 = Dream of Cenarius (Damage part)
        */

        [Behavior(BehaviorType.Combat, WoWClass.Druid, WoWSpec.DruidFeral)]
        public static Composite FeralCombat()
        {
            return new PrioritySelector(
                // Interrupt please.
                Spell.Cast("Skull Bash", ret => StyxWoW.Me.CurrentTarget.IsCasting && StyxWoW.Me.CurrentTarget.CanInterruptCurrentSpellCast),
                Spell.WaitForCastOrChannel(),
                new Decorator(ret => Me.CurrentTarget != null && (!Me.CurrentTarget.IsWithinMeleeRange || Me.IsCasting || SpellManager.GlobalCooldown),
                    new ActionAlwaysSucceed()),

                Spell.Cast("Feral Spirit", ret => AdvancedAI.Burst),
                new Throttle(Spell.Cast("Nature's Vigil", ret => Me.HasAura("Berserk"))),
                Spell.Cast("Incarnation", ret => Me.HasAura("Berserk")),
                Spell.CastOnGround("Force of Nature",
                                    u => (Me.CurrentTarget ?? Me).Location,
                                    ret => StyxWoW.Me.CurrentTarget != null
                                    && StyxWoW.Me.CurrentTarget.Distance < 40),
                Spell.Cast(5185, ret => Me.HasAura("Predatory Swiftness") && Me.GetAuraTimeLeft("Predatory Swiftness").TotalSeconds <= 1.5 && !Me.HasAura(108381)),
                Spell.Cast("Savage Roar", ret => !StyxWoW.Me.HasAura("Savage Roar")),
                Spell.Cast("Faerie Fire", ret => !Me.CurrentTarget.HasAura("Weakened Armor", 3)),
                //healing_touch,if=buff.predatory_swiftness.up&(combo_points>=4|(set_bonus.tier15_2pc_melee&combo_points>=3))&buff.dream_of_cenarius_damage.stack<2
                Spell.Cast(5185, ret => StyxWoW.Me.HasAura("Predatory Swiftness") && StyxWoW.Me.ComboPoints >= 4 && (Me.HasAura(DreamofCenarius) && Me.CachedStackCount(DreamofCenarius) <= 1 || !Me.HasAura(DreamofCenarius))),
                Spell.Cast(5185,  ret => Me.HasAura("Nature's Swiftness")),
                //use_item,name=eternal_blossom_grips,sync=tigers_fury
                new Decorator(ret => Me.HasAura("Tiger's Fury"),
                    new PrioritySelector(
                        new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),
                        new Action(ret => { Item.UseTrinkets(); return RunStatus.Failure; }))),
                Spell.Cast("Tiger's Fury", ret => Me.EnergyPercent <= 35 && !Me.ActiveAuras.ContainsKey("Clearcasting") && !Me.HasAura("Berserk")),
                Spell.Cast("Berserk", ret => Me.HasAura("Tiger's Fury") && AdvancedAI.Burst),
                Spell.Cast("Ferocious Bite", ret => Me.ComboPoints >= 1 && Me.CurrentTarget.HasAura("Rip") && (Me.CurrentTarget.GetAuraTimeLeft("Rip").TotalSeconds <= 3 && Me.CurrentTarget.HealthPercent <= 25)),
                Spell.Cast("Thrash", ret => Me.CurrentTarget.TimeToDeath() >= 6 && Me.ActiveAuras.ContainsKey("Clearcasting") && Me.CurrentTarget.GetAuraTimeLeft("Thrash").TotalSeconds <= 3),
                Spell.Cast("Ferocious Bite", ret => Me.ComboPoints >= 5 && Me.CurrentTarget.TimeToDeath() <= 4 || Me.CurrentTarget.TimeToDeath() <= 1 && Me.ComboPoints >= 3),
                Spell.Cast("Savage Roar", ret => Me.HasAuraExpired("Savage Roar", 3) && Me.ComboPoints == 0 && Me.CurrentTarget.HealthPercent <= 25),
                Spell.Cast(NaturesSwiftness, ret => !Me.HasAura(DreamofCenarius) && !Me.HasAura("Predatory Swiftness") && Me.ComboPoints >= 4 && Me.CurrentTarget.HealthPercent <= 25),
                Spell.Cast("Rip", ret => Me.ComboPoints == 5 && Me.HasAura(DreamofCenarius) && Me.CurrentTarget.HealthPercent <= 25 && Me.CurrentTarget.TimeToDeath() >= 30),
                //pool_resource,wait=0.25,if=combo_points>=5&dot.rip.ticking&target.health.pct<=25&((energy<50&buff.berserk.down)|(energy<25&buff.berserk.remains>1))
                //PoolinResources(),
                // Spell.Cast("Rip", ret => Me.ComboPoints == 5 && !Me.CurrentTarget.HasMyAura("Rip")),
                Spell.Cast("Ferocius Bite", ret => Me.ComboPoints >= 5 && Me.CurrentTarget.HasMyAura("Rip") && Me.CurrentTarget.HealthPercent <= 25 && (Me.ComboPoints >= 5 && Me.CurrentTarget.HasMyAura("Rip") && Me.CurrentTarget.HealthPercent <= 25 && ((Me.CurrentEnergy < 50 && !Me.HasAura("Berserk")) || (Me.CurrentEnergy < 25 && Me.GetAuraTimeLeft("Berserk").TotalSeconds > 1)))),
                Spell.Cast("Rip", ret => Me.ComboPoints == 5 && (Me.CurrentTarget.HasMyAura("Rip") && Me.CurrentTarget.GetAuraTimeLeft("Rip").TotalSeconds <= 3 || !Me.CurrentTarget.HasMyAura("Rip")) && Me.HasAura(DreamofCenarius)),
                Spell.Cast(NaturesSwiftness, ret => !Me.HasAura(DreamofCenarius) && !Me.HasAura("Predatory Swiftness") && Me.ComboPoints >= 4 && Me.CurrentTarget.GetAuraTimeLeft("Rip").TotalSeconds <= 3),
                Spell.Cast("Rip", ret => Me.ComboPoints == 5 && Me.CurrentTarget.TimeToDeath() >= 6 && Me.CurrentTarget.HasAuraExpired("Rip", 2) && (Me.HasAura("Berserk") || Me.CurrentTarget.GetAuraTimeLeft("Rip").TotalSeconds + 1.9 <= SpellManager.Spells["Tiger's Fury"].CooldownTimeLeft.TotalSeconds)),
                Spell.Cast("Savage Roar", ret => Me.HasAuraExpired("Savage Roar", 3) && Me.ComboPoints == 0 && Me.GetAuraTimeLeft("Savage Roar").TotalSeconds + 2 <= Me.CurrentTarget.GetAuraTimeLeft("Rip").TotalSeconds),
                Spell.Cast("Savage Roar", ret => Me.HasAuraExpired("Savage Roar", 6) && Me.ComboPoints >= 5 && Me.GetAuraTimeLeft("Savage Roar").TotalSeconds + 2 <= Me.CurrentTarget.GetAuraTimeLeft("Rip").TotalSeconds),
                //pool_resource,wait=0.25,if=combo_points>=5&((energy<50&buff.berserk.down)|(energy<25&buff.berserk.remains>1))&dot.rip.remains>=6.5
                //PoolResources(),
                Spell.Cast("Ferocious Bite", ret => Me.ComboPoints >= 5 && Me.CurrentTarget.GetAuraTimeLeft("Rip").TotalSeconds >= 6 && (Me.ComboPoints >= 5 && ((Me.CurrentEnergy < 50 && !Me.HasAura("Berserk")) || (Me.CurrentEnergy < 25 && Me.GetAuraTimeLeft("Berserk").TotalSeconds > 1)) && Me.CurrentTarget.GetAuraTimeLeft("Rip", true).TotalSeconds >= 6.5)),
                Spell.Cast("Rake", ret => Me.CurrentTarget.GetAuraTimeLeft("Rake").TotalSeconds <= 9 && Me.HasAura(DreamofCenarius)),
                Spell.Cast("Rake", ret => Me.CurrentTarget.GetAuraTimeLeft("Rake").TotalSeconds <= 3),
                //pool_resource,wait=0.25,for_next=1
                Spell.Cast("Thrash", ret => Me.CurrentTarget.GetAuraTimeLeft("Rake").TotalSeconds <= 3 && Me.CurrentTarget.TimeToDeath() >= 6 && (Me.CurrentTarget.GetAuraTimeLeft("Rake").TotalSeconds >= 4 || Me.HasAura("Berserk"))),
                Spell.Cast("Thrash", ret => Me.CurrentTarget.GetAuraTimeLeft("Rake").TotalSeconds <= 3 && Me.CurrentTarget.TimeToDeath() >= 6 && Me.ComboPoints == 5),
                Spell.Cast("Shred", ret => Me.ActiveAuras.ContainsKey("Clearcasting") && Me.CurrentTarget.MeIsSafelyBehind || Me.ActiveAuras.ContainsKey("Clearcasting") && Me.HasAnyAura("Tiger's Fury", "Berserk")),
                Spell.Cast("Shred", ret => Me.HasAura("Berserk")),
                Spell.Cast("Mangle", ret => Me.ComboPoints <= 5 && Me.CurrentTarget.GetAuraTimeLeft("Rip").TotalSeconds <= 3 || Me.ComboPoints == 0 && Me.HasAuraExpired("Savage Roar", 2)),
                Spell.Cast("Shred", ret => (Me.CurrentTarget.MeIsSafelyBehind || (TalentManager.HasGlyph("Shred") && (Me.HasAnyAura("Tiger's Fury", "Berserk"))))),
                Spell.Cast("Mangle", ret => !Me.CurrentTarget.MeIsBehind));

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
