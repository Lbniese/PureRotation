using AdvancedAI.Managers;
using Styx;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using AdvancedAI.Helpers;
using System.Linq;

namespace AdvancedAI.Class.Shaman.PvE
{
    class ElementalShaman
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }

        public static Composite ElementalCombat()
        {
            return new PrioritySelector(
                Common.CreateInterruptBehavior(),
                new Decorator(ret => AdvancedAI.Burst && Me.CurrentTarget.IsBoss(),
                    new PrioritySelector(
                        Spell.Cast("Elemental Mastery"),
                        Spell.Cast("Ascendance", ret => Me.CurrentTarget.CachedGetAuraTimeLeft("Flame Shock") > 18 && Me.CachedHasAura("Elemental Mastery")),
                        Spell.Cast("Fire Elemental Totem", ret => Me.CurrentTarget.TimeToDeath() > (TalentManager.HasGlyph("Fire Elemental Totem") ? 30 : 60)),
                        Spell.Cast("Earth Elemental Totem", ret => Me.CurrentTarget.TimeToDeath() > 60 && Spell.GetSpellCooldown("Fire Elemental Totem").TotalSeconds > 61),
                        Spell.Cast("Stormlash Totem", ret => PartyBuff.WeHaveBloodlust))),
                
                Spell.WaitForCast(),
                Spell.Cast("Thunderstorm", ret => Me.ManaPercent < 60 && TalentManager.HasGlyph("Thunderstorm")),

                new Decorator(ret => Unit.UnfriendlyUnitsNearTargetFacing(10).Count() >= 4,
                    AOE()),

                Spell.Cast("Spiritwalker's Grace", ret => Me.IsMoving && !SpellManager.Spells["Lava Burst"].Cooldown),
                Spell.Cast("Flame Shock", on => FlameShockTar, ret => FlameShockTar.CachedHasAuraDown("Flame Shock", 1, true, 3)),
                Spell.Cast("Lava Burst"),
                Spell.Cast("Elemental Blast"),
                Spell.Cast("Earth Shock", ret => Me.CachedHasAura("Lightning Shield", Unit.UnfriendlyUnitsNearTargetFacing(10).Count() > 2 ? 7 : 6)),
                Spell.Cast("Searing Totem", ret => !Totems.ExistInRange(Me.CurrentTarget.Location, WoWTotem.Searing)),
                Spell.Cast(Unit.UnfriendlyUnitsNearTargetFacing(10).Count() > 1 ? "Chain Lightning" : "Lightning Bolt"));
        }

        private static Composite AOE()
        {
            return new PrioritySelector(
                new Decorator(ret => Unit.UnfriendlyUnits(10).Count() > 5 && TalentManager.HasGlyph("Thunderstorm"),
                    Spell.Cast("Thunderstorm")),
                Spell.Cast("Chain Lightning"));
        }

        #region Flame Shock Target
        private static WoWUnit FlameShockTar
        {
            get
            {
                if (Unit.UnfriendlyUnitsNearTargetFacing(10).Count() < 2 && Me.CurrentTarget.CachedHasAuraDown("Flame Shock", 1, true, 3))
                    return Me.CurrentTarget;
                if (Unit.UnfriendlyUnitsNearTargetFacing(10).Count().Between(2, 3))
                {
                    var besttar = (from unit in ObjectManager.GetObjectsOfType<WoWUnit>(false)
                                   where unit.IsAlive
                                   where unit.IsTargetingMyPartyMember || unit.IsTargetingMyRaidMember
                                   where unit.CachedHasAuraDown("Flame Shock", 1, true, 3)
                                   where unit.InLineOfSight
                                   select unit).FirstOrDefault();
                    return besttar;
                }
                return null;
            }
        }
        #endregion

        #region ShamanTalents
        private enum ShamanTalents
        {
            NaturesGuardian = 1,
            StoneBulwarkTotem,
            AstralShift,
            FrozenPower,
            EarthgrabTotem,
            WindwalkTotem,
            CallOfTheElements,
            TotemicRestoration,
            TotemicProjection,
            ElementalMastery,
            AncestralSwiftness,
            EchoOfTheElements,
            HealingTideTotem,
            AncestralGuidance,
            Conductivity,
            UnleashedFury,
            PrimalElementalist,
            ElementalBlast
        }
        #endregion
    }
}
