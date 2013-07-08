using CommonBehaviors.Actions;
using Styx.TreeSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAI.Spec
{
    class WindwalkerMonkPvP
    {
        public static Composite CreateWMPvPCombat
        {
            get
            {
                return new PrioritySelector(
                    new ActionAlwaysSucceed()
                    );

            }
        }

        public static Composite CreateWMPvPBuffs
        {
            get
            {
                return new PrioritySelector(
                    new ActionAlwaysSucceed()
                    );

            }
        }

        #region MonkTalents
        public enum MonkTalents
        {
            Celerity = 1,//Tier 1
            TigersLust,
            Momentum,
            ChiWave,//Tier 2
            ZenSphere,
            ChiBurst,
            PowerStrikes,//Tier 3
            Ascension,
            ChiBrew,
            RingofPeace,//Tier 4
            ChargingOxWave,
            LegSweep,
            HealingElixirs,//Tier 5
            DampenHarm,
            DiffuseMagic,
            RushingJadeWind,//Tier 6
            InvokeXuen,
            ChiTorpedo
        }
        #endregion
    }
}
