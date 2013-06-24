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
    class AssassinationRogue// : AdvancedAI
    {
        LocalPlayer Me { get { return StyxWoW.Me; } }
        public static Composite CreateARCombat
        {
            get
            {
                return new PrioritySelector(
                    //8	1.00	virmens_bite_potion,if=buff.bloodlust.react|target.time_to_die<40
                    //9	6.54	auto_attack
                    //A	0.00	kick
                    //B	2.00	preparation,if=!buff.vanish.up&cooldown.vanish.remains>60
                    //C	7.99	use_item,slot=hands
                    //D	3.05	berserking
                    //E	5.54	vanish,if=time>10&!buff.stealthed.up&!buff.shadow_blades.up
                    //F	6.34	ambush
                    //G	2.98	shadow_blades,if=buff.bloodlust.react|time>60
                    //H	1.00	slice_and_dice,if=buff.slice_and_dice.remains<2
                    //I	0.21	dispatch,if=dot.rupture.ticks_remain<2&energy>90
                    //J	1.79	mutilate,if=dot.rupture.ticks_remain<2&energy>90
                    //K	0.00	marked_for_death,if=talent.marked_for_death.enabled&combo_points=0
                    //L	18.90	rupture,if=ticks_remain<2|(combo_points=5&ticks_remain<3)
                    //M	4.29	vendetta
                    //N	67.08	envenom,if=combo_points>4
                    //O	0.00	envenom,if=combo_points>=2&buff.slice_and_dice.remains<3
                    //P	113.45	dispatch,if=combo_points<5
                    //Q	85.23	mutilate
                    //R	0.00	tricks_of_the_trade
                    );
            }
        }

        public static Composite CreateARBuffs { get; set; }
    }
}
