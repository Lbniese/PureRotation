﻿using CommonBehaviors.Actions;
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
    class DisciplinePriest// : AdvancedAI
    {
        //public override WoWClass Class { get { return WoWClass.Priest; } }
        //public override WoWSpec Spec { get { return WoWSpec.PriestDiscipline; } }
        LocalPlayer Me { get { return StyxWoW.Me; } }

        internal static Composite CreateDPCombat { get; set; }

        internal static Composite CreateDPBuffs { get; set; }
    }
}