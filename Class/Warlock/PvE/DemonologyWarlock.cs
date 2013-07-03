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
    class DemonologyWarlock// : AdvancedAI
    {
        LocalPlayer Me { get { return StyxWoW.Me; } }
        public static Composite CreateDemWCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        DemonologyWarlockPvP.CreateDemWPvPCombat)
                    //8	0.00	curse_of_the_elements,if=debuff.magic_vulnerability.down
                    //9	1.00	jade_serpent_potion,if=buff.bloodlust.react|target.health.pct<=20
                    //A	4.32	lifeblood
                    //B	3.04	berserking
                    //C	4.67	imp_swarm,if=buff.dark_soul.up|(cooldown.dark_soul.remains>(120%(1%spell_haste)))|time_to_die<32
                    //D	4.32	dark_soul
                    //E	0.00	service_pet,if=talent.grimoire_of_service.enabled
                    //F	0.00	felguard:felstorm
                    //G	0.00	wrathguard:wrathstorm
                    //H	0.00	run_action_list,name=aoe,if=active_enemies>4
                    //I	1.00	summon_doomguard
                    //J	0.00	metamorphosis,if=buff.perfect_aim.react&active_enemies>1
                    //K	4.53	doom,cycle_targets=1,if=buff.metamorphosis.up&buff.perfect_aim.react&(crit_pct<100|ticks_remain<=add_ticks)
                    //L	0.52	touch_of_chaos,cycle_targets=1,if=buff.metamorphosis.up&dot.corruption.ticking&dot.corruption.remains<1.5
                    //M	3.16	soul_fire,if=buff.metamorphosis.up&buff.molten_core.react&(buff.perfect_aim.react&buff.perfect_aim.remains>cast_time)
                    //N	2.33	doom,cycle_targets=1,if=buff.metamorphosis.up&(ticks_remain<=1|(ticks_remain+1<n_ticks&buff.dark_soul.up))&target.time_to_die>=30&miss_react&dot.doom.crit_pct<100
                    //O	18.92	touch_of_chaos,cycle_targets=1,if=buff.metamorphosis.up&dot.corruption.ticking&dot.corruption.remains<20&dot.corruption.crit_pct<100
                    //P	17.08	cancel_metamorphosis,if=buff.metamorphosis.up&buff.dark_soul.down&demonic_fury<=650&target.time_to_die>30
                    //Q	61.28	soul_fire,if=buff.metamorphosis.up&buff.molten_core.react&(buff.dark_soul.remains<action.shadow_bolt.cast_time|buff.dark_soul.remains>cast_time)
                    //R	87.42	touch_of_chaos,if=buff.metamorphosis.up
                    //S	4.14	corruption,cycle_targets=1,if=buff.perfect_aim.react&(crit_pct<100|ticks_remain<=add_ticks)
                    //T	1.77	hand_of_guldan,if=buff.perfect_aim.react&buff.perfect_aim.remains>travel_time
                    //U	1.33	corruption,cycle_targets=1,if=!ticking&target.time_to_die>=6&miss_react
                    //V	20.70	metamorphosis,if=(buff.dark_soul.up&demonic_fury%32>buff.dark_soul.remains)|(dot.corruption.remains<5&dot.corruption.crit_pct<100)|!dot.doom.ticking|demonic_fury>=950|demonic_fury%32>target.time_to_die|buff.perfect_aim.react
                    //W	25.36	hand_of_guldan,if=!in_flight&dot.shadowflame.remains<travel_time+action.shadow_bolt.cast_time&(charges=2|dot.shadowflame.remains>travel_time|(charges=1&recharge_time<4))
                    //X	61.45	soul_fire,if=buff.molten_core.react&(buff.dark_soul.remains<action.shadow_bolt.cast_time|buff.dark_soul.remains>cast_time)
                    //Y	8.39	life_tap,if=mana.pct<60
                    //Z	71.47	shadow_bolt
                    //a	0.00	fel_flame,moving=1
                    //b	0.00	life_tap
                    );
            }
        }

        public static Composite CreateDemWBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        DemonologyWarlockPvP.CreateDemWPvPBuffs)
                    );
            }
        }
    }
}
