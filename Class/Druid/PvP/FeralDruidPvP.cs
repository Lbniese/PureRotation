using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonBehaviors.Actions;
using Styx.TreeSharp;

namespace AdvancedAI.Spec
{
    class FeralDruidPvP
    {
        public static Composite CreateFDPvPCombat
        {
            get
            {
                return new PrioritySelector(
                    new ActionAlwaysSucceed()
                    );

            }
        }

        public static Composite CreateFDPvPBuffs
        {
            get
            {
                return new PrioritySelector(
                    new ActionAlwaysSucceed()
                    );

            }
        }
    }
}
