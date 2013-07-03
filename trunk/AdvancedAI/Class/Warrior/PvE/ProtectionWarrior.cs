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
    class ProtectionWarrior// : AdvancedAI
    {
        //public override WoWClass Class { get { return WoWClass.Warrior; } }
        //public override WoWSpec Spec { get { return WoWSpec.WarriorProtection; } }
        LocalPlayer Me { get { return StyxWoW.Me; } }

        internal static Composite CreatePWCombat 
        { 
            get 
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        ProtectionWarriorPvP.CreatePWPvPCombat));
            }
        }

        internal static Composite CreatePWBuffs 
        { 
            get 
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        ProtectionWarriorPvP.CreatePWPvPBuffs));
            }
        }
    }
}
