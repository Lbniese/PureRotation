using AdvancedAI.Managers;
using CommonBehaviors.Actions;
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
    class BloodDeathknight
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        internal static int BloodRuneSlotsActive { get { return Me.GetRuneCount(0) + Me.GetRuneCount(1); } }
        internal static int FrostRuneSlotsActive { get { return Me.GetRuneCount(2) + Me.GetRuneCount(3); } }
        internal static int UnholyRuneSlotsActive { get { return Me.GetRuneCount(4) + Me.GetRuneCount(5); } }
        private static List<WoWUnit> _nearbyUnfriendlyUnits;

        internal static Composite CreateBDKCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        BloodDeathknightPvP.CreateBDKPvPCombat),
                    Common.CreateInterruptBehavior(),
                    Spell.WaitForCastOrChannel(),

                    new PrioritySelector(
                            ctx => _nearbyUnfriendlyUnits = Unit.UnfriendlyUnitsNearTarget(15f).ToList(),
                            new Decorator(
                                ret => AdvancedAI.Aoe && _nearbyUnfriendlyUnits.Count() >= 3,
                                new PrioritySelector(
                                    Spell.CastOnGround("Death and Decay", ret => StyxWoW.Me.CurrentTarget.Location, ret => true, false),

                                    // Spell.Cast("Gorefiend's Grasp", ret => Common.HasTalent( DeathKnightTalents.GorefiendsGrasp)),
                                    Spell.Cast("Remorseless Winter", ret => TalentManager.IsSelected((int)DeathKnightTalents.RemoreselessWinter)),

                                    // Apply Diseases
                                    CreateApplyDiseases(),

                                    // Spread Diseases
                                    Spell.Cast("Blood Boil",
                                        ret => TalentManager.IsSelected((int)DeathKnightTalents.RollingBlood)
                                            && !StyxWoW.Me.HasAura("Unholy Blight")
                                            && StyxWoW.Me.CurrentTarget.DistanceSqr <= 10 * 10
                                            && ShouldSpreadDiseases),

                                    Spell.Cast("Pestilence",
                                        ret => !StyxWoW.Me.HasAura("Unholy Blight")
                                            && ShouldSpreadDiseases),

                                    // Active Mitigation
                                    Spell.Cast("Death Strike"),


                                    // AoE Damage
                                    Spell.Cast("Blood Boil", ret => AdvancedAI.Aoe && _nearbyUnfriendlyUnits.Count >= 3),
                                    Spell.Cast("Heart Strike", ret => _nearbyUnfriendlyUnits.Count < 3),
                                    Spell.Cast("Rune Strike"),
                                    Spell.Cast("Icy Touch", ret => !StyxWoW.Me.CurrentTarget.IsImmune(WoWSpellSchool.Frost)),

                                    new ActionAlwaysSucceed()
                                    )
                                )
                            ),

                    Spell.Cast("Blood Boil",
                        ret => AdvancedAI.Aoe
                            && SpellManager.HasSpell("Scarlet Fever")
                            && StyxWoW.Me.CurrentTarget.DistanceSqr <= 10 * 10
                            && Unit.NearbyUnfriendlyUnits.Any(u =>
                            {
                                long frostTimeLeft = (long)u.GetAuraTimeLeft("Frost Fever").TotalMilliseconds;
                                long bloodTimeLeft = (long)u.GetAuraTimeLeft("Blood Plauge").TotalMilliseconds;
                                return frostTimeLeft > 500 && bloodTimeLeft > 500 && (frostTimeLeft < 3000 || bloodTimeLeft < 3000);
                            })),


                    Spell.Cast("Outbreak", ret => !Me.CurrentTarget.HasAnyAura("Frost Fever", "Blood Plague")),
                    Spell.Cast("Blood Boil", ret => Me.HasAura("Crimson Scourge") && !Me.CurrentTarget.HasAnyAura("Frost Fever", "Blood Plague")),
                    Spell.Cast("Plague Strike", ret => Me.CurrentTarget.HasAura("Blood Plague")),
                    Spell.Cast("Icy Touch", ret => Me.CurrentTarget.HasAura("Frost Fever")),
                    Spell.Cast("Death Strike", ret => Me.HealthPercent < 40 || 
                                                      (Me.UnholyRuneCount + Me.FrostRuneCount + Me.DeathRuneCount >= 4) || 
                                                      (Me.HealthPercent < 90 && (Me.GetAuraTimeLeft("Blood Shield").TotalSeconds < 2))),
                    Spell.Cast("Blood Tap", ret => (Me.CurrentRunicPower >= 60 || Me.HealthPercent > 90) 
                                                    && Me.CurrentRunicPower >= 30 && (Me.UnholyRuneCount == 0 || Me.FrostRuneCount == 0) && 
                                                    !Me.HasAura("Lichborne")),
                    Spell.Cast("Rune Tap", ret => Me.HealthPercent <= 80 && Me.BloodRuneCount >= 1),
                    Spell.Cast("Rune Strike", ret => (Me.CurrentRunicPower >= 60 || Me.HealthPercent > 90) && Me.CurrentRunicPower >= 30 && 
                                                     (Me.UnholyRuneCount == 0 || Me.FrostRuneCount == 0) && !Me.HasAura("Lichborne")),
                    Spell.Cast("Soul Reaper", ret => Me.BloodRuneCount > 0 && Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent < 35), 
                    //heartstrike
                    Spell.Cast(55050, ret => _nearbyUnfriendlyUnits.Count < 3),
                    Spell.CastOnGround("Death and Decay", on => Me.CurrentTarget.Location),
                    Spell.Cast("Death Coil", ret => !SpellManager.HasSpell("Rune Strike") && StyxWoW.Me.CurrentRunicPower >= 80),
                    Spell.Cast("Death Coil", ret => !StyxWoW.Me.CurrentTarget.IsWithinMeleeRange)

                    );
            }
        }

        internal static Composite CreateBDKBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        BloodDeathknightPvP.CreateBDKPvPBuffs),
                    Spell.Cast("Anti-Magic Shell",
                        ret => Unit.NearbyUnfriendlyUnits.Any(u => (u.IsCasting || u.ChanneledCastingSpellId != 0) && u.CurrentTargetGuid == StyxWoW.Me.Guid)),
                    Spell.CastOnGround("Anti-Magic Zone", 
                        loc => StyxWoW.Me.Location,
                        ret => TalentManager.IsSelected((int)DeathKnightTalents.AntiMagicZone) 
                            && !StyxWoW.Me.HasAura("Anti-Magic Shell") 
                            && Unit.NearbyUnfriendlyUnits.Any(u => (u.IsCasting || u.ChanneledCastingSpellId != 0) && u.CurrentTargetGuid == StyxWoW.Me.Guid) 
                            && Targeting.Instance.FirstUnit != null 
                            && Targeting.Instance.FirstUnit.IsWithinMeleeRange),
                    Spell.Cast("Dancing Rune Weapon",
                        ret => Unit.NearbyUnfriendlyUnits.Count() > 2),
                    Spell.Cast("Bone Shield",
                        ret => !Me.HasAnyAura("Bone Shield", "Vampiric Blood", "Dancing Rune Weapon", "Lichborne", "Icebound Fortitude")),
                    Spell.Cast("Vampiric Blood",
                        ret => Me.HealthPercent < 60
                            && (!Me.HasAnyAura("Bone Shield", "Vampiric Blood", "Dancing Rune Weapon", "Lichborne", "Icebound Fortitude"))),
                    Spell.Cast("Icebound Fortitude",
                        ret => StyxWoW.Me.HealthPercent < 30
                            && (!Me.HasAnyAura("Bone Shield", "Vampiric Blood", "Dancing Rune Weapon", "Lichborne", "Icebound Fortitude"))),
                    Spell.Cast("Lichborne",ret => StyxWoW.Me.IsCrowdControlled()),
                    Spell.Cast("Desecrated Ground", ret => TalentManager.IsSelected((int)DeathKnightTalents.DesecratedGround) && StyxWoW.Me.IsCrowdControlled()),
                    // Symbiosis
                    Spell.Cast("Might of Ursoc", ret => Me.HealthPercent < 60),
                    Spell.Cast("Army of the Dead",
                        ret => StyxWoW.Me.HealthPercent < 60),
                    // I need to use Empower Rune Weapon to use Death Strike
                    Spell.Cast("Empower Rune Weapon",
                        ret => StyxWoW.Me.HealthPercent < 60
                            && !SpellManager.CanCast("Death Strike")),
                    new PrioritySelector(
                        ctx => StyxWoW.Me.PartyMembers.FirstOrDefault(u => u.IsDead && u.DistanceSqr < 40 * 40 && u.InLineOfSpellSight),
                        Spell.Cast("Raise Ally", ctx => ctx as WoWUnit)),
                    // *** Offensive Cooldowns ***
                    // I am using pet as dps bonus
                    Spell.Cast("Raise Dead",
                        ret => Common.UseLongCoolDownAbility
                            && !GhoulMinionIsActive),
                    Spell.Cast("Death's Advance",
                        ret => TalentManager.IsSelected((int)DeathKnightTalents.DeathsAdvance)
                            && StyxWoW.Me.GotTarget && !SpellManager.CanCast("Death Grip", false)
                            && StyxWoW.Me.CurrentTarget.DistanceSqr > 10 * 10),
                    Spell.Cast("Blood Tap",
                        ret => StyxWoW.Me.HasAura("Blood Charge", 5)
                            && (BloodRuneSlotsActive == 0 || FrostRuneSlotsActive == 0 || UnholyRuneSlotsActive == 0)),
                    Spell.Cast("Plague Leech", ret => CanCastPlagueLeech));
            }
        }

        public static Composite CreateApplyDiseases()
        {
            // throttle to avoid/reduce following an Outbreak with a Plague Strike for example
            return new Throttle(
                new PrioritySelector(
                // abilities that don't require Runes first
                    Spell.BuffSelf(
                        "Unholy Blight",
                        ret => SpellManager.CanCast("Unholy Blight")
                            && Unit.NearbyUnfriendlyUnits.Any(u => (u.IsPlayer || u.IsBoss()) && u.Distance < (u.MeleeDistance() + 5) && u.HasAuraExpired("Blood Plague"))),

                    Spell.Cast("Outbreak", ret => Me.CurrentTarget.HasAuraExpired("Frost Fever") || Me.CurrentTarget.HasAuraExpired("Blood Plague")),

                // now Rune based abilities
                    new Decorator(
                        ret => !Me.CurrentTarget.IsImmune(WoWSpellSchool.Frost) && Me.CurrentTarget.HasAuraExpired("Frost Fever"),
                        new PrioritySelector(
                            Spell.Cast("Howling Blast", ret => AdvancedAI.Aoe && Me.Specialization == WoWSpec.DeathKnightFrost),
                            Spell.Cast("Icy Touch", ret => !AdvancedAI.Aoe || Me.Specialization != WoWSpec.DeathKnightFrost)
                            )
                        ),

                    Spell.Cast("Plague Strike", ret => Me.CurrentTarget.HasAuraExpired("Blood Plague"))
                    )
                );
        }

        internal static bool ShouldSpreadDiseases
        {
            get
            {
                int radius = TalentManager.HasGlyph("Pestilence") ? 15 : 10;

                return !Me.CurrentTarget.HasAuraExpired("Blood Plague")
                    && !Me.CurrentTarget.HasAuraExpired("Frost Fever")
                    && Unit.NearbyUnfriendlyUnits.Any(u => Me.SpellDistance(u) < radius && u.HasAuraExpired("Blood Plague") && u.HasAuraExpired("Frost Fever"));
            }
        }

        internal static bool CanCastPlagueLeech
        {
            get
            {
                if (!Me.GotTarget)
                    return false;

                int frostFever = (int)Me.CurrentTarget.GetAuraTimeLeft("Frost Fever").TotalMilliseconds;
                int bloodPlague = (int)Me.CurrentTarget.GetAuraTimeLeft("Blood Plague").TotalMilliseconds;
                // if there is 3 or less seconds left on the diseases and we have a fully depleted rune then return true.
                return (frostFever.Between(350, 3000) || bloodPlague.Between(350, 3000))
                    && (BloodRuneSlotsActive == 0 || FrostRuneSlotsActive == 0 || UnholyRuneSlotsActive == 0);
            }
        }

        internal const uint Ghoul = 26125;
        internal static bool GhoulMinionIsActive
        {
            get { return Me.Minions.Any(u => u.Entry == Ghoul); }
        }

        #region DeathKnightTalents
        public enum DeathKnightTalents
        {
            RollingBlood = 1,//Tier 1
            PlagueLeech,
            UnholyBlight,
            LichBorne,//Tier 2
            AntiMagicZone,
            Purgatory,
            DeathsAdvance,//Tier 3
            Chilblains,
            Asphyxiate,
            DeathPact,//Tier 4
            DeathSiphon,
            Conversion,
            BloodTap,//Tier 5
            RunicEmpowerment,
            RunicCorruption,
            GorefiendsGrasp,//Tier 6
            RemoreselessWinter,
            DesecratedGround
        }
        #endregion
    }
}
