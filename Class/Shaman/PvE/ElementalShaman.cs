using CommonBehaviors.Actions;
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

namespace AdvancedAI.Spec
{
    class ElementalShaman
    {
        LocalPlayer Me { get { return StyxWoW.Me; } }
        public static Composite CreateElSCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        ElementalShamanPvP.CreateElSPvPCombat)
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

        public static Composite CreateElSBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        ElementalShamanPvP.CreateElSPvPBuffs));
            }
        }
    }
}
