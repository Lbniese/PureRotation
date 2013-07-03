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
    class RestorationDruid// : AdvancedAI
    {
        //public override WoWClass Class { get { return WoWClass.Druid; } }
        //public override WoWSpec Spec { get { return WoWSpec.DruidRestoration; } }
        LocalPlayer Me { get { return StyxWoW.Me; } }

        public static Composite CreateRDCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        RestorationDruidPvP.CreateRDPvPCombat));
            }
        }

        public static Composite CreateRDBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        RestorationDruidPvP.CreateRDPvPBuffs));
            }
        }
    }
}
