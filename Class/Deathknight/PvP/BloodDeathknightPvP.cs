using CommonBehaviors.Actions;
using Styx.TreeSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAI.Spec
{
    class BloodDeathknightPvP
    {
        public static Composite CreateBDKPvPCombat
        {
            get
            {
                return new PrioritySelector(
                    new ActionAlwaysSucceed()
                    );
                
            }
        }

        public static Composite CreateBDKPvPBuffs
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
