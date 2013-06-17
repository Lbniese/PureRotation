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
    class ProtectionPaladin
    {
        //public override WoWClass Class { get { return WoWClass.Paladin; } }
        //public override WoWSpec Spec { get { return WoWSpec.PaladinProtection; } }
        LocalPlayer Me { get { return StyxWoW.Me; } }

        public static Composite CreatePPCombat { get; set; }

        public static Composite CreatePPBuffs { get; set; }
    }
}
