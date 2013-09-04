using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Styx.Common;
using Styx.Helpers;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

namespace AdvancedAI.Helpers
{
    class Interrupting
    {
        public static WoWUnit InteruptTarget
        {
            get
            {
                var bestInt = (from unit in ObjectManager.GetObjectsOfType<WoWPlayer>(false)
                               where unit.IsAlive
                               where unit.IsPlayer
                               where !unit.IsFriendly
                               where !unit.IsInMyPartyOrRaid
                               where unit.InLineOfSight
                               where unit.Distance <= 10
                               where unit.IsCasting
                               where unit.CanInterruptCurrentSpellCast
                               where unit.CurrentCastTimeLeft.TotalMilliseconds < 500
                               select unit).FirstOrDefault();
                return bestInt;
            }
        }
    }
}
