using AdvancedAI.Managers;
using Styx;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using AdvancedAI.Helpers;
using System.Linq;
using Action = Styx.TreeSharp.Action;

namespace AdvancedAI.Spec
{
    class ElementalShaman
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        public static Composite CreateElSCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        ElementalShamanPvP.CreateElSPvPCombat),

                Spell.WaitForCastOrChannel(),

                // Interrupt please.
                Spell.Cast("Wind Shear", ret => StyxWoW.Me.CurrentTarget.IsCasting && StyxWoW.Me.CurrentTarget.CanInterruptCurrentSpellCast),

                //mana
                Spell.Cast("Thunderstorm", ret => StyxWoW.Me.ManaPercent <= 83),

                // AE
                new Decorator
                    (ret => Unit.UnfriendlyUnitsNearTarget(10).Count() > 2 && AdvancedAI.Aoe,
                    CreateAoe()),

                        Spell.Cast("Unleash Elements", ret => TalentManager.IsSelected((int)ShamanTalents.UnleashedFury)),

                        //gloves and hands
                        new Action(ret => { Item.UseTrinkets(); return RunStatus.Failure; }),
                        new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),

                        Spell.Cast("Flame Shock", ret => !Me.CurrentTarget.HasMyAura("Flame Shock") || 
                            Me.CurrentTarget.HasMyAura("Flame Shock") && Me.CurrentTarget.GetAuraTimeLeft("Flame Shock", true).TotalSeconds < 6),

                        // Raid Cooldowns
                    new Decorator(ret => AdvancedAI.Burst,
                        new PrioritySelector(
                        Spell.Cast("Fire Elemental Totem"),
                        Spell.Cast("Stormlash Totem"),
                        Spell.Cast("Ascendance", ret => Me.CurrentTarget.GetAuraTimeLeft("Flame Shock", true).TotalSeconds > 15 && !Me.HasAura("Ascendance")),
                        Spell.Cast("Spiritwalker's Grace", ret => Me.HasAura("Ascendance") && StyxWoW.Me.IsMoving))),

                        //LvB while moving
                        Spell.Cast("Lava Burst", mov => false , on => Me.CurrentTarget, ret => Me.HasAura("Ascendance") || 
                            StyxWoW.Me.IsMoving && Me.HasAura("Ascendance") && StyxWoW.Me.HasAura("Spritwalker's Grace") || Me.HasAura("Lava Surge")),

                        Spell.Cast("Lava Burst"),
                        Spell.Cast("Elemental Blast", ret => !Me.HasAura("Ascendance")),
                        Spell.Cast("Earth Shock",
                            ret => Me.HasAura("Lightning Shield", 7)),
                        Spell.Cast("Earth Shock",
                            ret => Me.HasAura("Lightning Shield", 4) &&
                                   Me.CurrentTarget.GetAuraTimeLeft("Flame Shock", true).TotalSeconds > 6),

                        Spell.Cast("Searing Totem", ret => Me.GotTarget
                                   && Me.CurrentTarget.SpellDistance() < Totems.GetTotemRange(WoWTotem.Searing) - 2f
                                    && !Totems.Exist(WoWTotemType.Fire)),

                        Spell.Cast("Unleash Elements",
                            ret => Me.IsMoving
                                && !Me.HasAura("Spiritwalker's Grace")),

                        Spell.Cast("Chain Lightning", ret => Unit.UnfriendlyUnitsNearTarget(10f).Count() >= 2 && !Unit.UnfriendlyUnitsNearTarget(10f).Any(u => u.IsCrowdControlled())),

                        Spell.Cast("Lightning Bolt", ret => !Me.HasAura("Ascendance"))


                    //6	0.00	wind_shear
                    //7	0.00	bloodlust,if=target.health.pct<25|time>5
                    //8	0.00	stormlash_totem,if=!active&!buff.stormlash.up&(buff.bloodlust.up|time>=60)
                    //9	1.00	jade_serpent_potion,if=time>60&(pet.primal_fire_elemental.active|pet.greater_fire_elemental.active|target.time_to_die<=60)
                    //A	4.26	blood_fury,if=buff.bloodlust.up|buff.ascendance.up|((cooldown.ascendance.remains>10|level<87)&cooldown.fire_elemental_totem.remains>10)
                    //B	0.00	elemental_mastery,if=talent.elemental_mastery.enabled&(time>15&((!buff.bloodlust.up&time<120)|(!buff.berserking.up&!buff.bloodlust.up&buff.ascendance.up)|(time>=200&(cooldown.ascendance.remains>30|level<87))))
                    //C	0.00	ancestral_swiftness,if=talent.ancestral_swiftness.enabled&!buff.ascendance.up
                    //D	2.00	fire_elemental_totem,if=!active
                    //E	4.43	ascendance,if=active_enemies>1|(dot.flame_shock.remains>buff.ascendance.duration&(target.time_to_die<20|buff.bloodlust.up|time>=60)&cooldown.lava_burst.remains>0)
                    //F	0.00	run_action_list,name=single,if=active_enemies=1
                    //G	0.00	run_action_list,name=aoe,if=active_enemies>1
                    //actions.single
                    //#	count	action,conditions
                    //Q	7.93	use_item,name=gloves_of_the_witch_doctor,if=((cooldown.ascendance.remains>10|level<87)&cooldown.fire_elemental_totem.remains>10)|buff.ascendance.up|buff.bloodlust.up|totem.fire_elemental_totem.active
                    //R	0.00	unleash_elements,if=talent.unleashed_fury.enabled&!buff.ascendance.up
                    //S	0.00	spiritwalkers_grace,moving=1,if=buff.ascendance.up
                    //T	111.22	lava_burst,if=dot.flame_shock.remains>cast_time&(buff.ascendance.up|cooldown_react)
                    //U	13.61	flame_shock,if=ticks_remain<2
                    //V	29.13	elemental_blast,if=talent.elemental_blast.enabled
                    //W	21.39	earth_shock,if=buff.lightning_shield.react=buff.lightning_shield.max_stack
                    //X	4.32	earth_shock,if=buff.lightning_shield.react>3&dot.flame_shock.remains>cooldown&dot.flame_shock.remains<cooldown+action.flame_shock.tick_time
                    //Y	2.17	flame_shock,if=time>60&remains<=buff.ascendance.duration&cooldown.ascendance.remains+buff.ascendance.duration<duration
                    //Z	1.94	earth_elemental_totem,if=!active&cooldown.fire_elemental_totem.remains>=60
                    //a	5.87	searing_totem,if=cooldown.fire_elemental_totem.remains>20&!totem.fire.active
                    //b	0.00	spiritwalkers_grace,moving=1,if=((talent.elemental_blast.enabled&cooldown.elemental_blast.remains=0)|(cooldown.lava_burst.remains=0&!buff.lava_surge.react))|(buff.raid_movement.duration>=action.unleash_elements.gcd+action.earth_shock.gcd)
                    //c	175.65	lightning_bolt
                    );
            }
        }

        private static Composite CreateAoe()
        {
            return new PrioritySelector(

                        Spell.Cast("Unleash Elements", ret => TalentManager.IsSelected((int)ShamanTalents.UnleashedFury)),

                        Totems.CreateTotemsNormalBehavior(),

                        //gloves and hands                        
                        new Action(ret => { Item.UseTrinkets(); return RunStatus.Failure; }),
                        new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),

                        // only us earthquake if more than 4 tars but need to make it so it will do the rest even if less than 4 tars
                        Spell.CastOnGround("Earthquake", on => StyxWoW.Me.CurrentTarget.Location, ret => Unit.UnfriendlyUnitsNearTarget(10).Count() > 4),

                        Spell.Cast("Flame Shock", ret => !Me.CurrentTarget.HasMyAura("Flame Shock") ||
                            Me.CurrentTarget.HasMyAura("Flame Shock") && Me.CurrentTarget.GetAuraTimeLeft("Flame Shock", true).TotalSeconds < 6),

                        // Raid Cooldowns
                        Spell.Cast("Ascendance", ret => AdvancedAI.Burst && StyxWoW.Me.CurrentTarget.GetAuraTimeLeft("Flame Shock", true).TotalSeconds > 15 && !StyxWoW.Me.HasAura("Ascendance")),
                        Spell.Cast("Lava Beam", ret => Clusters.GetBestUnitForCluster(Unit.UnfriendlyUnitsNearTarget(15f), ClusterType.Chained, 12)),

                        //need to make it so it will only place it if the are 2 or more tar in range (8y) or have it move it using the talent totemic projection
                //Spell.Cast("Magma Totem"),

                        //LvB while moving
                        Spell.Cast("Lava Burst", mov => false, on => StyxWoW.Me.CurrentTarget, ret => StyxWoW.Me.HasAura("Ascendance") || StyxWoW.Me.IsMoving && StyxWoW.Me.HasAura("Ascendance") && StyxWoW.Me.HasAura("Spritwalker's Grace") || StyxWoW.Me.HasAura("Lava Surge")),

                        Spell.Cast("Lava Burst"),
                        Spell.Cast("Earth Shock",
                            ret => StyxWoW.Me.HasAura("Lightning Shield", 5) &&
                                   StyxWoW.Me.CurrentTarget.GetAuraTimeLeft("Flame Shock", true).TotalSeconds > 3),
                        Spell.Cast("Unleash Elements",
                            ret => StyxWoW.Me.IsMoving
                                && !StyxWoW.Me.HasAura("Spiritwalker's Grace")),

                        Spell.Cast("Chain Lightning", ret => Clusters.GetBestUnitForCluster(Unit.UnfriendlyUnitsNearTarget(15f), ClusterType.Chained, 12))


                );
        }

        public static Composite CreateElSBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        ElementalShamanPvP.CreateElSPvPBuffs));
            }
        }

        #region ShamanTalents
        public enum ShamanTalents
        {
            NaturesGuardian = 1,
            StoneBulwarkTotem,
            AstralShift,
            FrozenPower,
            EarthgrabTotem,
            WindwalkTotem,
            CallOfTheElements,
            TotemicRestoration,
            TotemicProjection,
            ElementalMastery,
            AncestralSwiftness,
            EchoOfTheElements,
            HealingTideTotem,
            AncestralGuidance,
            Conductivity,
            UnleashedFury,
            PrimalElementalist,
            ElementalBlast
        }
        #endregion
    }
}
