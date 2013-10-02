using System.Windows.Forms;
using CommonBehaviors.Actions;
using Styx;
using Styx.TreeSharp;
using Styx.WoWInternals.WoWObjects;
using AdvancedAI.Helpers;
using System.Linq;

namespace AdvancedAI.Class.Druid.PvE
{
    class GuardianDruid
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }

        public static Composite GuardianCombat()
        {
            return new PrioritySelector(
                // Don't do anything if we have no target, nothing in melee range, or we're casting. (Includes vortex!)
                new Decorator(ret => !Me.GotTarget ||  Me.IsCasting,
                    new ActionAlwaysSucceed()),

                //Spell.Cast("Bear Form", ret => Me.Shapeshift != ShapeshiftForm.Bear),
                Spell.Cast("Skull Bash", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast),

                new Decorator(ret => AdvancedAI.Burst,
                    CreateCooldowns()),
                // So.. the guardian rotation boils down to... 4 abilities.
                // 5 when counting Swipe in AOE rotations

                // So, theorycrafted this myself. The returns on this are worth using it. Unfortunately, you'll
                // rarely have > 90 rage when tanking properly (aka; keeping up a high SD uptime)
                // This could be tweaked to less rage [50] to take advantage of the crazy absorb you get from it
                // But for now, we'll leave it at 90 and hit Maul manually when required. (A lot of bosses this tier auto-attack
                // for jack shit, so we'll leave it up to the user to deal with it)
                Spell.Cast("Maul", ret => Me.RagePercent > 90 && Me.CachedHasAura("Tooth and Claw")),
                // Mangle on CD
                Spell.Cast("Mangle"),
                // Thrash on CD to keep up the debuffs + mangle procs
                Spell.Cast("Thrash", ret => Unit.UnfriendlyUnits(8).Count() >= 1),
                // AOE comes in before lac/ff
                CreateAoe(),
                // Lac for threat + mangle procs
                Spell.Cast("Lacerate"),
                // Keep up the sunders + mangle procs (not great for threat, hence lowest priority)
                Spell.Cast("Faerie Fire"),

                Spell.Cast("Maul", ret => !IsCurrentTank())
                // Symbiosis effect :)
                //Cast("Consecration")
                );
        }

        private static Composite CreateCooldowns()
        {
            return new PrioritySelector(
                new Decorator(ret => IsCurrentTank(),
                    new PrioritySelector(
                        new Action(ret => { Item.UseTrinkets(); return RunStatus.Failure; }),
                        new Action(ret => { Item.UseHands(); return RunStatus.Failure; }))),

                Spell.Cast("Cenarion Ward", on => Me),
                // Enrage if we need 20 more rage for a FR or SD
                Spell.Cast("Enrage", ret => Me.RagePercent < 40),

                Spell.Cast("Healing Touch", ret => Me.HasAura(145162) && Me.HealthPercent <= 90 || Me.HasAura(145162) && Me.GetAuraTimeLeft(145162).TotalSeconds < 1.5),

                //Cast("Incarnation: Son of Ursoc", ret => !StyxWoW.Me.HasAura("Berserk")),

                // Symbiosis effect.
                //Cast("Bone Shield", ret => StyxWoW.Me, ret => !StyxWoW.Me.HasAura("Bone Shield")),
                //Spell.Cast("Incarnation", ret => KeyboardPolling.IsKeyDown(Keys.Q) && KeyboardPolling.IsKeyDown(Keys.LMenu)),

                // Could be tweaked to be lower. Probably 40.
                Spell.Cast("Barkskin", ret => Me.HealthPercent <= 60),

                // Could be tweaked to be lower, again. 50 seems reasonable. May add a toggle for this to deal with big spike damage manually.
                Spell.Cast("Survival Instincts", ret => Me.HealthPercent <= 50 && !Me.CachedHasAura("Might of Ursoc")),

                // We need health FAST. Get some.
                Spell.Cast("Might of Ursoc", ret => Me.HealthPercent <= 30 && !Me.CachedHasAura("Survival Instincts")),

                // Never higher than 70 here. It heals 30% of our health. Wasted heal if we use it above 70.
                Spell.Cast("Renewal", ret => Me.HealthPercent <= 50 || Me.CachedHasAura("Might of Ursoc")),

                // Emergency heals.
                Item.UsePotionAndHealthstone(40),

                // Only use FR when we have full rage. Its probably not worth blowing it when we don't need to.
                // Also, don't overwrite it if we're glyphed for it!
                Spell.Cast("Frenzied Regeneration",
                    ret =>
                    Me.HealthPercent <= 65 && Me.CurrentRage >= 60 &&
                    !Me.CachedHasAura("Frenzied Regeneration")),

                // Don't overwrite SDs. But keep it up as much as possible.
                Spell.Cast("Savage Defense", ret => !Me.CachedHasAura("Savage Defense"))
                );
        }

        private static Composite CreateAoe()
        {
            return new Decorator(ret => Unit.UnfriendlyUnits(8).Count() >= 2,
                new PrioritySelector(
                // Best whenn used on 3-5 mobs. Not 30+
                   // Spell.Cast("Berserk", ret => !Me.CachedHasAura("Incarnation: Son of Ursoc")),
                    Spell.Cast("Swipe")
                    ));
        }

        static bool IsCurrentTank()
        {
            return Me.CurrentTarget.CurrentTargetGuid == Me.Guid;
        }

        private static WoWUnit InterruptableUnit()
        {
            // Simple 'nuff. First one we're able to interrupt!
            // Skull Bash has a 13yd range (charge) so make sure we use it if we can.
            // TODO: Figure out if we want a toggle for which list to use. For now, just using melee range mobs to ensure we're not charging off
            // while tanking a boss, to do an interrupt on a mob somewhere.
            return Unit.UnfriendlyUnits(8).FirstOrDefault(
                    u => /*u.Distance <= 13 &&*/ u.IsCasting &&
                    u.CanInterruptCurrentSpellCast);
        }

        #region DruidTalents
        public enum DruidTalents
        {
            FelineSwiftness = 1,//Tier 1
            DisplacerBeast,
            WildCharge,
            NaturesSwiftness,//Tier 2
            Renewal,
            CenarionWard,
            FaerieSwarm,//Tier 3
            MassEntanglement,
            Typhoon,
            SouloftheForest,//Tier 4
            Incarnation,
            ForceofNature,
            DisorientingRoar,//Tier 5
            UrsolsVortex,
            MightyBash,
            HeartoftheWild,//Tier 6
            DreamofCenarius,
            NaturesVigil
        }
        #endregion
    }
}
