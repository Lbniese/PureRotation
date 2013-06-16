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
    class FuryWarrior : AdvancedAI
    {
        public override WoWClass Class { get { return WoWClass.Warrior; } }
        //public override WoWSpec Spec { get { return WoWSpec.WarriorFury; } }
        LocalPlayer Me { get { return StyxWoW.Me; } }        

        protected override Composite CreateCombat()
        {
            return new PrioritySelector(
                // Don't do anything if we have no target, nothing in melee range, or we're casting. (Includes vortex!)
                //new Decorator(
                //    ret =>
                //    !StyxWoW.Me.GotTarget || StyxWoW.Me.IsCasting ||
                //    StyxWoW.Me.CurrentPendingCursorSpell != null,
                //    new ActionAlwaysSucceed()),
                // Interrupt please.
                Spell.Cast("Pummel",
                    ret =>
                    StyxWoW.Me.CurrentTarget.IsCasting &&
                    StyxWoW.Me.CurrentTarget.CanInterruptCurrentSpellCast),
                Spell.Cast("Impending Victory", ret => StyxWoW.Me.HealthPercent <= 90 && StyxWoW.Me.HasAura("Victorious")),
                // Kee SS up if we've got more than 2 mobs to get to killing.
                new Decorator(ret => Unit.UnfriendlyMeleeUnits.Count() > 2,
                    CreateAoe()),
                new Decorator(ret => StyxWoW.Me.CurrentTarget.HealthPercent <= 20,
                    CreateExecuteRange()),
                new Decorator(ret => StyxWoW.Me.CurrentTarget.HealthPercent > 20,
                    new PrioritySelector(
                        Item.UsePotionAndHealthstone(40),
                        Spell.Cast("Blood Fury", ret => StyxWoW.Me.CurrentTarget.IsBoss),
                        // Stack our crit CDs for the most efficiency.
                         Spell.Cast("Recklessness",
                            ret => StyxWoW.Me.CurrentTarget.IsBoss && StyxWoW.Me.HasAura("Skull Banner")),
                        Spell.Cast("Avatar", ret => StyxWoW.Me.CurrentTarget.IsBoss && StyxWoW.Me.HasAura("Skull Banner")),
                        Spell.Cast("Skull Banner", ret => StyxWoW.Me.CurrentTarget.IsBoss),
                        new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),
                        // Only drop DC if we need to use HS for TFB. This lets us avoid breaking HS as a rage dump, when we don't want it to be one.
                        Spell.Cast("Heroic Strike", ret => Me.CurrentRage >= 80),
                        Spell.Cast("Bloodbath", ret => StyxWoW.Me.CurrentTarget.IsBoss),
                        Spell.Cast("Berserker Rage", ret => !StyxWoW.Me.HasAura("Enraged")),
                        Spell.Cast("Bloodthirst"),
                        Spell.Cast("Colossus Smash"),
                        Spell.Cast("Raging Blow"),
                        Spell.Cast("Wild Strike", ret => StyxWoW.Me.HasAura("Bloodsurge", 1)),                        
                        Spell.Cast("Dragon Roar"),
                        //Cast("Storm Bolt"),
                        Spell.Cast("Battle Shout", ret => StyxWoW.Me.RagePercent < 30),
                        Spell.Cast("Heroic Throw"),
                        // Don't use this in execute range, unless we need the heal. Thanks!
                        Spell.Cast("Impending Victory",
                            ret => StyxWoW.Me.CurrentTarget.HealthPercent > 20 || StyxWoW.Me.HealthPercent < 50))
                    )
                );
        }

        protected override Composite CreateBuffs()
        {
            return Spell.Cast("Battle Shout", ret => !StyxWoW.Me.HasAura("Battle Shout"));
        }

        private Composite CreateAoe()
        {
            return new PrioritySelector(
                Spell.Cast("Dragon Roar"),
                //Cast("Shockwave"),
                //Cast("Bladestorm", ret => UnfriendlyMeleeUnits.Count() >= 4),
                // Basically, we want to pop RB when we have 1 less stacks then we do mobs around us.
                // eg; if we have 3 mobs, we want 2 stacks of cleaver. This just ensures we have a minimum of 1, and a max of 3. (You can't have more than 3 stacks!)
                Spell.Cast("Raging Blow",
                    ret =>
                    StyxWoW.Me.HasAura("Meat Cleaver", (int)MathEx.Clamp(1, 3, Unit.UnfriendlyMeleeUnits.Count() - 1))),
                Spell.Cast("Whirlwind")
                );
        }

        private Composite CreateExecuteRange()
        {
            return new PrioritySelector(
                // Pop all our CDs. Get ready to truck the mob.
                Spell.Cast("Recklessness", ret => StyxWoW.Me.CurrentTarget.IsBoss),
                Spell.Cast("Skull Banner", ret => StyxWoW.Me.CurrentTarget.IsBoss),
                Spell.Cast("Avatar", ret => StyxWoW.Me.CurrentTarget.IsBoss),
                Spell.Cast("Bloodbath", ret => StyxWoW.Me.CurrentTarget.IsBoss),
                Spell.Cast("Berserker Rage", ret => !StyxWoW.Me.HasAura("Enraged")),
                new Action(ret =>
                {
                    Item.UseTrinkets();
                    return RunStatus.Failure;
                }),
                Spell.Cast("Colossus Smash"),
                Spell.Cast("Dragon Roar"),
                Spell.Cast("Execute"),
                Spell.Cast("Bloodthirst"),
                Spell.Cast("Storm Bolt"),
                Spell.Cast("Battle Shout"),
                // Don't leave our execute range!
                new ActionAlwaysSucceed()
                );
        }
    }
}
