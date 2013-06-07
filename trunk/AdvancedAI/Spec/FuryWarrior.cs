using CommonBehaviors.Actions;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.Helpers;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

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
        LocalPlayer Me { get { return StyxWoW.Me; } }

        protected override Composite CreateCombat()
        {
            return new PrioritySelector(
                // Don't do anything if we have no target, nothing in melee range, or we're casting. (Includes vortex!)
                new Decorator(
                    ret =>
                    !StyxWoW.Me.GotTarget || StyxWoW.Me.IsCasting ||
                    StyxWoW.Me.CurrentPendingCursorSpell != null,
                    new ActionAlwaysSucceed()),
                // Interrupt please.
                Cast("Pummel",
                    ret =>
                    StyxWoW.Me.CurrentTarget.IsCasting &&
                    StyxWoW.Me.CurrentTarget.CanInterruptCurrentSpellCast),
                Cast("Impending Victory", ret => StyxWoW.Me.HealthPercent <= 90 && StyxWoW.Me.HasAura("Victorious")),
                // Kee SS up if we've got more than 2 mobs to get to killing.
                new Decorator(ret => UnfriendlyMeleeUnits.Count() > 2,
                    CreateAoe()),
                new Decorator(ret => StyxWoW.Me.CurrentTarget.HealthPercent <= 20,
                    CreateExecuteRange()),
                new Decorator(ret => StyxWoW.Me.CurrentTarget.HealthPercent > 20,
                    new PrioritySelector(
                        CreateUsePotionAndHealthstone(40),
                        Cast("Blood Fury", ret => StyxWoW.Me.CurrentTarget.IsBoss),
                // Stack our crit CDs for the most efficiency.
                        Cast("Recklessness",
                            ret => StyxWoW.Me.CurrentTarget.IsBoss && StyxWoW.Me.HasAura("Skull Banner")),
                        Cast("Avatar", ret => StyxWoW.Me.CurrentTarget.IsBoss && StyxWoW.Me.HasAura("Skull Banner")),
                        Cast("Skull Banner", ret => StyxWoW.Me.CurrentTarget.IsBoss),
                        new Action(ret => { UseHands(); return RunStatus.Failure; }),
                // Only drop DC if we need to use HS for TFB. This lets us avoid breaking HS as a rage dump, when we don't want it to be one.
                        Cast("Heroic Strike", ret => NeedHeroicStrikeDump),
                        Cast("Bloodbath", ret => StyxWoW.Me.CurrentTarget.IsBoss),
                        Cast("Berserker Rage", ret => !StyxWoW.Me.HasAura("Enraged")),
                        Cast("Bloodthirst"),
                        Cast("Colossus Smash"),
                        Cast("Raging Blow"),
                        Cast("Wild Strike", ret => StyxWoW.Me.HasAura("Bloodsurge", 1)),
                        HeroicLeap(),
                        Cast("Dragon Roar"),
                //Cast("Storm Bolt"),
                        Cast("Battle Shout", ret => StyxWoW.Me.RagePercent < 30),
                        Cast("Heroic Throw"),
                // Don't use this in execute range, unless we need the heal. Thanks!
                        Cast("Impending Victory",
                            ret => StyxWoW.Me.CurrentTarget.HealthPercent > 20 || StyxWoW.Me.HealthPercent < 50))
                    )
                );
        }

        protected override Composite CreateBuffs()
        {
            return Cast("Battle Shout", ret => !StyxWoW.Me.HasAura("Battle Shout"));
        }

        private Composite CreateAoe()
        {
            return new PrioritySelector(
                Cast("Dragon Roar"),
                //Cast("Shockwave"),
                //Cast("Bladestorm", ret => UnfriendlyMeleeUnits.Count() >= 4),
                // Basically, we want to pop RB when we have 1 less stacks then we do mobs around us.
                // eg; if we have 3 mobs, we want 2 stacks of cleaver. This just ensures we have a minimum of 1, and a max of 3. (You can't have more than 3 stacks!)
                Cast("Raging Blow",
                    ret =>
                    StyxWoW.Me.HasAura("Meat Cleaver", (int)MathEx.Clamp(1, 3, UnfriendlyMeleeUnits.Count() - 1))),
                Cast("Whirlwind")
                );
        }

        private Composite CreateExecuteRange()
        {
            return new PrioritySelector(
                // Pop all our CDs. Get ready to truck the mob.
                Cast("Recklessness", ret => StyxWoW.Me.CurrentTarget.IsBoss),
                Cast("Skull Banner", ret => StyxWoW.Me.CurrentTarget.IsBoss),
                Cast("Avatar", ret => StyxWoW.Me.CurrentTarget.IsBoss),
                Cast("Bloodbath", ret => StyxWoW.Me.CurrentTarget.IsBoss),
                Cast("Berserker Rage", ret => !StyxWoW.Me.HasAura("Enraged")),
                new Action(ret =>
                {
                    UseTrinkets();
                    return RunStatus.Failure;
                }),
                Cast("Colossus Smash"),
                Cast("Dragon Roar"),
                Cast("Execute"),
                Cast("Bloodthirst"),
                Cast("Storm Bolt"),
                Cast("Battle Shout"),
                // Don't leave our execute range!
                new ActionAlwaysSucceed()
                );
        }
    }
}
