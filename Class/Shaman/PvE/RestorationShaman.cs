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
    class RestorationShaman
    {
        LocalPlayer Me { get { return StyxWoW.Me; } }
        public static Composite CreateRSCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        RestorationShamanPvP.CreateRSPvPCombat));
            }
        }

        public static Composite CreateRSBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        RestorationShamanPvP.CreateRSPvPBuffs));
            }
        }

        #region ShamanTalents
        public enum ShamanTalents
        {
            NaturesGuardian = 1,
            StoneBulwarkTotem,
            AstralShift,
            FrozenPower,
            EarthgrabTotem,
            WindwalkTotem,
            CallOfTheElements,
            TotemicRestoration,
            TotemicProjection,
            ElementalMastery,
            AncestralSwiftness,
            EchoOfTheElements,
            HealingTideTotem,
            AncestralGuidance,
            Conductivity,
            UnleashedFury,
            PrimalElementalist,
            ElementalBlast
        }
        #endregion
    }
}
