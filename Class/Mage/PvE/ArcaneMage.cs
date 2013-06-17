﻿using CommonBehaviors.Actions;
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
    class ArcaneMage : AdvancedAI
    {
        public override WoWClass Class { get { return WoWClass.Mage; } }
        //public override WoWSpec Spec { get { return WoWSpec.MageArcane; } }
        LocalPlayer Me { get { return StyxWoW.Me; } }

        protected override Composite CreateCombat()
        {
            return new PrioritySelector(
                Spell.Cast("Arcane Missles", ret => Me.HasAura("Arcane Charge", 4)),
                Spell.Cast("Arcane Barrage", ret => Me.HasAura("Arcane Charge", 4)),
                Spell.Cast("Living Bomb", ret => !Me.CurrentTarget.HasAura("Living Bomb")),
                Spell.Cast("Arcane Blast"),
                Spell.Cast("Arcane Barrage", ret => Me.IsMoving),
                Spell.Cast("Arcane Explosion", ret => Me.CurrentTarget.Distance < 10 && Me.IsMoving),
                Spell.Cast("Fire Blast", ret => Me.CurrentTarget.Distance >= 10 && Me.IsMoving)
                );
        }

        protected override Composite CreateBuffs()
        {
            return new PrioritySelector(
                PartyBuff.BuffGroup("Arcane Brilliance"),
                Spell.Cast("Mage Armor", ret => !Me.HasAura("Mage Armor"))
                );
        }
    }
}
