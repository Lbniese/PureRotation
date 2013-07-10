using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonBehaviors.Actions;
using Styx.TreeSharp;

namespace AdvancedAI.Spec
{
    class AfflictionWarlockPvP
    {
        public static Composite CreateAWPvPCombat
        {
            get
            {
                return new PrioritySelector(
                    new ActionAlwaysSucceed()
                    );

            }
        }

        public static Composite CreateAWPvPBuffs
        {
            get
            {
                return new PrioritySelector(
                    new ActionAlwaysSucceed()
                    );

            }
        }

        #region WarlockTalents
        public enum WarlockTalents
        {
            None = 0,
            DarkRegeneration,
            SoulLeech,
            HarvestLife,
            HowlOfTerror,
            MortalCoil,
            Shadowfury,
            SoulLink,
            SacrificialPact,
            DarkBargain,
            BloodHorror,
            BurningRush,
            UnboundWill,
            GrimoireOfSupremacy,
            GrimoireOfService,
            GrimoireOfSacrifice,
            ArchimondesVengeance,
            KiljadensCunning,
            MannorothsFury
        }
        #endregion
    }
}
