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
    class BalanceDruid
    {
        LocalPlayer Me { get { return StyxWoW.Me; } }
        public static Composite CreateBDCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        BalanceDruidPvP.CreateBDPvPCombat)
                    //7	1.00	jade_serpent_potion,if=buff.bloodlust.react|target.time_to_die<=40|buff.celestial_alignment.up
                    //8	18.90	starfall,if=!buff.starfall.up
                    //9	0.00	treants,if=talent.force_of_nature.enabled
                    //A	2.85	berserking,if=buff.celestial_alignment.up
                    //B	7.63	use_item,slot=hands,if=buff.celestial_alignment.up|cooldown.celestial_alignment.remains>30
                    //C	1.00	wild_mushroom_detonate,moving=0,if=buff.wild_mushroom.stack>0&buff.solar_eclipse.up
                    //D	0.00	natures_swiftness,if=talent.natures_swiftness.enabled&talent.dream_of_cenarius.enabled
                    //E	0.00	healing_touch,if=talent.dream_of_cenarius.enabled&!buff.dream_of_cenarius_damage.up&mana.pct>25
                    //F	2.98	incarnation,if=talent.incarnation.enabled&(buff.lunar_eclipse.up|buff.solar_eclipse.up)
                    //G	2.86	celestial_alignment,if=(!buff.lunar_eclipse.up&!buff.solar_eclipse.up)&(buff.chosen_of_elune.up|!talent.incarnation.enabled|cooldown.incarnation.remains>10)
                    //H	0.00	natures_vigil,if=talent.natures_vigil.enabled
                    //I	76.02	starsurge,if=buff.shooting_stars.react&(active_enemies<5|!buff.solar_eclipse.up)
                    //J	17.81	moonfire,cycle_targets=1,if=buff.lunar_eclipse.up&(remains<(buff.natures_grace.remains-2+2*set_bonus.tier14_4pc_caster))
                    //K	14.55	sunfire,cycle_targets=1,if=buff.solar_eclipse.up&(remains<(buff.natures_grace.remains-2+2*set_bonus.tier14_4pc_caster))
                    //L	0.00	hurricane,if=active_enemies>4&buff.solar_eclipse.up&buff.natures_grace.up
                    //M	14.09	moonfire,cycle_targets=1,if=active_enemies<5&(remains<(buff.natures_grace.remains-2+2*set_bonus.tier14_4pc_caster))
                    //N	14.42	sunfire,cycle_targets=1,if=active_enemies<5&(remains<(buff.natures_grace.remains-2+2*set_bonus.tier14_4pc_caster))
                    //O	0.00	hurricane,if=active_enemies>5&buff.solar_eclipse.up&mana.pct>25
                    //P	0.01	moonfire,cycle_targets=1,if=buff.lunar_eclipse.up&ticks_remain<2
                    //Q	0.23	sunfire,cycle_targets=1,if=buff.solar_eclipse.up&ticks_remain<2
                    //R	0.00	hurricane,if=active_enemies>4&buff.solar_eclipse.up&mana.pct>25
                    //S	7.93	starsurge,if=cooldown_react
                    //T	19.05	starfire,if=buff.celestial_alignment.up&cast_time<buff.celestial_alignment.remains
                    //U	0.50	wrath,if=buff.celestial_alignment.up&cast_time<buff.celestial_alignment.remains
                    //V	81.75	starfire,if=eclipse_dir=1|(eclipse_dir=0&eclipse>0)
                    //W	101.98	wrath,if=eclipse_dir=-1|(eclipse_dir=0&eclipse<=0)
                    //X	0.00	moonfire,moving=1,cycle_targets=1,if=ticks_remain<2
                    //Y	0.00	sunfire,moving=1,cycle_targets=1,if=ticks_remain<2
                    //Z	0.00	wild_mushroom,moving=1,if=buff.wild_mushroom.stack<buff.wild_mushroom.max_stack
                    //a	0.00	starsurge,moving=1,if=buff.shooting_stars.react
                    //b	0.00	moonfire,moving=1,if=buff.lunar_eclipse.up
                    //c	0.00	sunfire,moving=1                    
                    );
            }
        }

        public static Composite CreateBDBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        BalanceDruidPvP.CreateBDPvPBuffs));
            }
        }
    }
}
