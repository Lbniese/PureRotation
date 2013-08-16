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
                    new Decorator(ret => Unit.UnfriendlyMeleeUnits.Count() > 2,
                        CreateAoe()),    
                        
                    Spell.Cast("Shield Slam"),
                    Spell.Cast("Revenge", ret => Me.RagePercent < 90),
                    Spell.Cast("Devastate"),
                    Spell.Cast("Thunder Clap", ret => Me.CurrentTarget.HasAura("Weakened Blows")),
                    Spell.Cast("Commanding Shout"),
                    Spell.Cast("Heroic Strike", ret => Me.RagePercent > 85));
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
                Spell.Cast("Thunder Clap", ret => Me.CurrentTarget.HasAura("Weakened Blows"))
                );
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
