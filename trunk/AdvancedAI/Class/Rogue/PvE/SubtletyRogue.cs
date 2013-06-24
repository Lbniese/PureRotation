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
    class SubtletyRogue// : AdvancedAI
    {
        //public override WoWClass Class { get { return WoWClass.Rogue; } }
        //public override WoWSpec Spec { get { return WoWSpec.RogueSubtlety; } }
        LocalPlayer Me { get { return StyxWoW.Me; } }

        public static Composite CreateSRCombat
        {
            get
            {
                return new PrioritySelector(
                    //8	1.00	virmens_bite_potion,if=buff.bloodlust.react|target.time_to_die<40
                    //9	6.45	auto_attack
                    //A	0.00	kick
                    //B	7.73	use_item,slot=hands,if=buff.shadow_dance.up
                    //C	2.89	berserking,if=buff.shadow_dance.up
                    //D	3.04	shadow_blades
                    //E	11.14	premeditation,if=combo_points<3|(talent.anticipation.enabled&anticipation_charges<3)
                    //F	16.66	pool_resource,for_next=1
                    //G	40.82	ambush,if=combo_points<5|(talent.anticipation.enabled&anticipation_charges<3)
                    //H	14.26	pool_resource,for_next=1,extra_amount=75
                    //I	7.74	shadow_dance,if=energy>=75&buff.stealthed.down&buff.vanish.down&debuff.find_weakness.down
                    //J	0.04	pool_resource,for_next=1,extra_amount=45
                    //K	5.45	vanish,if=energy>=45&energy<=75&combo_points<=3&buff.shadow_dance.down&buff.master_of_subtlety.down&debuff.find_weakness.down
                    //L	0.00	marked_for_death,if=talent.marked_for_death.enabled&combo_points=0
                    //M	0.00	run_action_list,name=generator,if=talent.anticipation.enabled&anticipation_charges<4&buff.slice_and_dice.up&dot.rupture.remains>2&(buff.slice_and_dice.remains<6|dot.rupture.remains<4)
                    //N	0.00	run_action_list,name=finisher,if=combo_points=5
                    //O	0.00	run_action_list,name=generator,if=combo_points<4|energy>80|talent.anticipation.enabled
                    //P	0.00	run_action_list,name=pool
                    //actions.finisher
                    //#	count	action,conditions
                    //Q	10.70	slice_and_dice,if=buff.slice_and_dice.remains<4
                    //R	16.57	rupture,if=ticks_remain<2
                    //S	76.20	eviscerate
                    //T	0.00	run_action_list,name=pool
                    //actions.generator
                    //#	count	action,conditions
                    //U	0.00	run_action_list,name=pool,if=buff.master_of_subtlety.down&buff.shadow_dance.down&debuff.find_weakness.down&(energy+cooldown.shadow_dance.remains*energy.regen<80|energy+cooldown.vanish.remains*energy.regen<60)
                    //V	18.85	hemorrhage,if=remains<3|position_front
                    //W	0.00	shuriken_toss,if=talent.shuriken_toss.enabled&(energy<65&energy.regen<16)
                    //X	163.63	backstab
                    //Y	0.00	run_action_list,name=pool
                    );
            }
        }

        public static Composite CreateSRBuffs { get; set; }
    }
}
