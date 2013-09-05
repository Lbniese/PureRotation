using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using AdvancedAI.Helpers;
using System.Linq;
using Action = Styx.TreeSharp.Action;

namespace AdvancedAI.Spec
{
    static class FuryWarrior
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        public static Composite CreateFWCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        FuryWarriorPvP.CreateFWPvPCombat),
                    new Decorator(ret => Me.HasAura("Dire Fixation"),
                        new PrioritySelector(
                            Class.BossMechs.HorridonHeroic())),
                    Common.CreateInterruptBehavior(),
                    Spell.Cast("Shattering Throw", ret => Me.CurrentTarget.IsBoss && Me.HasAnyAura("Heroism", "Bloodlust")),
                    Spell.Cast("Impending Victory", ret => Me.HealthPercent <= 90 && Me.HasAura("Victorious")),
                    Spell.Cast("Berserker Rage", ret => !Me.HasAura("Enrage") && Me.CurrentTarget.HasAura("Colossus Smash")),
                    Spell.Cast("Colossus Smash", ret => Me.CurrentRage > 80 && Me.HasAura("Raging Blow!") && Me.HasAura("Enrage")),
                    HeroicLeap(),
                    DemoBanner(),
                    new Decorator(ret => Unit.UnfriendlyUnits(8).Count() > 2,
                        CreateAoe()),
                    new Decorator(ret => Me.CurrentTarget.HealthPercent <= 20,
                        CreateExecuteRange()),
                    new Decorator(ret => Me.CurrentTarget.HealthPercent > 20,
                        new PrioritySelector(
                            Item.UsePotionAndHealthstone(40),
                            new Decorator(ret => AdvancedAI.Burst,
                                new PrioritySelector(
                                    Spell.Cast("Blood Fury", ret => Me.CurrentTarget.IsBoss),
                                    Spell.Cast("Recklessness", ret => Me.CurrentTarget.IsBoss),
                                    Spell.Cast("Avatar", ret => Me.CurrentTarget.IsBoss),
                                    Spell.Cast("Bloodbath"),
                                    Spell.Cast("Skull Banner", ret => Me.CurrentTarget.IsBoss),
                                    new Action(ret => { Item.UseHands(); return RunStatus.Failure; }))),
                            new Decorator(ret => !Me.CurrentTarget.HasAura("Colossus Smash"),
                                new PrioritySelector(
                                    Spell.Cast("Bloodthirst"),
                                    Spell.Cast("Heroic Strike", ctx => Me.CurrentRage > 105 && ColossusSmashCheck()),
                                    Spell.Cast("Raging Blow", ret => Me.HasAura("Raging Blow!", 2) && ColossusSmashCheck()),
                                    Spell.Cast("Wild Strike", ret => Me.HasAura("Bloodsurge")),
                                    Spell.Cast("Dragon Roar", ret => Me.CurrentTarget.Distance <= 8),
                                    Spell.Cast("Raging Blow", ret => Me.HasAura("Raging Blow!", 1) && ColossusSmashCheck()),
                                    Spell.Cast("Battle Shout", ret => Me.RagePercent < 30 && Spell.GetSpellCooldown("Colossus Smash").TotalSeconds <= 2),
                                    Spell.Cast("Shockwave"),
                                    Spell.Cast("Wild Strike", ret => Me.CurrentRage >= 115 && ColossusSmashCheck()))),
                            new Decorator(ret => Me.CurrentTarget.HasAura("Colossus Smash"),
                                new PrioritySelector(
                                    Spell.Cast("Heroic Strike", ctx => Me.CurrentRage > 30),
                                    Spell.Cast("Bloodthirst"),
                                    Spell.Cast("Raging Blow"),
                                    Spell.Cast("Wild Strike", ret => Me.HasAura("Bloodsurge")))))));
            }
        }

        internal static Composite CreateFWBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator( ret => AdvancedAI.PvPRot,
                        FuryWarriorPvP.CreateFWPvPBuffs),
                    Spell.Cast("Battle Shout", ret => !StyxWoW.Me.HasAura("Battle Shout")));
            }
        }

        private static Composite CreateAoe()
        {
            return new PrioritySelector(
                new Decorator(ret => Unit.UnfriendlyUnits(8).Count() >= 5,
                    new PrioritySelector(
                        Spell.Cast("Whirlwind"),
                        Spell.Cast("Bloodthirst"),
                        Spell.Cast("Raging Blow"))),
                Spell.Cast("Whirlwind", ret => !Me.HasAura("Meat Cleaver", (int)MathEx.Clamp(1, 3, Unit.UnfriendlyUnits(8).Count() - 1))),
                Spell.Cast("Bloodthirst"),
                Spell.Cast("Raging Blow", ret => Me.HasAura("Meat Cleaver", (int)MathEx.Clamp(1, 3, Unit.UnfriendlyUnits(8).Count() - 1))),
                Spell.Cast("Cleave", ret => Me.CurrentRage >= 105 && Spell.GetSpellCooldown("Colossus Smash").TotalSeconds >= 3));
        }

        private static Composite CreateExecuteRange()
        {
            return new PrioritySelector(
                new Decorator(ret => !Me.CurrentTarget.HasAura("Colossus Smash"),
                    new PrioritySelector(
                        Spell.Cast("Bloodthirst"),
                        Spell.Cast("Raging Blow"),
                        new Decorator(ret => Me.RagePercent < 85,
                            new Action(ret => RunStatus.Success)))),
                new Decorator(ret => Me.CurrentTarget.HasAura("Colossus Smash"),
                    new PrioritySelector(
                        Spell.Cast("Execute"))));
        }

        private static Composite HeroicLeap()
        {
            return
                new Decorator(ret => SpellManager.CanCast("Heroic Leap") &&
                    Lua.GetReturnVal<bool>("return IsLeftControlKeyDown() and not GetCurrentKeyBoardFocus()", 0),
                    new Action(ret =>
                    {
                        SpellManager.Cast("Heroic Leap");
                        Lua.DoString("if SpellIsTargeting() then CameraOrSelectOrMoveStart() CameraOrSelectOrMoveStop() end");
                        return;
                    }));
        }

        private static Composite DemoBanner()
        {
            return
                new Decorator(ret => SpellManager.CanCast("Demoralizing Banner") &&
                    Lua.GetReturnVal<bool>("return IsLeftShiftKeyDown() and not GetCurrentKeyBoardFocus()", 0),
                    new Action(ret =>
                    {
                        SpellManager.Cast("Demoralizing Banner");
                        Lua.DoString("if SpellIsTargeting() then CameraOrSelectOrMoveStart() CameraOrSelectOrMoveStop() end");
                        return;
                    }));
        }

        private static bool ColossusSmashCheck()
        {
            return (Spell.GetSpellCooldown("Colossus Smash").TotalSeconds >= 3 ||
                    !SpellManager.Spells["Colossus Smash"].Cooldown);
        }

        #region WarriorTalents
        public enum WarriorTalents
        {
            None = 0,
            Juggernaut,
            DoubleTime,
            Warbringer,
            EnragedRegeneration,
            SecondWind,
            ImpendingVictory,
            StaggeringShout,
            PiercingHowl,
            DisruptingShout,
            Bladestorm,
            Shockwave,
            DragonRoar,
            MassSpellReflection,
            Safeguard,
            Vigilance,
            Avatar,
            Bloodbath,
            StormBolt
        }
        #endregion
    }
}
