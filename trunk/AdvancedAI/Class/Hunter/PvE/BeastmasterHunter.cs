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
    class BeastmasterHunter
    {
        LocalPlayer Me { get { return StyxWoW.Me; } }
        public static Composite CreateBMHCombat 
        { 
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        BeastmasterHunterPvP.CreateBMPvPCombat)
                    //8	1.00	virmens_bite_potion,if=buff.bloodlust.react|target.time_to_die<=60
                    //9	1.00	auto_shot
                    //A	0.00	explosive_trap,if=active_enemies>1
                    //B	10.19	focus_fire,five_stacks=1
                    //C	1.02	serpent_sting,if=!ticking
                    //D	4.31	blood_fury
                    //E	0.00	fervor,if=enabled&!ticking&focus<=65
                    //F	8.97	bestial_wrath,if=focus>60&!buff.beast_within.up
                    //G	0.00	multi_shot,if=active_enemies>5
                    //H	0.00	cobra_shot,if=active_enemies>5
                    //I	4.64	rapid_fire,if=!buff.rapid_fire.up
                    //J	2.00	stampede,if=buff.rapid_fire.up|buff.bloodlust.react|target.time_to_die<=25
                    //K	16.60	kill_shot
                    //L	68.42	kill_command
                    //M	5.25	a_murder_of_crows,if=enabled&!ticking
                    //N	29.65	glaive_toss,if=enabled
                    //O	0.00	lynx_rush,if=enabled&!dot.lynx_rush.ticking
                    //P	14.70	dire_beast,if=enabled&focus<=90
                    //Q	0.00	barrage,if=enabled
                    //R	0.00	powershot,if=enabled
                    //S	1.86	readiness,wait_for_rapid_fire=1
                    //T	0.00	arcane_shot,if=buff.thrill_of_the_hunt.react
                    //U	0.00	focus_fire,five_stacks=1,if=!ticking&!buff.beast_within.up
                    //V	5.33	cobra_shot,if=dot.serpent_sting.remains<6
                    //W	151.50	arcane_shot,if=focus>=61|buff.beast_within.up
                    //X	104.88	cobra_shot
                    );
            }
        }

        public static Composite CreateBMHBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        BeastmasterHunterPvP.CreateBMPvPBuffs));
            }
        }
    }
}
