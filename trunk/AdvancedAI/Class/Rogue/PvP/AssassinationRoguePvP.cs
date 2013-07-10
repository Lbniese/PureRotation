using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonBehaviors.Actions;
using Styx.TreeSharp;

namespace AdvancedAI.Spec
{
    class AssassinationRoguePvP
    {
        public static Composite CreateARPvPCombat
        {
            get
            {
                return new PrioritySelector(
                    new ActionAlwaysSucceed()
                    );

            }
        }

        public static Composite CreateARPvPBuffs
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
