using Styx;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using AdvancedAI.Helpers;
using System.Linq;

namespace AdvancedAI.Spec
{
    class ShadowPriest
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        private const int MindFlay = 15407;
        private const int Insanity = 129197;
        private static uint Orbs { get { return Me.GetCurrentPower(WoWPowerType.ShadowOrbs); } }

        internal static Composite CreateSPCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        ShadowPriestPvP.CreateSPPvPCombat),
                    //new Decorator(ret => Me.IsCasting && (Me.ChanneledSpell == null || Me.ChanneledSpell.Id != MindFlay), new Action(ret => { return RunStatus.Success; })),
                    Spell.Cast("Shadowfiend", ret => Me.CurrentTarget.IsBoss && SpellManager.HasSpell("Shadowfiend") && SpellManager.Spells["Shadowfiend"].CooldownTimeLeft.TotalMilliseconds < 10),
                    Spell.Cast("Mindbender", ret => Me.CurrentTarget.IsBoss),
                    Spell.Cast("Power Infusion", ret => Me.CurrentTarget.IsBoss && SpellManager.HasSpell("Power Infusion") && SpellManager.Spells["Power Infusion"].CooldownTimeLeft.TotalMilliseconds < 10),
                    Spell.Cast("Void Shift", on => VoidTank),
                    new Decorator(ret => Unit.UnfriendlyUnitsNearTarget(10f).Count() > 2,
                        CreateAOE()),
                    Spell.Cast("Shadow Word: Pain", ret => Orbs == 3 && Me.CurrentTarget.HasMyAura("Shadow Word: Pain") && Me.CurrentTarget.GetAuraTimeLeft("Shadow Word: Pain").TotalSeconds <= 6),
                    Spell.Cast("Vampiric Touch", ret => Orbs == 3 && Me.CurrentTarget.HasMyAura("Vampiric Touch") && Me.CurrentTarget.GetAuraTimeLeft("Shadow Word: Pain").TotalSeconds <= 6),
                    Spell.Cast("Devouring Plague", ret => Orbs == 3 &&
                     Me.CurrentTarget.GetAuraTimeLeft("Shadow Word: Pain").TotalSeconds >= 6 &&
                     Me.CurrentTarget.GetAuraTimeLeft("Vampiric Touch").TotalSeconds >= 6),

                    //Spell.Cast("Devouring Plague", ret => Orbs == 3),
                    Spell.Cast("Mind Blast", ret => Orbs < 3),
                    new Throttle(1, 2,
                        new PrioritySelector(
                            Spell.Cast("Shadow Word: Death", ret => Orbs < 3))),
                    new Throttle(1, 3,
                        new PrioritySelector(
                            Spell.Cast("Mind Flay", ret => Me.CurrentTarget.HasMyAura("Devouring Plague")))),
                    Spell.Cast("Shadow Word: Pain", ret => !Me.CurrentTarget.HasMyAura("Shadow Word: Pain") || Me.CurrentTarget.HasAuraExpired("Shadow Word: Pain", 2)),
                    new Throttle(1, 2,
                        new PrioritySelector(
                            Spell.Cast("Vampiric Touch", ret => !Me.CurrentTarget.HasAura("Vampiric Touch") || Me.CurrentTarget.HasAuraExpired("Vampiric Touch", 4)))),
                    Spell.Cast("Halo", ret => Me.CurrentTarget.Distance < 30),
                    Spell.Cast("Cascade"),
                    Spell.Cast("Divine Star"),
                    new Throttle(1, 2,
                        new PrioritySelector(
                            Spell.Cast("Mind Flay", on => Me.CurrentTarget, ret => Me.CurrentTarget.HasMyAura("Shadow Word: Pain") && Me.CurrentTarget.HasMyAura("Vampiric Touch")))),
                    Spell.Cast("Shadow Word: Death", ret => Me.IsMoving),
                    Spell.Cast("Shadow Word: Pain", ret => Me.IsMoving)
                    );
            }
        }

        internal static Composite CreateSPBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        ShadowPriestPvP.CreateSPPvPBuffs),
                    PartyBuff.BuffGroup("Power Word: Fortitude"),
                    Spell.Cast("Shadowform", ret => !Me.HasAura("Shadowform")),
                    Spell.Cast("Inner Fire", ret => !Me.HasAura("Inner Fire")));
            }
        }

        private static Composite CreateAOE()
        {
            return new PrioritySelector(
                Spell.Cast("Mind Sear", on => SearTarget),
                Spell.Cast("Cascade"),
                Spell.Cast("Divine Star"),
                Spell.Cast("Halo", ret => Me.CurrentTarget.Distance < 30),
                Spell.Cast("Shadow Word: Pain", on => PainMobs),
                Spell.Cast("Vampiric Touch", on => TouchMobs),
                Spell.Cast("Mind Flay", ret => Me.CurrentTarget.HasAura("Devouring Plague")),
                Spell.Cast("Mind Blast", ret => Orbs < 3),
                Spell.Cast("Devouring Plague", ret => Orbs == 3),
                Spell.Cast("Mind Flay", on => Me.CurrentTarget, ret => Me.CurrentTarget.HasAura("Shadow Word: Pain") && Me.CurrentTarget.HasAura("Vampiric Touch") && Me.ChanneledCastingSpellId != MindFlay));
        }


        public static WoWUnit PainMobs
        {
            get
            {
                var painOn = (from unit in ObjectManager.GetObjectsOfType<WoWUnit>(false)
                               where unit.IsAlive
                               where unit.IsHostile
                               where unit.InLineOfSight
                               where !unit.HasAura("Shadow Word: Pain")
                               where unit.Distance < 40
                               where unit.IsTargetingUs() || unit.IsTargetingMyRaidMember
                               select unit).FirstOrDefault();
                return painOn;
            }
        }

        public static WoWUnit TouchMobs
        {
            get
            {
                var painOn = (from unit in ObjectManager.GetObjectsOfType<WoWUnit>(false)
                              where unit.IsAlive
                              where unit.IsHostile
                              where unit.InLineOfSight
                              where !unit.HasAura("Vampiric Touch")
                              where unit.Distance < 40
                              where unit.IsTargetingUs() || unit.IsTargetingMyRaidMember
                              select unit).FirstOrDefault();
                return painOn;
            }
        }

        public static WoWUnit SearTarget
        {
            get
            {
                var bestTank = Group.Tanks.FirstOrDefault(t => t.IsAlive && Clusters.GetClusterCount(t, Unit.NearbyUnfriendlyUnits, ClusterType.Radius, 10f) >= 5);
                if (bestTank != null)
                    return bestTank;
                var searMob = (from unit in ObjectManager.GetObjectsOfType<WoWUnit>(false)
                                where unit.IsAlive
                                where !unit.IsHostile
                                where unit.InLineOfSight
                                where Clusters.GetClusterCount(Me.CurrentTarget, Unit.NearbyUnfriendlyUnits, ClusterType.Radius, 10f) >= 4
                                select unit).FirstOrDefault();
                return searMob;
            }
        }

        public static WoWUnit VoidTank
        {
            get
            {
                var voidOn = (from unit in ObjectManager.GetObjectsOfType<WoWPlayer>(false)
                                where unit.IsAlive
                                where Group.Tanks.Any()
                                where unit.HealthPercent <= 30 && Me.HealthPercent > 70
                                where unit.IsPlayer
                                where !unit.IsHostile
                                where unit.InLineOfSight
                                select unit).FirstOrDefault();
                return voidOn;
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
