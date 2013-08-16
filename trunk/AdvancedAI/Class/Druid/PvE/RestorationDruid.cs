﻿using AdvancedAI.Managers;
using Styx;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using AdvancedAI.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedAI.Spec
{
    class RestorationDruid
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        static WoWUnit healtarget { get { return HealerManager.FindLowestHealthTarget(); } }
        static WoWUnit LifebloomTank { get { return HealerManager.GetBestTankTargetForHOT("Lifebloom"); } }
        static WoWUnit RejuvTank { get { return HealerManager.GetBestTankTargetForHOT("Rejuvenation"); } }
        static WoWUnit RegrowthTank { get { return HealerManager.GetBestTankTargetForHOT("Regrowth"); } }
        static WoWUnit SwiftmendTarget { get { return HealerManager.GetSwiftmendTarget; } }
        public static Composite CreateRDCombat
        {
            get
            {
                HealerManager.NeedHealTargeting = true;
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        RestorationDruidPvP.CreateRDPvPCombat),
                    Spell.Cast("Barkskin", 
                        on => Me, 
                        ret => Me.HealthPercent < 60 && 
                               Me.Combat && 
                               !Me.HasAuraWithEffect(WoWApplyAuraType.ModRangedDamageTakenPct, -1, -10, -30)),
                    Spell.Cast("Ironbark", 
                        on => healtarget, 
                        ret => healtarget.HealthPercent < 70 && 
                               healtarget.Combat && 
                               healtarget.HasAuraWithEffect(WoWApplyAuraType.ModDamagePercentTaken, -1, -10, -30)),
                    Spell.Cast("Might of Ursoc", ret => Me.HealthPercent < 30),
                    Spell.Cast("Innervate", on => Me, ret => Me.ManaPercent < 60 || (Me.HasAura("Hymn of Hope") && Me.ManaPercent < 80)),
                    new Decorator(ret => AdvancedAI.Dispell,
                        Dispelling.CreateDispelBehavior()),
                    new Decorator(ret => AdvancedAI.InterruptsEnabled,
                        Common.CreateInterruptBehavior()),
                    new Decorator(ret => AdvancedAI.Burst,
                        new PrioritySelector(
                            Spell.Cast("Force of Nature", ret => HealerManager.GetCountWithHealth(60) >= 5),
                            Spell.Cast("Tranquility", ret => HealerManager.GetCountWithHealth(60) >= 5),
                            Spell.Cast("Incarnation", ret => HealerManager.GetCountWithHealth(60) >= 5))),
                    Spell.Cast("Cenarion Ward", on => healtarget, ret => healtarget.HealthPercent < 90),
                    new Decorator(ret => TalentManager.IsSelected((int)DruidTalents.SouloftheForest), HandleSotF()),

                    new Decorator(ret => healtarget.HealthPercent < 20,
                        new Sequence(
                            Spell.Cast("Nature's Swiftness"),
                            Spell.Cast("Regrowth", on => healtarget))),

                    Spell.Cast("Lifebloom", 
                        on => LifebloomTank, 
                        ret => LifebloomTank.GetAuraTimeLeft("Lifebloom").TotalSeconds >= 1.5 &&
                               !LifebloomTank.HasMyAura("Lifebloom", 3) && 
                               LifebloomTank.HealthPercent >= 10 ),

                    Spell.Cast("Swiftmend", 
                        on => healtarget, 
                        ret => healtarget.HealthPercent <= 60 && 
                               healtarget.HasAnyAura("Regrowth", "Rejuvenation")),
                    new Decorator(ret => SwiftmendTarget != null,
                        new PrioritySelector(
                            Spell.Cast("Rejuvenation", 
                                on => SwiftmendTarget, 
                                ret => !SwiftmendTarget.HasAnyAura("Regrowth","Rejuvenation")),
                            Spell.Cast("Swiftmend", 
                                on => SwiftmendTarget, 
                                ret => SwiftmendTarget.HasAnyAura("Regrowth","Rejuvenation")))),

                    new PrioritySelector(context => BestWildGrowthTarget,
                        Spell.Cast("Wild Growth", 
                            on => healtarget,
                            ret => Clusters.GetClusterCount(healtarget, WildGrowthPlayers(), ClusterType.Radius, 30f) >= 2)),
                    //Spell.Cast("Wild Mushroom: Bloom", ret => false, ret => Me, ret => MushroomCount == 3 && Settings.Instance.EnableMushrooms && GetRadiusClusterCount(AnyMushrooom, MushroomUnits(), 8f) >= Settings.Instance.AmountShroom),
                    Spell.Cast("Regrowth", 
                        on => healtarget, 
                        ret => Me.HasAura("Spiritual Innervation") &&
                               healtarget.HealthPercent <= 95, 
                        cancel => Me.GetAuraTimeLeft("Spiritual Innervation").TotalSeconds <= Me.CurrentCastTimeLeft.TotalSeconds),
                    Spell.Cast("Regrowth", 
                        on => healtarget, 
                        ret => Me.HasAura("Clearcasting") && 
                               healtarget.HealthPercent <= 80, 
                        cancel => Me.GetAuraTimeLeft("Clearcasting").TotalSeconds <= Me.CurrentCastTimeLeft.TotalSeconds),
                    Spell.Cast("Rejuvenation", 
                        on => RejuvTank, 
                        ret => Me.ManaPercent > 30 && 
                               RejuvTank.GetAuraTimeLeft("Rejuvenation").TotalSeconds <= 1),
                    Spell.Cast("Rejuvenation", 
                        on => healtarget, 
                        ret => !healtarget.HasMyAura("Rejuvenation") &&
                               healtarget.HealthPercent <= 90),
                    Spell.Cast("Regrowth", 
                        on => healtarget, 
                        ret => (!healtarget.HasAura("Regrowth") &&
                               healtarget.HealthPercent <= 50) ||
                               (healtarget.HealthPercent <= 35), 
                        cancel => healtarget.HealthPercent > 70 && 
                                  !Me.HasAura("Clearcasting")),
                    Spell.Cast("Regrowth", 
                        on => RegrowthTank, 
                        ret => Me.HasAura("Clearcasting"), 
                        cancel => Me.GetAuraTimeLeft("Clearcasting").TotalSeconds <= Me.CurrentCastTimeLeft.TotalSeconds),
                    Spell.Cast("Healing Touch", 
                        on => healtarget,
                        ret => healtarget.HealthPercent <= 70, 
                        cancel => healtarget.HealthPercent < 50 && 
                                  Me.CurrentCastTimeLeft.TotalSeconds > 0.5),
                    Spell.Cast("Lifebloom", 
                        on => LifebloomTank,
                        ret => healtarget.HealthPercent > 10 && 
                               !LifebloomTank.HasMyAura("Lifebloom", 3)),

                    //Spell.CastOnGround("Wild Mushroom", ret => BestAoeTarget.Location, ret => Settings.Instance.EnableMushrooms && (MushroomCount < 3 || GetRadiusClusterCount(AnyMushrooom, NearbyPartyPlayers, 8f) == 0)),
                    Spell.Cast("Nourish", 
                        on => healtarget, 
                        ret => Me.HasAnyAura("Glyph of Rejuvenation", "Heroism", "Bloodlust", "Time Warp", "Ancient Hysteria") &&
                               healtarget.HealthPercent <= 99 && 
                               healtarget.HasAnyAura("Rejuvenation", "Regrowth", "Wild Growth", "Lifebloom"),
                        cancel => healtarget.HealthPercent <= 50 && 
                                  Me.CurrentCastTimeLeft.TotalSeconds > Spell.GcdTimeLeft.TotalSeconds)
                    //Mirabis Tank Styler
                    //Spell.Cast("Nourish", 
                    //    mov => true,
                    //    on => healtarget, 
                    //    ret => healtarget.HealthPercent <= 90,
                    //   cancel => healtarget.HealthPercent < 70),
                    //Spell.Cast("Regrowth", 
                    //    mov => true,
                    //    on => RegrowthTank, 
                    //    ret => true,
                    //    cancel => (healtarget.HealthPercent < 70 && 
                    //              RegrowthTank.HealthPercent > 60) || 
                    //              (RegrowthTank.HealthPercent > 60 && 
                    //              Me.CurrentCastTimeLeft.TotalSeconds < 0.4))
                                  );
            }
        }

        public static Composite CreateRDBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        RestorationDruidPvP.CreateRDPvPBuffs));
            }
        }
        #region Sotf
        public static Composite HandleSotF()
        {
            return new PrioritySelector(
                     new PrioritySelector(context => BestWildGrowthTarget,
                         Spell.Cast("Wild Growth", 
                            mov => false, 
                            on => healtarget, 
                            ret => Me.HasAura("Soul of the Forest") && 
                                   Clusters.GetClusterCount(healtarget, WildGrowthPlayers(), ClusterType.Radius, 30f) >= 4)),
                         new Decorator(ret => SwiftmendTarget != null,
                            new PrioritySelector(
                                Spell.Cast("Rejuvenation", 
                                    mov => false, 
                                    on => SwiftmendTarget, 
                                    ret =>!SwiftmendTarget.HasAnyAura("Regrowth", "Rejuvenation")),
                                Spell.Cast("Swiftmend", 
                                    mov => false, 
                                    on => SwiftmendTarget, 
                                    ret => SwiftmendTarget.HasAnyAura("Regrowth", "Rejuvenation")))),
                     new PrioritySelector(context => BestWildGrowthTarget,
                         new Decorator(ret => SwiftmendTarget != null,
                             new PrioritySelector(
                                 Spell.Cast("Rejuvenation", 
                                    mov => false, 
                                    on => SwiftmendTarget, 
                                    ret => !SwiftmendTarget.HasAnyAura("Regrowth", "Rejuvenation")),
                                 Spell.Cast("Swiftmend", 
                                    mov => false, 
                                    on => SwiftmendTarget, 
                                    ret => SwiftmendTarget.HasAnyAura("Regrowth", "Rejuvenation") && 
                                           (Me.GetAuraTimeLeft("Harmony").TotalSeconds <= 2.35 || 
                                           Spell.GetSpellCooldown("Wild Growth").TotalSeconds >= Spell.GcdTimeLeft.TotalSeconds) && 
                                           Clusters.GetClusterCount(SwiftmendTarget, WildGrowthPlayers(), ClusterType.Radius, 30f) < 5))),
                                 Spell.Cast("Wild Growth", 
                                    mov => false, 
                                    on => healtarget,
                                    ret => !SpellManager.Spells["Swiftmend"].Cooldown && 
                                           Clusters.GetClusterCount(healtarget, WildGrowthPlayers(), ClusterType.Radius, 30f) >= 5)),
                                 Spell.Cast("Rejuvenation", 
                                    mov => false, 
                                    on => healtarget, 
                                    ret => Me.HasAura("Soul of the Forest") && 
                                           HealManager.HealTarget.GetPredictedHealthPercent() <= 70),
                                 Spell.Cast("Rejuvenation", 
                                    mov => false, 
                                    on => RejuvTank, 
                                    ret => Me.HasAura("Soul of the Forest")));
        }
        #endregion
        #region WG
        public static WoWUnit BestWildGrowthTarget
        {
            get
            {
                return Clusters.GetBestUnitForCluster(Unit.NearbyFriendlyPlayers, ClusterType.Radius, 30f);
            }
        }

        private static IEnumerable<WoWPlayer> WildGrowthPlayers()
        {
            return Unit.NearbyFriendlyPlayers.Where(u => u.IsAlive && u.InLineOfSpellSight && u.GetPredictedHealthPercent() <= 95).ToList();
        }
        #endregion

        #region DruidTalents
        public enum DruidTalents
        {
            FelineSwiftness = 1,//Tier 1
            DisplacerBeast,
            WildCharge,
            NaturesSwiftness,//Tier 2
            Renewal,
            CenarionWard,
            FaerieSwarm,//Tier 3
            MassEntanglement,
            Typhoon,
            SouloftheForest,//Tier 4
            Incarnation,
            ForceofNature,
            DisorientingRoar,//Tier 5
            UrsolsVortex,
            MightyBash,
            HeartoftheWild,//Tier 6
            DreamofCenarius,
            NaturesVigil
        }
        #endregion

        #region NS target
        public static WoWUnit NSTar
        {
            get
            {
                var tanks = Group.Tanks.OrderByDescending(u => u.HealthPercent).LastOrDefault();
                if (tanks != null && tanks.IsAlive && tanks.IsValid && tanks.HealthPercent < 35 && tanks.Distance < 40)
                    return tanks;
                return null;
            }
        }
        #endregion
    }
}
