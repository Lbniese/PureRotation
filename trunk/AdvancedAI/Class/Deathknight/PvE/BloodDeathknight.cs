using AdvancedAI.Managers;
using CommonBehaviors.Actions;
using Styx;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using AdvancedAI.Helpers;
using System.Linq;

namespace AdvancedAI.Class.Deathknight.PvE
{
    class BloodDeathknight
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        private static int BloodRuneSlotsActive { get { return Me.BloodRuneCount; } }
        private static int FrostRuneSlotsActive { get { return Me.FrostRuneCount; } }
        private static int UnholyRuneSlotsActive { get { return Me.UnholyRuneCount; } }
        private static int DeathRuneSlotsActive { get { return Me.DeathRuneCount; } }
        //private static int BloodRuneSlotsActive { get { return Me.GetRuneCount(0) + Me.GetRuneCount(1); } }
        //private static int FrostRuneSlotsActive { get { return Me.GetRuneCount(2) + Me.GetRuneCount(3); } }
        //private static int UnholyRuneSlotsActive { get { return Me.GetRuneCount(4) + Me.GetRuneCount(5); } }


        public static Composite BloodDKCombat()
        {
            return new PrioritySelector(
                Common.CreateInterruptBehavior(),
                new Decorator(ret => Me.CurrentTarget != null && (Me.IsCasting || Me.IsChanneling),
                    new ActionAlwaysSucceed()),
                //Spell.WaitForCastOrChannel(),
                CreateApplyDiseases(),
                BloodDKCombatBuffs(),

                new Decorator(ret => AdvancedAI.LFRMode,
                    CreateAFK()),

                new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),

                new Decorator(ret => Unit.UnfriendlyUnits(12).Count() >= 2 && !Me.HasAura("Unholy Blight") && AdvancedAI.Aoe && ShouldSpreadDiseases,
                    new Throttle(1, 2,
                            new PrioritySelector(
                                Spell.Cast("Blood Boil", ret => SpellManager.HasSpell("Roiling Blood")),
                                Spell.Cast("Pestilence", ret => !SpellManager.HasSpell("Roiling Blood"))))),
                
                //using line for talent testing
                //Spell.Cast("Dark Command", ret => SpellManager.HasSpell("Roiling Blood")),
                
                Spell.Cast("Death Strike", ret => Me.HealthPercent < 40 || 
                                                    (Me.UnholyRuneCount + Me.FrostRuneCount + Me.DeathRuneCount >= 4) || 
                                                    (Me.HealthPercent < 90 && (Me.GetAuraTimeLeft("Blood Shield").TotalSeconds < 2)) ||
                                                    IsCurrentTank() && !Me.CachedHasAura("Blood Shield") ||
                                                    Me.HasAura("Blood Charge", 10)),
                DnD(),
                Spell.Cast("Blood Boil", ret => Me.HasAura(81141) && AdvancedAI.Aoe && !SpellManager.CanCast("Death and Decay")),
                Spell.Cast("Rune Tap", ret => Me.HealthPercent <= 80 && Me.BloodRuneCount >= 1),

                new Decorator(ret => Me.CurrentRunicPower >= 30 && !Me.HasAura("Lichborne"),
                    Spell.Cast("Rune Strike", ret => (Me.CurrentRunicPower >= 60 || Me.HealthPercent > 90) && 
                                                     (Me.UnholyRuneCount == 0 || Me.FrostRuneCount == 0 || Me.BloodRuneCount == 0))),

                Spell.Cast("Soul Reaper", ret => Me.BloodRuneCount > 0 && Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent <= 35),
                Spell.Cast("Blood Boil", ret => AdvancedAI.Aoe && !SpellManager.CanCast("Death and Decay") && Unit.UnfriendlyUnits(10).Count() >= 3 && Me.BloodRuneCount > 0),
                Spell.Cast("Heart Strike", ret => Me.BloodRuneCount > 0),
                //Spell.CastOnGround("Death and Decay", ret => Me.CurrentTarget.Location /*ret => AdvancedAI.Aoe && Unit.UnfriendlyUnitsNearTarget(12f).Count() >= 1*/),
                Spell.Cast("Horn of Winter", ret => Me.CurrentRunicPower < 90));
        }

        public static Composite BloodDKPreCombatBuffs()
        {
            return new PrioritySelector(
                Spell.Cast("Bone Shield", ret => !Me.HasAura("Bone Shield")),
                Spell.Cast("Horn of Winter", ret => !Me.HasPartyBuff(PartyBuffType.AttackPower)
            ));
        }

        public static Composite BloodDKCombatBuffs()
        {
            return new PrioritySelector(
                Spell.Cast("Dancing Rune Weapon", ret => IsCurrentTank()),
                Spell.Cast("Blood Tap", ret => Me.HasAura("Blood Charge", 5) && Me.HealthPercent < 90 && !SpellManager.CanCast("Death Strike")),
                Spell.Cast("Bone Shield", ret => !Me.HasAura("Bone Shield")),
                Spell.Cast("Conversion", ret => Me.HealthPercent < 60 && Me.RunicPowerPercent > 20 && !Me.CachedHasAura("Conversion")),
                 Spell.Cast("Conversion", ret => Me.HealthPercent > 90 && Me.CachedHasAura("Conversion")),
                Spell.Cast("Vampiric Blood", ret => Me.HealthPercent < 60
                        && (!Me.HasAnyAura("Bone Shield", "Vampiric Blood", "Dancing Rune Weapon", "Lichborne", "Icebound Fortitude"))),
                Spell.Cast("Icebound Fortitude", ret => Me.HealthPercent < 30
                        && (!Me.HasAnyAura("Bone Shield", "Vampiric Blood", "Dancing Rune Weapon", "Lichborne", "Icebound Fortitude"))),
                Spell.Cast("Might of Ursoc", ret => Me.HealthPercent < 60),
                Spell.Cast("Raise Dead", ret => Me.HealthPercent < 45 && !GhoulMinionIsActive),
                Spell.Cast("Death Pact", ret => Me.HealthPercent < 45),
                //Spell.Cast("Army of the Dead", ret => Me.HealthPercent < 40),
                Spell.Cast("Empower Rune Weapon", ret => Me.HealthPercent < 45 && !SpellManager.CanCast("Death Strike")),
                Spell.Cast("Blood Tap", ret => Me.HasAura("Blood Charge", 10) && (BloodRuneSlotsActive == 0 || FrostRuneSlotsActive == 0 || UnholyRuneSlotsActive == 0)),
                Spell.Cast("Plague Leech", ret => CanCastPlagueLeech));
        }

        private static Composite CreateAFK()
        {
            return new PrioritySelector(

                Spell.Cast("Anti-Magic Shell", ret => Me.CurrentTarget.IsCasting),
                Spell.Cast("Asphyxiate", ret => Unit.UnfriendlyUnits(8).Count() < 3),
                Spell.Cast("Remorseless Winter", ret => Unit.UnfriendlyUnits(8).Count() >= 3),
                Spell.Cast("Desecrated Ground", ret => Me.IsCrowdControlled())

            );
    }

        private static Composite CreateApplyDiseases()
        {
            return new Throttle(
                new PrioritySelector(
                    Spell.Cast("Unholy Blight", ret => SpellManager.CanCast("Unholy Blight") && AdvancedAI.Aoe &&
                                                       Unit.NearbyUnfriendlyUnits.Any(u => (u.IsPlayer || u.IsBoss()) &&
                                                       u.Distance < (u.MeleeDistance() + 5) && u.HasAuraExpired("Blood Plague"))),
                    Spell.Cast("Outbreak", ret => Me.CurrentTarget.HasAuraExpired("Frost Fever") || Me.CurrentTarget.HasAuraExpired("Blood Plague")),
                    Spell.Cast("Icy Touch", ret => !Me.CurrentTarget.IsImmune(WoWSpellSchool.Frost) && Me.CurrentTarget.HasAuraExpired("Frost Fever")),
                    Spell.Cast("Plague Strike", ret => Me.CurrentTarget.HasAuraExpired("Blood Plague"))));
        }

        private static bool ShouldSpreadDiseases
        {
            get
            {
                int radius = TalentManager.HasGlyph("Pestilence") ? 15 : 10;

                return !Me.CurrentTarget.HasAuraExpired("Blood Plague")
                    && !Me.CurrentTarget.HasAuraExpired("Frost Fever")
                    && Unit.NearbyUnfriendlyUnits.Any(u => Me.SpellDistance(u) < radius && u.HasAuraExpired("Blood Plague") && u.HasAuraExpired("Frost Fever"));
            }
        }

        private static bool CanCastPlagueLeech
        {
            get
            {
                if (!Me.GotTarget)
                    return false;

                int frostFever = (int)Me.CurrentTarget.GetAuraTimeLeft("Frost Fever").TotalMilliseconds;
                int bloodPlague = (int)Me.CurrentTarget.GetAuraTimeLeft("Blood Plague").TotalMilliseconds;
                return (frostFever.Between(350, 3000) || bloodPlague.Between(350, 3000))
                    && (BloodRuneSlotsActive == 0 || FrostRuneSlotsActive == 0 || UnholyRuneSlotsActive == 0);
            }
        }

        private const uint Ghoul = 26125;

        private static bool GhoulMinionIsActive
        {
            get { return Me.Minions.Any(u => u.Entry == Ghoul); }
        }

        #region Is Tank
        static bool IsCurrentTank()
        {
            return StyxWoW.Me.CurrentTarget.CurrentTargetGuid == StyxWoW.Me.Guid;
        }
        #endregion

        #region DnD
        private static Composite DnD()
        {
            return new Decorator(ret => SpellManager.CanCast("Death and Decay") && AdvancedAI.Aoe && (Unit.UnfriendlyUnits(12).Count() >= 3 || Me.CachedHasAura(81141)),
                new Action(ret =>
                {
                    var tpos = StyxWoW.Me.CurrentTarget.Location;
                    
                    SpellManager.Cast("Death and Decay");
                    SpellManager.ClickRemoteLocation(tpos);
                }));
        }
        #endregion

        #region DeathKnightTalents

        internal enum DeathKnightTalents
        {
            RoilingBlood = 1,//Tier 1
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
