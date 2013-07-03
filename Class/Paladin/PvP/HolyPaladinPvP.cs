using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonBehaviors.Actions;
using Styx.TreeSharp;

namespace AdvancedAI.Spec
{
    class HolyPaladinPvP
    {
        public static Composite CreateHPaPvPCombat
        {
            get
            {
                return new PrioritySelector(
                    new ActionAlwaysSucceed()
                    );

            }
        }

        public static Composite CreateHPaPvPBuffs
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
