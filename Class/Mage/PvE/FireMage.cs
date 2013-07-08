using AdvancedAI.Helpers;
using Styx;
using Styx.TreeSharp;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAI.Spec
{
    class FireMage
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        public static Composite CreateFiMCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        FireMagePvP.CreateFiMPvPCombat),
                    Spell.Cast("Evocation", ret => !Me.HasAura("Invoker's Energy")),
                    Spell.Cast("Living Bomb", ret => !Me.CurrentTarget.HasAura("Living Bomb")),
                    Spell.Cast("Pyroblast", ret => Me.HasAura("Pyroblast!")),
                    Spell.Cast("Inferno Blast", ret => Me.HasAura("Heating Up")),
                    Spell.Cast("Fireball"),
                    Spell.Cast("Scorch", ret => Me.IsMoving)
                    );

            }
        }

        public static Composite CreateFiMBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        FireMagePvP.CreateFiMPvPBuffs),
                    PartyBuff.BuffGroup("Arcane Brilliance"),
                    Spell.Cast("Molten Armor", ret => !Me.HasAura("Molten Armor"))
                    );
            }
        }

        #region MageTalents
        public enum MageTalents
        {
            PresenceofMind = 1,//Tier 1
            BazingSpeed,
            IceFloes,
            TemporalShield,//Tier 2
            Flameglow,
            IceBarrier,
            RingofFrost,//Tier 3
            IceWard,
            Frostjaw,
            GreaterInvisibility,//Tier 4
            Cauterize,
            ColdSnap,
            NetherTempest,//Tier 5
            LivingBomb,
            FrostBomb,
            Invocation,//Tier 6
            RuneofPower,
            IncantersWard
        }
        #endregion
    }
}
