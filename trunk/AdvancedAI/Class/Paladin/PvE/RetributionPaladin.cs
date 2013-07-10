using Styx;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using AdvancedAI.Helpers;
using System.Linq;
using Action = Styx.TreeSharp.Action;

namespace AdvancedAI.Spec
{
    class RetributionPaladin
    {        
        static LocalPlayer Me { get { return StyxWoW.Me; } }

        internal static Composite CreateRPCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        RetributionPaladinPvP.CreateRPPvPCombat),
                    Spell.Cast("Rebuke", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast),
                    Spell.Cast("Inquisition", ret => (!Me.HasAura("Inquisition") || Me.HasAuraExpired("Inquisition", 2)) && (Me.CurrentHolyPower >= 3 || Me.ActiveAuras.ContainsKey("Divine Purpose"))),
                    Spell.Cast("Avenging Wrath", ret => Me.HasAura("Inquisition") && Me.CurrentTarget.IsBoss),
                    Spell.Cast("Holy Avenger", ret => Me.HasAura("Inquisition") && Me.CurrentTarget.IsBoss),
                    Spell.Cast("Guardian of Ancient Kings", ret => Me.HasAura("Inquisition") && Me.CurrentTarget.IsBoss),
                    new Decorator(ret => Me.HasAura("Inquisition"),
                        new PrioritySelector(
                            new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),
                            new Action(ret => { Item.UseTrinkets(); return RunStatus.Failure; }))),
                    Spell.Cast("Execution Sentence", ret => Me.HasAura("Inquisition") && Me.CurrentTarget.IsBoss),
                    Spell.Cast("Holy Prism", ret => Me.HasAura("Inquisition")),
                    Spell.CastOnGround("Light's Hammer", ret => Me.CurrentTarget.Location, ret => Me.HasAura("Inquisition") && Me.CurrentTarget.IsBoss),
                    Spell.Cast("Divine Storm", ret => Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8 * 8) >= 2 && Me.HasAura("Inquisition") && (Me.CurrentHolyPower == 5 || Me.ActiveAuras.ContainsKey("Divine Purpose"))),
                    Spell.Cast("Templar's Verdict", ret => Me.HasAura("Inquisition") && (Me.CurrentHolyPower == 5 || Me.ActiveAuras.ContainsKey("Divine Purpose"))),
                    Spell.Cast("Hammer of Wrath", ret => Me.CurrentHolyPower <= 4),
                    Spell.Cast("Exorcism", ret => Me.CurrentHolyPower <= 4),
                    Spell.Cast("Hammer of the Righteous", ret => Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8 * 8) >= 4),
                    Spell.Cast("Crusader Strike", ret => Me.CurrentHolyPower <= 4),
                    Spell.Cast("Judgment", on => SecTar, ret => Clusters.GetClusterCount(Me, Unit.NearbyUnfriendlyUnits, ClusterType.Radius, 8f) >= 2 && Me.HasAura("Glyph of Double Jeopardy")),
                    Spell.Cast("Judgment", ret => Me.CurrentHolyPower <= 4),
                    Spell.Cast("Divine Storm", ret => Me.HasAura("Inquisition") && Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8 * 8) >= 2 && Me.GetAuraTimeLeft("Inquisition").TotalSeconds > 4),
                    Spell.Cast("Templar's Verdict", ret => Me.HasAura("Inquisition") && Me.GetAuraTimeLeft("Inquisition").TotalSeconds > 4),
                    Spell.Cast("Sacred Shield", on => Me, ret => !Me.HasAura("Sacred Shield")));
            }
        }

        #region SecTar
        public static WoWUnit SecTar
        {
            get
            {
                if (!StyxWoW.Me.GroupInfo.IsInParty)
                    return null;
                if (StyxWoW.Me.GroupInfo.IsInParty)
                {
                    var secondTarget = (from unit in ObjectManager.GetObjectsOfType<WoWUnit>(false)
                                        where unit.IsAlive
                                        where unit.IsHostile
                                        where unit.Distance < 30
                                        where unit.IsTargetingMyPartyMember || unit.IsTargetingMyRaidMember
                                        where unit.InLineOfSight
                                        where unit.Guid != Me.CurrentTarget.Guid
                                        select unit).FirstOrDefault();
                    return secondTarget;
                }
                return null;
            }
        }
        #endregion

        public static Composite CreateRPBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        RetributionPaladinPvP.CreateRPPvPBuffs));
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
