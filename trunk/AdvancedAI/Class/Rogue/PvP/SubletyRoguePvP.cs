using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonBehaviors.Actions;
using Styx.TreeSharp;

namespace AdvancedAI.Spec
{
    class SubletyRoguePvP
    {
        public static Composite CreateSRPvPCombat
        {
            get
            {
                return new PrioritySelector(
                    new ActionAlwaysSucceed()
                    );

            }
        }

        public static Composite CreateSRPvPBuffs
        {
            get
            {
                return new PrioritySelector(
                    new ActionAlwaysSucceed()
                    );

            }
        }

        #region RogueTalents
        public enum RogueTalents
        {
            None = 0,
            Nightstalker,
            Subterfuge,
            ShadowFocus,
            DeadlyThrow,
            NerveStrike,
            CombatReadiness,
            CheatDeath,
            LeechingPoison,
            Elusivenss,
            CloakAndDagger,
            Shadowstep,
            BurstOfSpeed,
            PreyOnTheWeak,
            ParalyticPoison,
            DirtyTricks,
            ShurikenToss,
            MarkedForDeath,
            Anticipation
        }
        #endregion
    }
}
