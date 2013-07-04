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
    class DestructionWarlock
    {
        LocalPlayer Me { get { return StyxWoW.Me; } }
        public static Composite CreateDWCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        DestructionWarlockPvP.CreateDWPvPCombat)
                    //8	0.00	curse_of_the_elements,if=debuff.magic_vulnerability.down
                    //9	1.00	jade_serpent_potion,if=buff.bloodlust.react|target.health.pct<=20
                    //A	4.32	lifeblood
                    //B	4.32	blood_fury
                    //C	4.32	dark_soul
                    //D	3.32	service_pet,if=talent.grimoire_of_service.enabled
                    //E	0.00	run_action_list,name=aoe,if=active_enemies>3
                    //F	1.00	summon_doomguard
                    //G	0.00	rain_of_fire,if=!ticking&!in_flight&active_enemies>1
                    //H	0.00	havoc,target=2,if=active_enemies>1
                    //I	15.39	shadowburn,if=ember_react&(burning_ember>3.5|mana.pct<=20|buff.dark_soul.up|target.time_to_die<20|buff.havoc.stack>=1|(trinket.has_proc.intellect&trinket.proc.intellect.react)|buff.perfect_aim.react)
                    //J	5.70	chaos_bolt,if=ember_react&target.health.pct>20&buff.perfect_aim.react&buff.perfect_aim.remains>cast_time
                    //K	4.47	immolate,cycle_targets=1,if=target.time_to_die>=5&miss_react&buff.perfect_aim.remains>cast_time&buff.perfect_aim.react&crit_pct<100
                    //L	4.67	immolate,cycle_targets=1,if=buff.dark_soul.react&dot.immolate.crit_pct<(stat.crit+30)&ticks_remain<add_ticks%2&miss_react
                    //M	14.17	immolate,cycle_targets=1,if=stat.spell_power>spell_power&ticks_remain<add_ticks%2&miss_react&dot.immolate.crit_pct<(stat.crit+30)
                    //N	6.14	immolate,cycle_targets=1,if=ticks_remain<cast_time&target.time_to_die>=5&miss_react
                    //O	1.59	conflagrate,if=charges=2&buff.havoc.stack=0
                    //P	72.56	rain_of_fire,if=!ticking&!in_flight
                    //Q	43.67	chaos_bolt,if=ember_react&target.health.pct>20&(buff.backdraft.stack<3|level<86|(active_enemies>1&action.incinerate.cast_time<1))&(burning_ember>(4.5-active_enemies)|buff.dark_soul.remains>cast_time|buff.skull_banner.remains>cast_time|(trinket.proc.intellect.react&trinket.proc.intellect.remains>cast_time))
                    //R	0.00	chaos_bolt,if=ember_react&target.health.pct>20&(buff.havoc.stack=3&buff.havoc.remains>cast_time)
                    //S	37.22	conflagrate
                    //T	168.47	incinerate                    
                    );
            }
        }

        public static Composite CreateDWBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        DestructionWarlockPvP.CreateDWPvPBuffs)
                                  );
            }
        }
    }
}
