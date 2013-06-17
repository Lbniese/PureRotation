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
    class HolyPaladin
    {
        //public override WoWClass Class { get { return WoWClass.Paladin; } }
        //public override WoWSpec Spec { get { return WoWSpec.PaladinHoly; } }
        LocalPlayer Me { get { return StyxWoW.Me; } }

        public static Composite CreateHPCombat { get; set; }

        public static Composite CreateHPBuffs { get; set; }
    }
}
