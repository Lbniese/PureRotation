using System.Windows.Forms;
using AdvancedAI.Helpers;
using AdvancedAI;
using CommonBehaviors.Actions;
using Styx;
using Styx.Common;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = Styx.TreeSharp.Action;
using AdvancedAI.Managers;
using Styx.CommonBot;

namespace AdvancedAI.Spec
{
    class ProtectionWarrior
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        internal static Composite CreatePWCombat 
        { 
            get 
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        ProtectionWarriorPvP.CreatePWPvPCombat),
                    new Decorator(ret => Me.HasAura("Dire Fixation"),
                        new PrioritySelector(
                            Class.BossMechs.HorridonHeroic())),
                    new Throttle(1,1,
                        new PrioritySelector(
                    Common.CreateInterruptBehavior())),

                    new Decorator(ret => AdvancedAI.Burst,
                        new PrioritySelector(
                    Spell.Cast("Recklessness", ret => Me.CurrentTarget.IsWithinMeleeRange),
                    Spell.Cast("Bloodbath", ret => Me.CurrentTarget.IsWithinMeleeRange),
                    Spell.Cast("Avatar", ret => Me.HasAura("Recklessness") && Me.CurrentTarget.IsWithinMeleeRange),
                    Spell.Cast("Skull Banner", ret => Me.HasAura("Recklessness") && Me.CurrentTarget.IsWithinMeleeRange))),
                    new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),

                    Spell.Cast("Demoralizing Shout", ret => Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 10 * 10) >= 1),
                    Spell.Cast("Shield Block", ret => Me.CurrentRage > 60),

                    new Decorator(ret => Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8 * 8) >= 2,
                        CreateAoe()),

                    DemoBanner(),
                    HeroicLeap(),
                    MockingBanner(),   
                    
                    Spell.Cast("Shield Slam"),
                    Spell.Cast("Revenge", ret => Me.CurrentRage < 90),
                    Spell.Cast("Dragon Roar", ret =>  Me.CurrentTarget.Distance <= 8),
                    Spell.Cast("Execute"),
                    Spell.Cast("Thunder Clap", ret => Me.CurrentTarget.HasAura("Weakened Blows")),
                    Spell.Cast("Commanding Shout"),
                    Spell.Cast("Heroic Strike", ret => Me.CurrentRage > 85 || Me.HasAura(122510) || Me.HasAura(122016)),
                    Spell.Cast("Devastate"));
            }
        }

        internal static Composite CreatePWBuffs 
        { 
            get 
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        ProtectionWarriorPvP.CreatePWPvPBuffs));
            }
        }

        private static Composite CreateAoe()
        {
            return new PrioritySelector(
                Spell.Cast("Thunder Clap"),
                Spell.Cast("Cleave", ret => Me.CurrentRage > 85 || Me.HasAura(122510) || Me.HasAura(122016)),
                Spell.Cast("Thunder Clap", ret => Me.CurrentTarget.HasAura("Weakened Blows"))
                );
        }

        private static Composite HeroicLeap()
        {
            return
                new Decorator(ret => SpellManager.CanCast("Heroic Leap") &&
                    Lua.GetReturnVal<bool>("return IsLeftAltKeyDown() and not GetCurrentKeyBoardFocus()", 0),
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
                    KeyboardPolling.IsKeyDown(Keys.Z),
                    new Action(ret =>
                    {
                        SpellManager.Cast("Demoralizing Banner");
                        Lua.DoString("if SpellIsTargeting() then CameraOrSelectOrMoveStart() CameraOrSelectOrMoveStop() end");
                        return;
                    }));
        }

        private static Composite MockingBanner()
        {
            return
                new Decorator(ret => SpellManager.CanCast("Mocking Banner") &&
                    KeyboardPolling.IsKeyDown(Keys.C),
                    new Action(ret =>
                    {
                        SpellManager.Cast("Mocking Banner");
                        Lua.DoString("if SpellIsTargeting() then CameraOrSelectOrMoveStart() CameraOrSelectOrMoveStop() end");
                        return;
                    }));
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
