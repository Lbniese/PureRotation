using AdvancedAI.Managers;
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
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        RestorationDruidPvP.CreateRDPvPCombat),
                    new Decorator(ret => TalentManager.IsSelected((int)DruidTalents.SouloftheForest), HandleSotF()),
                    Spell.Cast("Healing Touch", 
                        mov => false, 
                        on => healtarget, 
                        ret => Me.HasAura("Nature's Swiftness")),
                    Spell.Cast("Lifebloom", 
                        mov => false, 
                        on => LifebloomTank, 
                        ret => LifebloomTank.GetAuraTimeLeft("Lifebloom").TotalSeconds <= 1.5),
                    Spell.Cast("Lifebloom", 
                        mov => false, 
                        on => LifebloomTank, 
                        ret =>  LifebloomTank.HealthPercent >= 10 && LifebloomTank.GetAuraTimeLeft("Lifebloom").TotalSeconds <= 1.5 && (!Me.HasMyAura("Lifebloom", 3) || Me.GetAuraTimeLeft("Lifebloom").TotalSeconds <= 1.5)),
                    Spell.Cast("Lifebloom", 
                        mov => false, 
                        on => Me, 
                        ret => LifebloomTank.HealthPercent >= 10 && LifebloomTank.GetAuraTimeLeft("Lifebloom").TotalSeconds <= 1.5 && (!Me.HasMyAura("Lifebloom", 3) || Me.GetAuraTimeLeft("Lifebloom").TotalSeconds <= 1.5)),
                    Spell.Cast("Swiftmend", 
                        mov => false, 
                        on => healtarget, 
                        ret => healtarget.HealthPercent <= 35 && healtarget.HasAnyAura("Regrowth", "Rejuvenation")),
                    new Decorator(ret => SwiftmendTarget != null,
                        new PrioritySelector(
                            Spell.Cast("Rejuvenation", 
                                mov => false, 
                                on => SwiftmendTarget, 
                                ret => !TalentManager.IsSelected((int)DruidTalents.SouloftheForest) && !SwiftmendTarget.HasAnyAura("Regrowth","Rejuvenation")),
                            Spell.Cast("Swiftmend", 
                                mov => false, 
                                on => SwiftmendTarget, 
                                ret => !TalentManager.IsSelected((int)DruidTalents.SouloftheForest) && SwiftmendTarget.HasAnyAura("Regrowth","Rejuvenation")))),
                    new PrioritySelector(context => BestWildGrowthTarget,
                        Spell.Cast("Wild Growth", 
                            mov => false,
                            on => healtarget,
                            ret => !TalentManager.IsSelected((int)DruidTalents.SouloftheForest) && Clusters.GetClusterCount(healtarget, WildGrowthPlayers(), ClusterType.Radius, 30f) >= 5)),
                    //Spell.Cast("Wild Mushroom: Bloom", ret => false, ret => Me, ret => MushroomCount == 3 && Settings.Instance.EnableMushrooms && GetRadiusClusterCount(AnyMushrooom, MushroomUnits(), 8f) >= Settings.Instance.AmountShroom),
                    Spell.Cast("Regrowth", 
                        mov => true,
                        on => healtarget, 
                        ret => Me.HasAura("Spiritual Innervation") && healtarget.GetPredictedHealthPercent() <= 95, ret => Me.GetAuraTimeLeft("Spiritual Innervation").TotalSeconds <= Me.CurrentCastTimeLeft.TotalSeconds),
                    Spell.Cast("Regrowth", 
                        mov => true,
                        on => healtarget, 
                        ret => Me.HasAura("Clearcasting") && healtarget.GetPredictedHealthPercent() <= 95, ret => Me.GetAuraTimeLeft("Clearcasting").TotalSeconds <= Me.CurrentCastTimeLeft.TotalSeconds),
                    Spell.Cast("Rejuvenation", 
                        mov => false,
                        on => RejuvTank, 
                        ret => Me.ManaPercent > 30 && RejuvTank.GetAuraTimeLeft("Rejuvenation").TotalSeconds <= 1),
                    Spell.Cast("Rejuvenation", 
                        mov => false, 
                        on => healtarget, 
                        ret => !healtarget.HasMyAura("Rejuvenation") && healtarget.GetPredictedHealthPercent() <= 85),
                    Spell.Cast("Regrowth", 
                        mov => true,
                        on => healtarget, 
                        ret => (!healtarget.HasAura("Regrowth") && healtarget.GetPredictedHealthPercent() <= 50) || (healtarget.GetPredictedHealthPercent() <= 35), 
                        cancel => healtarget.HealthPercent > 70 && !Me.HasAura("Clearcasting")),
                    Spell.Cast("Regrowth", 
                        mov => true,
                        on => RegrowthTank, 
                        ret => Me.HasAura("Clearcasting"), ret => Me.GetAuraTimeLeft("Clearcasting").TotalSeconds <= Me.CurrentCastTimeLeft.TotalSeconds),
                    Spell.Cast("Healing Touch", 
                        mov => true,
                        on => healtarget, 
                        ret => healtarget.GetPredictedHealthPercent() <= 30, 
                        cancel => healtarget.HealthPercent < 50 && Me.CurrentCastTimeLeft.TotalSeconds > 0.5),
                    Spell.Cast("Lifebloom", 
                        mov => false,
                        on => LifebloomTank, 
                        ret => healtarget.GetPredictedHealthPercent() > 10 && !HealManager.Tank.HasMyAura("Lifebloom", 3)),
                    //Spell.CastOnGround("Wild Mushroom", ret => BestAoeTarget.Location, ret => Settings.Instance.EnableMushrooms && (MushroomCount < 3 || GetRadiusClusterCount(AnyMushrooom, NearbyPartyPlayers, 8f) == 0)),
                    Spell.Cast("Nourish", 
                        mov => true,
                        on => healtarget, 
                        ret => Me.HasAnyAura("Glyph of Rejuvenation", "Heroism", "Bloodlust", "Time Warp", "Ancient Hysteria") && healtarget.GetPredictedHealthPercent() <= 99 && healtarget.HasAnyAura("Rejuvenation", "Regrowth", "Wild Growth", "Lifebloom"), 
                        cancel => healtarget.GetPredictedHealthPercent() <= 50 && Me.CurrentCastTimeLeft.TotalSeconds > Spell.GcdTimeLeft.TotalSeconds),
                    //Mirabis Tank Styler
                    Spell.Cast("Nourish", 
                        mov => true,
                        on => healtarget, 
                        ret => true, 
                        ret => healtarget.GetPredictedHealthPercent(true) < 70),
                    Spell.Cast("Regrowth", 
                        mov => true,
                        on => RegrowthTank, 
                        ret => true, 
                        cancel => (healtarget.GetPredictedHealthPercent(true) < 70 && RegrowthTank.HealthPercent > 60) || (RegrowthTank.HealthPercent > 60 && Me.CurrentCastTimeLeft.TotalSeconds < 0.4)));
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

        public static Composite HandleSotF()
        {
            return new PrioritySelector(
                     new PrioritySelector(context => BestWildGrowthTarget,
                         Spell.Cast("Wild Growth", 
                            mov => false, 
                            on => healtarget, 
                            ret => Me.HasAura("Soul of the Forest") && Clusters.GetClusterCount(healtarget, WildGrowthPlayers(), ClusterType.Radius, 30f) >= 4)),
                         new Decorator(ret => SwiftmendTarget != null,
                            new PrioritySelector(
                                Spell.Cast("Rejuvenation", 
                                    mov => false, 
                                    on => SwiftmendTarget, 
                                    ret => !TalentManager.IsSelected((int)DruidTalents.SouloftheForest) && !SwiftmendTarget.HasAnyAura("Regrowth", "Rejuvenation")),
                                Spell.Cast("Swiftmend", 
                                    mov => false, 
                                    on => SwiftmendTarget, 
                                    ret => !TalentManager.IsSelected((int)DruidTalents.SouloftheForest) && SwiftmendTarget.HasAnyAura("Regrowth", "Rejuvenation")))),
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
                                    ret => SwiftmendTarget.HasAnyAura("Regrowth", "Rejuvenation") && (Me.GetAuraTimeLeft("Harmony").TotalSeconds <= 2.35 || Spell.GetSpellCooldown("Wild Growth").TotalSeconds >= Spell.GcdTimeLeft.TotalSeconds) && Clusters.GetClusterCount(SwiftmendTarget, WildGrowthPlayers(), ClusterType.Radius, 30f) < 5))),
                                 Spell.Cast("Wild Growth", 
                                    mov => false, 
                                    on => healtarget,
                                    ret => !SpellManager.Spells["Swiftmend"].Cooldown && Clusters.GetClusterCount(healtarget, WildGrowthPlayers(), ClusterType.Radius, 30f) >= 5)),
                                 Spell.Cast("Rejuvenation", 
                                    mov => false, 
                                    on => healtarget, 
                                    ret => Me.HasAura("Soul of the Forest") && HealManager.HealTarget.GetPredictedHealthPercent() <= 70),
                                 Spell.Cast("Rejuvenation", 
                                    mov => false, 
                                    on => RejuvTank, 
                                    ret => Me.HasAura("Soul of the Forest")));
        }

        public static WoWUnit BestWildGrowthTarget
        {
            get
            {
                return Clusters.GetBestUnitForCluster(Unit.NearbyFriendlyPlayers, ClusterType.Radius, 30f);
            }
            //NearbyPartyUnits.Where(u => u.IsAlive && u.CanSelect && u.InLineOfSpellSight).OrderByDescending(HealManager.AoEPriority).FirstOrDefault();

        }

        private static IEnumerable<WoWPlayer> WildGrowthPlayers()
        {
            return Unit.NearbyFriendlyPlayers.Where(u => u.IsAlive && u.InLineOfSpellSight && u.GetPredictedHealthPercent() <= 95).ToList();
        }

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
    }
}
