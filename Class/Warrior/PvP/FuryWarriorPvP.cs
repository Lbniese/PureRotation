using Styx;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAI.Class.Warrior.PvP
{
    class FuryWarriorPvP : AdvancedAI
    {
        public override WoWClass Class { get { return WoWClass.Warrior; } }
        LocalPlayer Me { get { return StyxWoW.Me; } } 
    }
}
