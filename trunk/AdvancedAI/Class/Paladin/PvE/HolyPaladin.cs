using System;
using AdvancedAI.Managers;
using Styx;
using Styx.TreeSharp;
using Styx.WoWInternals.WoWObjects;
using AdvancedAI.Helpers;
using System.Linq;

namespace AdvancedAI.Spec
{
    class HolyPaladin
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        static WoWPlayer tank { get { return Group.Tanks.FirstOrDefault(); } }
        static WoWUnit healtarget { get { return HealerManager.FindLowestHealthTarget(); } }
        public static Composite CreateHPaCombat
        {
            get
            {
                HealerManager.NeedHealTargeting = true;
                var cancelHeal = Math.Max(95, Math.Max(93, Math.Max(55, 25)));
                return new PrioritySelector(ctx => HealerManager.Instance.TargetList.Any(t => t.IsAlive),
                    new Decorator(ret => AdvancedAI.PvPRot,
                        HolyPaladinPvP.CreateHPaPvPCombat),
                    new Decorator(ret => Me.Combat || healtarget.Combat || healtarget.GetPredictedHealthPercent() <= 99,
                        new PrioritySelector(
                            //beacon tank
                            Spell.Cast("Beacon of Light", on => tank, ret => !tank.HasAura("Beacon of Light"))
                            //urgent heals
                            //tank heals
                            //aoe heals
                            // single heals
                        )));
            }
        }

        public static Composite CreateHPaBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        HolyPaladinPvP.CreateHPaPvPBuffs));
            }
        }

        #region PaladinTalents
        public enum PaladinTalents
        {
            SpeedofLight = 1,//Tier 1
            LongArmoftheLaw,
            PersuitofJustice,
            FistofJustice,//Tier 2
            Repentance,
            BurdenofGuilt,
            SelflessHealer,//Tier 3
            EternalFlame,
            SacredShield,
            HandofPurity,//Tier 4
            UnbreakableSpirit,
            Clemency,
            HolyAvenger,//Tier 5
            SanctifiedWrath,
            DivinePurpose,
            HolyPrism,//Tier 6
            LightsHammer,
            ExecutionSentence
        }
        #endregion
    }
}
