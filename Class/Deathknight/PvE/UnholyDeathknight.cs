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
using System.Windows.Forms;

namespace AdvancedAI.Spec
{
    class UnholyDeathknight// : AdvancedAI
    {
        //public override WoWClass Class { get { return WoWClass.DeathKnight; } }
        //public override WoWSpec Spec { get { return WoWSpec.DeathKnightUnholy; } }
        LocalPlayer Me { get { return StyxWoW.Me; } }

        public static Composite CreateUDKCombat
        {
            get
            {
                if (AdvancedAI.PvPRot)
                    return UnholyDeathknightPvP.CreateBDKPvPCombat;
                else
                    return new PrioritySelector(

                        );
            }
        }

        public static Composite CreateUDKBuffs { get; set; }
    }
}
