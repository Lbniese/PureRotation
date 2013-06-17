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
    class FireMage// : AdvancedAI
    {
        //public override WoWClass Class { get { return WoWClass.Mage; } }
        //public override WoWSpec Spec { get { return WoWSpec.MageFire; } }
        static LocalPlayer Me { get { return StyxWoW.Me; } }

        public static Composite CreateFMCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Cast("Evocation", ret => !Me.HasAura("Invoker's Energy")),
                    Spell.Cast("Living Bomb", ret => !Me.CurrentTarget.HasAura("Living Bomb")),
                    Spell.Cast("Pyroblast", ret => Me.HasAura("Pyroblast!")),
                    Spell.Cast("Inferno Blast", ret => Me.HasAura("Heating Up")),
                    Spell.Cast("Fireball"),
                    Spell.Cast("Scorch", ret => Me.IsMoving)
                    );

            }
        }

        public static Composite CreateFMBuffs
        {
            get
            {
                return new PrioritySelector(
                    PartyBuff.BuffGroup("Arcane Brilliance"),
                    Spell.Cast("Molten Armor", ret => !Me.HasAura("Molten Armor"))
                    );
            }
        }

    }
}
