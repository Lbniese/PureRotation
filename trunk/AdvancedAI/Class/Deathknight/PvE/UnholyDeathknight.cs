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
using System.Windows.Forms;

namespace AdvancedAI.Spec
{
    class UnholyDeathknight// : AdvancedAI
    {
        LocalPlayer Me { get { return StyxWoW.Me; } }
        public static Composite CreateUDKCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        UnholyDeathknightPvP.CreateUDKPvPCombat)
                    //9	1.00	auto_attack
                    //A	3.33	blood_fury,if=time>=2
                    //B	1.00	mogu_power_potion,if=buff.dark_transformation.up&target.time_to_die<=35
                    //C	3.00	unholy_frenzy,if=time>=4
                    //D	7.90	use_item,slot=hands,if=time>=4
                    //E	0.00	run_action_list,name=aoe,if=active_enemies>=5
                    //F	0.00	run_action_list,name=single_target,if=active_enemies<5
                    //actions.single_target
                    //#	count	action,conditions
                    //c	3.82	outbreak,if=stat.attack_power>(dot.blood_plague.attack_power*1.1)&time>15&!(cooldown.unholy_blight.remains>79)
                    //d	8.48	plague_strike,if=stat.attack_power>(dot.blood_plague.attack_power*1.1)&time>15&!(cooldown.unholy_blight.remains>79)
                    //e	14.10	blood_tap,if=talent.blood_tap.enabled&buff.blood_charge.stack>10&runic_power>=32
                    //f	3.68	unholy_blight,if=talent.unholy_blight.enabled&(dot.frost_fever.remains<3|dot.blood_plague.remains<3)
                    //g	1.20	outbreak,if=dot.frost_fever.remains<3|dot.blood_plague.remains<3
                    //h	32.28	soul_reaper,if=target.health.pct-3*(target.health.pct%target.time_to_die)<=45
                    //i	12.47	blood_tap,if=talent.blood_tap.enabled&(target.health.pct-3*(target.health.pct%target.time_to_die)<=45&cooldown.soul_reaper.remains=0)
                    //j	0.00	plague_strike,if=!dot.blood_plague.ticking|!dot.frost_fever.ticking
                    //k	3.03	summon_gargoyle
                    //l	10.66	dark_transformation
                    //m	3.69	blood_tap,if=talent.blood_tap.enabled&buff.shadow_infusion.stack=5
                    //n	21.90	death_coil,if=runic_power>90
                    //o	1.14	death_and_decay,if=unholy=2
                    //p	0.00	blood_tap,if=talent.blood_tap.enabled&unholy=2&cooldown.death_and_decay.remains=0
                    //q	2.57	scourge_strike,if=unholy=2
                    //r	6.95	festering_strike,if=blood=2&frost=2
                    //s	13.56	death_and_decay
                    //t	6.20	blood_tap,if=talent.blood_tap.enabled&cooldown.death_and_decay.remains=0
                    //u	70.03	death_coil,if=buff.sudden_doom.react|(buff.dark_transformation.down&rune.unholy<=1)
                    //v	122.75	scourge_strike
                    //w	0.00	plague_leech,if=talent.plague_leech.enabled&cooldown.outbreak.remains<1
                    //x	35.36	festering_strike
                    //y	17.99	horn_of_winter
                    //z	40.24	death_coil,if=buff.dark_transformation.down|(cooldown.summon_gargoyle.remains>8&buff.dark_transformation.remains>8)
                    //{	14.84	blood_tap,if=talent.blood_tap.enabled&buff.blood_charge.stack>=8
                    //|	1.80	empower_rune_weapon
                    );
            }
        }

        public static Composite CreateUDKBuffs { get; set; }
    }
}
