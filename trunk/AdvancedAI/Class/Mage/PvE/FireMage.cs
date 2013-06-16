using Styx;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAI.Class.Mage.PvE
{
    class FireMage : AdvancedAI
    {
        public override WoWClass Class { get { return WoWClass.Mage; } }
        //public override WoWSpec Spec { get { return WoWSpec.MageFire; } }
        static LocalPlayer Me { get { return StyxWoW.Me; } }
    }
}
