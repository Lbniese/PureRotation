using System;
using System.Linq;
using System.Windows.Forms;
using AdvancedAI.Helpers;
using AdvancedAI.Managers;
using CommonBehaviors.Actions;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;

namespace AdvancedAI.Class.Priest.PvE
{
    class ShadowPriest
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        private const int MindFlay = 15407;
        private const int Insanity = 129197;
        private static uint Orbs { get { return Me.GetCurrentPower(WoWPowerType.ShadowOrbs); } }

        public static Composite ShadowCombat()
        {
            return new PrioritySelector(

                //new Decorator(ret => Me.IsCasting && (Me.ChanneledSpell == null || Me.ChanneledSpell.Id != MindFlay), new Action(ret => { return RunStatus.Success; })),
                new Decorator(ret => !Me.Combat || Me.Mounted || !Me.CurrentTarget.IsAlive || !Me.GotTarget /*|| Me.ChanneledSpell.Name == "Hymn of Hope"*/,
                    new ActionAlwaysSucceed()),
                
                new Decorator(ret => Me.ChanneledSpell != null,
                    new PrioritySelector(
                        new Decorator(ret => Me.ChanneledSpell.Name == "Hymn of Hope",
                            Spell.WaitForCastOrChannel()))),
                
                Hymn(),
                MassDispel(),

                new Decorator(ret => AdvancedAI.Burst,
                    new PrioritySelector(
                        Spell.Cast("Lifeblood"),
                        new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),
                        new Action(ret => { Item.UseTrinkets(); return RunStatus.Failure; }),
                            new Decorator(ret => Me.CurrentTarget.IsBoss(),
                                new PrioritySelector(
                                    Spell.Cast("Shadowfiend", ret => Spell.GetSpellCooldown("Shadowfiend").TotalMilliseconds < 10),
                                    Spell.Cast("Mindbender", ret => Spell.GetSpellCooldown("Mindbender").TotalMilliseconds < 10),
                                    Spell.Cast("Power Infusion", ret => SpellManager.HasSpell("Power Infusion") && Spell.GetSpellCooldown("Power Infusion").TotalMilliseconds < 10))))),

                Spell.Cast("Void Shift", on => VoidTank),
                Spell.Cast("Prayer of Mending", on => Me, ret => Me.HealthPercent <=85),
                new Throttle(1,
                    Spell.Cast("Vampiric Embrace", ret => HealerManager.GetCountWithHealth(55) > 4)),

                new Decorator(
                    ret => Me.GotTarget && Me.ChanneledSpell != null,
                    new PrioritySelector(
                        new Decorator(
                            ret => Me.ChanneledSpell.Name == "Mind Flay"
                                && CMF && !SpellManager.HasSpell("Solace and Insanity"),
                            new Sequence(
                                new Action(ret => Logging.WriteDiagnostic("/cancel Mind Flay on {0} @ {1:F1}%", Me.CurrentTarget.SafeName(), Me.CurrentTarget.HealthPercent)),
                                new Action(ret => SpellManager.StopCasting()),
                                new WaitContinue(TimeSpan.FromMilliseconds(500), ret => Me.ChanneledSpell == null, new ActionAlwaysSucceed()))))),

                new Decorator(ret => Unit.UnfriendlyUnitsNearTarget(10f).Count() > 2 && AdvancedAI.Aoe,
                    CreateAOE()),
                //Spell.Cast("Shadow Word: Pain", ret => Orbs == 3 && Me.CurrentTarget.CachedHasAura("Shadow Word: Pain") && Me.CurrentTarget.CachedGetAuraTimeLeft("Shadow Word: Pain") <= 6),
                //Spell.Cast("Vampiric Touch", ret => Orbs == 3 && Me.CurrentTarget.CachedHasAura("Vampiric Touch") && Me.CurrentTarget.CachedGetAuraTimeLeft("Shadow Word: Pain") <= 6),
                //Spell.Cast("Devouring Plague", ret => Orbs == 3 && Me.CurrentTarget.CachedGetAuraTimeLeft("Shadow Word: Pain") >= 6 && Me.CurrentTarget.CachedGetAuraTimeLeft("Vampiric Touch") >= 6),
                Spell.Cast("Devouring Plague", ret => Orbs == 3),

                Spell.Cast("Mind Blast", ret => Orbs < 3),
                new Throttle(2,
                    new PrioritySelector(
                        Spell.Cast("Shadow Word: Death", ret => Orbs < 3))),
                //new Throttle(2,
                //    new PrioritySelector(
                //        Spell.Cast("Mind Flay", ret => Me.CurrentTarget.CachedHasAura("Devouring Plague")))),
                Spell.Cast("Mind Flay", on => Me.CurrentTarget, ret => SpellManager.HasSpell("Solace and Insanity") && Me.CurrentTarget.CachedHasAura("Devouring Plague")),
                new Throttle(1,
                    new PrioritySelector(
                        Spell.Cast("Shadow Word: Pain", ret => !Me.CurrentTarget.CachedHasAura("Shadow Word: Pain") || Me.CurrentTarget.HasAuraExpired("Shadow Word: Pain", 3)))),
                new Throttle(TimeSpan.FromMilliseconds(1500),
                    new PrioritySelector(
                        Spell.Cast("Vampiric Touch", ret => !Me.CurrentTarget.CachedHasAura("Vampiric Touch") || Me.CurrentTarget.HasAuraExpired("Vampiric Touch", 4)))),
                Spell.Cast("Mind Spike", ret => Me.HasAura(87160)),
                Spell.Cast("Halo", ret => Me.CurrentTarget.Distance < 30),
                Spell.Cast("Cascade"),
                Spell.Cast("Divine Star"),
                //new Throttle(1,
                //    new PrioritySelector(
                Spell.Cast("Mind Flay", on => Me.CurrentTarget, ret => !CMF),
                Spell.Cast("Shadow Word: Death", ret => Me.IsMoving),
                Spell.Cast("Shadow Word: Pain", ret => Me.IsMoving));
        }

        public static Composite ShadowPreCombatBuffs()
        {
            return new PrioritySelector(
                //new Decorator(ret => AdvancedAI.PvPRot,
                //    ShadowPriestPvP.CreateSPPvPBuffs),
                PartyBuff.BuffGroup("Power Word: Fortitude"),
                Spell.Cast("Shadowform", ret => !Me.CachedHasAura("Shadowform")),
                Spell.Cast("Inner Fire", ret => !Me.HasAura("Inner Fire")));
        }

        private static Composite CreateAOE()
        {
            return new PrioritySelector(
                Spell.Cast("Mind Blast", ret => Orbs < 3),
                Spell.Cast("Devouring Plague", ret => Orbs == 3),
                Spell.Cast("Mind Sear", on => SearTarget),
                Spell.Cast("Cascade"),
                Spell.Cast("Divine Star"),
                Spell.Cast("Halo", ret => Me.CurrentTarget.Distance < 30),
                Spell.Cast("Shadow Word: Pain", on => PainMobs),
                Spell.Cast("Vampiric Touch", on => TouchMobs),
                Spell.Cast("Mind Flay", ret => Me.CurrentTarget.CachedHasAura("Devouring Plague") && SpellManager.HasSpell("Solace and Insanity")),
                Spell.Cast("Mind Flay", on => Me.CurrentTarget, ret => !CMF));
        }

        #region Cancel Mind Flay

        private static bool CMF
        {
            get { return Me.CurrentTarget.HasAuraExpired("Shadow Word: Pain", 3) || Me.CurrentTarget.HasAuraExpired("Vampiric Touch", 4) || 
                Orbs == 3 || !SpellManager.Spells["Mind Blast"].Cooldown || Me.HasAura(87160); }
        }

        #endregion

        #region Uility
        private static Composite Hymn()
        {
            return
                new Decorator(ret => SpellManager.CanCast("Hymn of Hope") &&
                    KeyboardPolling.IsKeyDown(Keys.Z),
                    new Action(ret =>
                    {
                        SpellManager.Cast("Hymn of Hope");
                        return;
                    }));
        }

        private static Composite MassDispel()
        {
            return
                new Decorator(ret => SpellManager.CanCast("Mass Dispel") &&
                    KeyboardPolling.IsKeyDown(Keys.C),
                    new Action(ret =>
                    {
                        SpellManager.Cast("Mass Dispel");
                        Lua.DoString("if SpellIsTargeting() then CameraOrSelectOrMoveStart() CameraOrSelectOrMoveStop() end");
                        return;
                    }));
        }

        public static WoWUnit VoidTank
        {
            get
            {
                var voidOn = (from unit in ObjectManager.GetObjectsOfTypeFast<WoWPlayer>()
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
        #endregion

        #region Muliti Dot & AoE
        public static WoWUnit PainMobs
        {
            get
            {
                var painOn = (from unit in ObjectManager.GetObjectsOfTypeFast<WoWUnit>()
                              where unit.IsAlive
                              where unit.IsHostile
                              where unit.InLineOfSight
                              where !unit.CachedHasAura("Shadow Word: Pain")
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
                var painOn = (from unit in ObjectManager.GetObjectsOfTypeFast<WoWUnit>()
                              where unit.IsAlive
                              where unit.IsHostile
                              where unit.InLineOfSight
                              where !unit.CachedHasAura("Vampiric Touch")
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
                var searMob = (from unit in ObjectManager.GetObjectsOfTypeFast<WoWUnit>()
                               where unit.IsAlive
                               where !unit.IsHostile
                               where unit.InLineOfSight
                               where Clusters.GetClusterCount(Me.CurrentTarget, Unit.NearbyUnfriendlyUnits, ClusterType.Radius, 10f) >= 5
                               select unit).FirstOrDefault();
                return searMob;
            }
        }
        #endregion

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
