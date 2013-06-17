using CommonBehaviors.Actions;
using Styx;
using Styx.TreeSharp;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = Styx.TreeSharp.Action;

namespace AdvancedAI.Spec
{
    class FuryWarriorPvP// : AdvancedAI
    {
        //public override WoWClass Class { get { return WoWClass.Warrior; } }
        LocalPlayer Me { get { return StyxWoW.Me; } }

        public static Composite CreateFWPvPCombat
        {
            get
            {
                return new PrioritySelector(

                    new Action(ret => { new ActionAlwaysSucceed(); })
                    );

            }
        }

        public static Composite CreateFWPvPBuffs
        {
            get
            {
                return new PrioritySelector(

                    );
            }
        }
    }
}
