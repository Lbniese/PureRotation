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
    class FrostMage// : AdvancedAI
    {
        LocalPlayer Me { get { return StyxWoW.Me; } }
        public static Composite CreateFMCombat
        {
            get
            {
                return new PrioritySelector(
                    //9	0.00	counterspell,if=target.debuff.casting.react
                    //A	0.00	cancel_buff,name=alter_time,moving=1
                    //B	0.00	conjure_mana_gem,if=mana_gem_charges<3&target.debuff.invulnerable.react
                    //C	0.50	time_warp,if=target.health.pct<25|time>5
                    //D	5.86	rune_of_power,if=buff.rune_of_power.remains<cast_time&buff.alter_time.down
                    //E	0.94	rune_of_power,if=cooldown.icy_veins.remains=0&buff.rune_of_power.remains<20
                    //F	2.04	mirror_image
                    //G	7.91	frozen_orb,if=!buff.fingers_of_frost.react
                    //H	2.94	icy_veins,if=(debuff.frostbolt.stack>=3&(buff.brain_freeze.react|buff.fingers_of_frost.react))|target.time_to_die<22,moving=0
                    //I	2.94	berserking,if=buff.icy_veins.up|target.time_to_die<18
                    //J	1.00	jade_serpent_potion,if=buff.icy_veins.up|target.time_to_die<45
                    //K	5.39	presence_of_mind,if=buff.icy_veins.up|cooldown.icy_veins.remains>15|target.time_to_die<15
                    //L	2.94	alter_time,if=buff.alter_time.down&buff.icy_veins.up
                    //M	0.00	flamestrike,if=active_enemies>=5
                    //N	3.48	frostfire_bolt,if=buff.alter_time.up&buff.brain_freeze.up
                    //O	8.24	ice_lance,if=buff.alter_time.up&buff.fingers_of_frost.up
                    //P	38.98	living_bomb,cycle_targets=1,if=(!ticking|remains<tick_time)&target.time_to_die>tick_time*3
                    //Q	5.15	frostbolt,if=debuff.frostbolt.stack<3
                    //R	61.17	frostfire_bolt,if=buff.brain_freeze.react&cooldown.icy_veins.remains>2
                    //S	70.82	ice_lance,if=buff.fingers_of_frost.react&cooldown.icy_veins.remains>2
                    //T	205.29	frostbolt
                    //U	0.00	fire_blast,moving=1
                    //V	0.00	ice_lance,moving=1                    
                    );
            }
        }

        public static Composite CreateFMBuffs { get; set; }
    }
}
