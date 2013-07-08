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
    class ArcaneMage
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        public static Composite CreateAMCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        ArcaneMagePvP.CreateAMPvPCombat),
                    Spell.Cast("Arcane Missles", ret => Me.HasAura("Arcane Charge", 4)),
                    Spell.Cast("Arcane Barrage", ret => Me.HasAura("Arcane Charge", 4)),
                    Spell.Cast("Living Bomb", ret => !Me.CurrentTarget.HasAura("Living Bomb")),
                    Spell.Cast("Arcane Blast"),
                    Spell.Cast("Arcane Barrage", ret => Me.IsMoving),
                    Spell.Cast("Arcane Explosion", ret => Me.CurrentTarget.Distance < 10 && Me.IsMoving),
                    Spell.Cast("Fire Blast", ret => Me.CurrentTarget.Distance >= 10 && Me.IsMoving)
                    );
            }
        }

        public static Composite CreateAMBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        ArcaneMagePvP.CreateAMPvPBuffs),
                    PartyBuff.BuffGroup("Arcane Brilliance"),
                    Spell.Cast("Mage Armor", ret => !Me.HasAura("Mage Armor"))
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
