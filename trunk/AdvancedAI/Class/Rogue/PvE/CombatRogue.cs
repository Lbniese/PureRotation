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
    class CombatRogue// : AdvancedAI
    {
        LocalPlayer Me { get { return StyxWoW.Me; } }
        public static Composite CreateCRCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        CombatRoguePvP.CreateCRPvPCombat)
                    //8	1.00	virmens_bite_potion,if=buff.bloodlust.react|target.time_to_die<40
                    //9	6.05	auto_attack
                    //A	0.00	kick
                    //B	1.96	preparation,if=!buff.vanish.up&cooldown.vanish.remains>60
                    //C	7.82	use_item,slot=hands,if=time=0|buff.shadow_blades.up
                    //D	2.96	berserking,if=time=0|buff.shadow_blades.up
                    //E	0.00	blade_flurry,if=active_enemies>=5
                    //F	6.05	ambush
                    //G	5.05	vanish,if=time>10&(combo_points<3|(talent.anticipation.enabled&anticipation_charges<3)|(buff.shadow_blades.down&(combo_points<4|(talent.anticipation.enabled&anticipation_charges<4))))&((talent.shadow_focus.enabled&buff.adrenaline_rush.down&energy<20)|(talent.subterfuge.enabled&energy>=90)|(!talent.shadow_focus.enabled&!talent.subterfuge.enabled&energy>=60))
                    //H	7.92	shadow_blades,if=!set_bonus.tier14_4pc_melee&time>5
                    //I	7.55	killing_spree,if=!set_bonus.tier14_4pc_melee&energy<35&buff.adrenaline_rush.down
                    //J	7.92	adrenaline_rush,if=!set_bonus.tier14_4pc_melee&(energy<35|buff.shadow_blades.up)
                    //K	0.00	shadow_blades,if=set_bonus.tier14_4pc_melee&((cooldown.killing_spree.remains>30.5&cooldown.adrenaline_rush.remains<=9)|(energy<35&(cooldown.killing_spree.remains=0|cooldown.adrenaline_rush.remains=0)))
                    //L	0.00	killing_spree,if=set_bonus.tier14_4pc_melee&((buff.shadow_blades.up&buff.adrenaline_rush.down&(energy<35|buff.shadow_blades.remains<=3.5))|(buff.shadow_blades.down&cooldown.shadow_blades.remains>30))
                    //M	0.00	adrenaline_rush,if=set_bonus.tier14_4pc_melee&buff.shadow_blades.up&(energy<35|buff.shadow_blades.remains<=15)
                    //N	14.33	slice_and_dice,if=buff.slice_and_dice.remains<2|(buff.slice_and_dice.remains<15&buff.bandits_guile.stack=11&combo_points>=4)
                    //O	0.00	marked_for_death,if=talent.marked_for_death.enabled&(combo_points=0&dot.revealing_strike.ticking)
                    //P	0.00	run_action_list,name=generator,if=combo_points<5|(talent.anticipation.enabled&anticipation_charges<=4&!dot.revealing_strike.ticking)
                    //Q	0.00	run_action_list,name=finisher,if=!talent.anticipation.enabled|buff.deep_insight.up|cooldown.shadow_blades.remains<=11|anticipation_charges>=4|(buff.shadow_blades.up&anticipation_charges>=3)
                    //R	0.00	run_action_list,name=generator,if=energy>60|buff.deep_insight.down|buff.deep_insight.remains>5-combo_points
                    //actions.finisher
                    //#	count	action,conditions
                    //S	14.30	rupture,if=ticks_remain<2&target.time_to_die>=26
                    //T	78.76	eviscerate
                    //actions.generator
                    //#	count	action,conditions
                    //U	19.29	revealing_strike,if=ticks_remain<2
                    //V	312.17	sinister_strike
                    );
            }
        }

        public static Composite CreateCRBuffs
        {
            get
            {
                return new PrioritySelector(
                new Decorator(ret => AdvancedAI.PvPRot,
                    CombatRoguePvP.CreateCRPvPBuffs));
            }
        }
    }
}
