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
    class HolyPriest
    {
        LocalPlayer Me { get { return StyxWoW.Me; } }
        public static Composite CreateHPCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        HolyPriestPvP.CreateHPPvPCombat));
            }
        }

        public static Composite CreateHPBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        HolyPriestPvP.CreateHPPvPBuffs));
            }
        }

        #region PriestTalents
        public enum PriestTalents
        {
            VoidTendrils = 1,
            Psyfiend,
            DominateMind,
            BodyAndSoul,
            AngelicFeather,
            Phantasm,
            FromDarknessComesLight,
            Mindbender,
            SolaceAndInsanity,
            DesperatePrayer,
            SpectralGuise,
            AngelicBulwark,
            TwistOfFate,
            PowerInfusion,
            DivineInsight,
            Cascade,
            DivineStar,
            Halo
        }
        #endregion
    }
}
