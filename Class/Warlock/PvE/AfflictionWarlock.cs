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
    class AfflictionWarlock// : AdvancedAI
    {
        LocalPlayer Me { get { return StyxWoW.Me; } }
        public static Composite CreateAWCombat
        {
            get
            {
                return new PrioritySelector(
                    //8	0.00	curse_of_the_elements,if=debuff.magic_vulnerability.down
                    //9	1.00	jade_serpent_potion,if=buff.bloodlust.react|target.health.pct<=20
                    //A	4.32	lifeblood
                    //B	3.03	berserking
                    //C	4.32	dark_soul
                    //D	0.00	service_pet,if=talent.grimoire_of_service.enabled
                    //E	0.00	run_action_list,name=aoe,if=active_enemies>3
                    //F	1.00	summon_doomguard
                    //G	15.22	soul_swap,cycle_targets=1,if=buff.soulburn.up
                    //H	7.64	soulburn,line_cd=5,if=buff.perfect_aim.react
                    //I	4.45	soulburn,if=(buff.dark_soul.up|trinket.proc.intellect.react)&(dot.agony.ticks_remain<=action.agony.add_ticks%2|dot.corruption.ticks_remain<=action.corruption.add_ticks%2|dot.unstable_affliction.ticks_remain<=action.unstable_affliction.add_ticks%2)&shard_react&(dot.unstable_affliction.crit_pct<100&dot.corruption.crit_pct<100&dot.agony.crit_pct<100)
                    //J	1.70	soulburn,if=(dot.unstable_affliction.ticks_remain<=1|dot.corruption.ticks_remain<=1|dot.agony.ticks_remain<=1)&shard_react&target.health.pct<=20&(dot.unstable_affliction.crit_pct<100&dot.corruption.crit_pct<100&dot.agony.crit_pct<100)
                    //K	10.69	haunt,if=!in_flight_to_target&remains<cast_time+travel_time+tick_time&shard_react&target.health.pct<=20
                    //L	1.44	soulburn,if=stat.spell_power>dot.unstable_affliction.spell_power&dot.unstable_affliction.ticks_remain<=action.unstable_affliction.add_ticks%2&shard_react&target.health.pct<=20&(dot.unstable_affliction.crit_pct<100&dot.corruption.crit_pct<100&dot.agony.crit_pct<100)
                    //M	0.43	life_tap,if=buff.dark_soul.down&buff.bloodlust.down&mana.pct<10&target.health.pct<=20
                    //N	15.27	drain_soul,interrupt=1,chain=1,if=target.health.pct<=20
                    //O	0.05	life_tap,if=target.health.pct<=20
                    //P	4.52	agony,cycle_targets=1,if=remains<gcd&remains+2<cooldown.dark_soul.remains&miss_react
                    //Q	29.00	haunt,if=!in_flight_to_target&remains<cast_time+travel_time+tick_time&(soul_shard>2|cooldown.dark_soul.remains>35|(soul_shard>1&cooldown.dark_soul.remains<cast_time))&shard_react
                    //R	6.26	corruption,cycle_targets=1,if=remains<gcd&remains<cooldown.dark_soul.remains&miss_react
                    //S	10.50	unstable_affliction,cycle_targets=1,if=remains<gcd+cast_time&remains<cooldown.dark_soul.remains&miss_react
                    //T	0.76	agony,cycle_targets=1,if=ticks_remain<=2&remains+2<cooldown.dark_soul.remains&miss_react&dot.agony.crit_pct<100
                    //U	1.19	corruption,cycle_targets=1,if=ticks_remain<=2&remains<cooldown.dark_soul.remains&miss_react&dot.corruption.crit_pct<100
                    //V	1.17	unstable_affliction,cycle_targets=1,if=(remains-cast_time)%(duration%current_ticks)<=2&remains<cooldown.dark_soul.remains&miss_react&dot.unstable_affliction.crit_pct<100
                    //W	2.88	agony,cycle_targets=1,if=stat.spell_power>spell_power&ticks_remain<add_ticks%2&remains+2<cooldown.dark_soul.remains&miss_react&dot.agony.crit_pct<100
                    //X	4.77	corruption,cycle_targets=1,if=stat.spell_power>spell_power&ticks_remain<add_ticks%2&remains<cooldown.dark_soul.remains&miss_react&dot.corruption.crit_pct<100
                    //Y	5.29	unstable_affliction,cycle_targets=1,if=stat.spell_power>spell_power&ticks_remain<add_ticks%2&remains<cooldown.dark_soul.remains&miss_react&dot.unstable_affliction.crit_pct<100
                    //Z	7.84	life_tap,if=buff.dark_soul.down&buff.bloodlust.down&mana.pct<50
                    //a	50.04	malefic_grasp,chain=1,interrupt_if=cooldown.dark_soul.remains=0|target.health.pct<=20
                    //b	0.00	life_tap,moving=1,if=mana.pct<80&mana.pct<target.health.pct
                    //c	0.00	fel_flame,moving=1
                    //d	0.00	life_tap
                    );
            }
        }

        public static Composite CreateAWBuffs { get; set; }
    }
}
