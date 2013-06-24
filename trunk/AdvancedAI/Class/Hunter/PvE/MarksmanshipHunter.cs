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
    class MarksmanshipHunter// : AdvancedAI
    {
        LocalPlayer Me { get { return StyxWoW.Me; } }
        public static Composite CreateMHKBuffs
        {
            get
            {
                return new PrioritySelector(
                    //8	1.00	virmens_bite_potion,if=buff.bloodlust.react|target.time_to_die<=60
                    //9	21.26	auto_shot
                    //A	0.00	explosive_trap,if=active_enemies>1
                    //B	4.32	blood_fury
                    //C	0.00	powershot,if=enabled
                    //D	0.00	lynx_rush,if=enabled&!dot.lynx_rush.ticking
                    //E	0.00	multi_shot,if=active_enemies>5
                    //F	0.00	steady_shot,if=active_enemies>5
                    //G	0.00	fervor,if=enabled&focus<=50
                    //H	4.33	rapid_fire,if=!buff.rapid_fire.up
                    //I	2.00	stampede,if=buff.rapid_fire.up|buff.bloodlust.react|target.time_to_die<=25
                    //J	5.91	a_murder_of_crows,if=enabled&!ticking
                    //K	16.21	dire_beast,if=enabled
                    //L	0.00	run_action_list,name=careful_aim,if=target.health.pct>80
                    //M	25.94	glaive_toss,if=enabled
                    //N	0.00	barrage,if=enabled
                    //O	14.90	steady_shot,if=buff.pre_steady_focus.up&buff.steady_focus.remains<=5
                    //P	0.86	serpent_sting,if=!ticking
                    //Q	38.22	chimera_shot
                    //R	1.00	readiness
                    //S	2.11	steady_shot,if=buff.steady_focus.remains<(action.steady_shot.cast_time+1)&!in_flight
                    //T	15.12	kill_shot
                    //U	18.28	aimed_shot,if=buff.master_marksman_fire.react
                    //V	0.00	arcane_shot,if=buff.thrill_of_the_hunt.react
                    //W	9.09	aimed_shot,if=buff.rapid_fire.up|buff.bloodlust.react
                    //X	105.64	arcane_shot,if=focus>=60|(focus>=43&(cooldown.chimera_shot.remains>=action.steady_shot.cast_time))&(!buff.rapid_fire.up&!buff.bloodlust.react)
                    //Y	106.65	steady_shot
                    );
            }
        }

        public static Composite CreateMHCombat { get; set; }
    }
}
