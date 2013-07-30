using AdvancedAI.Managers;
using Styx;
using Styx.TreeSharp;
using Styx.WoWInternals.WoWObjects;
using AdvancedAI.Helpers;
using System;
using System.Linq;

namespace AdvancedAI.Spec
{
    class DisciplinePriest
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        static WoWUnit healtarget { get { return HealerManager.FindLowestHealthTarget(); } }
        private static string[] _doNotHeal;
        internal static Composite CreateDPCombat
        {
            get
            {
                HealerManager.NeedHealTargeting = true;
                var cancelHeal = Math.Max(95, Math.Max(93, Math.Max(55, 25)));
                return new PrioritySelector(ctx => HealerManager.Instance.TargetList.Any(t => t.IsAlive),
                    Spell.WaitForCastOrChannel(),
                    new Decorator(ret => AdvancedAI.PvPRot,
                                  DisciplinePriestPvP.CreateDPPvPCombat),
                    new Decorator(ret => Me.Combat || healtarget.Combat || healtarget.GetPredictedHealthPercent() <= 99,
                 new PrioritySelector(

                    Spell.Cast("Void Shift", on => healtarget, ret => Group.Tanks.Any(u => u.Guid == healtarget.Guid && healtarget.HealthPercent < 25)),//tanks 
                    Spell.Cast("Mindbender", ret => Me.ManaPercent <= 87),
                    Spell.Cast("Inner Focus", ret => Me.HasAura("Spirit Shell") || healtarget.HealthPercent < 45),
                    //Spell.Cast("Prayer of Healing"),//with ss buff
                    Spell.Cast("Purify"),
                    Spell.Cast("Archangel", ret => Me.HasAura("Evangelism, 5")),//5 stacks
                    //healing
                    Spell.Cast("Power Word: Shield", on => healtarget,
                                ret => healtarget.HealthPercent < 80,
                                ret => Me.ManaPercent > 40),// every 12 secs on current tank
                    Spell.Cast("Prayer of Mending", on => healtarget, ret => !healtarget.HasAura("Prayer of Mending")),
                    Spell.Cast("Penance", on => healtarget,
                                ret => healtarget.HealthPercent < 65),// on ppl if low if not on boss
                    Spell.Cast("Flash Heal", on => healtarget,
                                ret => healtarget.HealthPercent < 25,
                                cancel => healtarget.HealthPercent > cancelHeal),
                    Spell.Cast("Greater Heal", on => healtarget,
                                ret => healtarget.HealthPercent < 45,
                                cancel => healtarget.HealthPercent > cancelHeal),
                    //dps part
                    Spell.Cast("Penance"),
                    Spell.Cast("Holy Fire"),
                    Spell.Cast("Smite")


                    )));

            }
        }

        internal static Composite CreateDPBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        DisciplinePriestPvP.CreateDPPvPBuffs));
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
