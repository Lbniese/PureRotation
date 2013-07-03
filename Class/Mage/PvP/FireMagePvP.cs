using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonBehaviors.Actions;
using Styx.TreeSharp;

namespace AdvancedAI.Spec
{
    class FireMagePvP
    {
        public static Composite CreateFiMPvPCombat
        {
            get
            {
                return new PrioritySelector(
                    new ActionAlwaysSucceed()
                    );

            }
        }

        public static Composite CreateFiMPvPBuffs
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
