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
using System.Threading.Tasks;

namespace AdvancedAI.Spec
{
    class ArmsWarrior
    {
        /// <summary>
        /// The name of this CombatRoutine
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name { get { return "Armed2"; } }
        private static LocalPlayer Me { get { return StyxWoW.Me; } }

        /// <summary>
        /// The <see cref="T:Styx.WoWClass"/> to be used with this routine
        /// </summary>
        /// <value>
        /// The class.
        /// </value>
        public override WoWClass Class { get { return WoWClass.Warrior; } }


        private Composite _combat, _buffs;


        public override Composite CombatBehavior { get { return _combat; } }
        public override Composite PreCombatBuffBehavior { get { return _buffs; } }
        public override Composite CombatBuffBehavior { get { return _buffs; } }


        public override void Initialize()
        {
            _combat = CreateCombat();
            _buffs = CreateBuffs();
        }


        Composite CreateBuffs()
        {
            return Cast("Battle Shout", ret => !Me.HasAura("Battle Shout"));
        }


        Composite CreateCombat()
        {
            return new PrioritySelector(


                // Interrupt please.
                Spell.Cast("Pummel", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast),
                Spell.Cast("Impending Victory", ret => Me.HealthPercent <= 90 && Me.HasAura("Victorious")),

                //Staying Alive
                //Spell.Cast("Rallying Cry", ret => Me.HealthPercent <= 30),
                Spell.Cast("Die by the Sword", ret => Me.HealthPercent <= 20),

                // Kee SS up if we've got more than 2 mobs to get to killing.
                new Decorator(ret => Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8 * 8) >= 4,
                    CreateAoe()),


                new Decorator(
                    new PrioritySelector(

                        Spell.Cast("Recklessness", ret => Me.CurrentTarget.IsBoss && Me.CurrentTarget.HasAuraExpired("Colossus Smash", 5)),

                        Spell.Cast("Bloodbath", ret => Me.CurrentTarget.IsBoss && Me.HasAura("Recklessness") || Me.CurrentTarget.IsBoss && GetSpellCooldown("Recklessness").TotalSeconds > 3),

                        Spell.Cast("Avatar", ret => Me.CurrentTarget.IsBoss && Me.HasAura("Recklessness")),

                        Spell.Cast("Skull Banner", ret => Me.CurrentTarget.IsBoss && Me.HasAura("Recklessness")),

                new Decorator(ret => Me.HasAura("Bloodbath"),
                    new PrioritySelector(
                        new Action(ret => { UseHands(); return RunStatus.Failure; }),
                        new Action(ret => { UseTrinkets(); return RunStatus.Failure; }))),

                        Spell.Cast("Berserker Rage", ret => !Me.ActiveAuras.ContainsKey("Enrage")),

                        Spell.Cast("Sweeping Strikes", ret => Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 5 * 5) >= 2),

                        //caueses some probs dont really like it
                //HeroicLeap(),

                        Spell.Cast("Heroic Strike", ret => (Me.CurrentTarget.HasMyAura("Colossus Smash") && Me.CurrentRage >= 70) || Me.CurrentRage >= 95),

                        Spell.Cast("Mortal Strike"),

                        Spell.Cast("Dragon Roar", ret => !Me.CurrentTarget.HasMyAura("Colossus Smash") && Me.HasAura("Bloodbath") && Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8 * 8) >= 1),

                        Spell.Cast("Colossus Smash", ret => Me.HasAuraExpired("Colossus Smash", 1)),

                        Spell.Cast("Execute", ret => Me.CurrentTarget.HasMyAura("Colossus Smash") || Me.HasAura("Recklessness") || Me.CurrentRage >= 85),

                        Spell.Cast("Dragon Roar", ret => (!Me.CurrentTarget.HasMyAura("Colossus Smash") && Me.CurrentTarget.HealthPercent < 20) || (Me.HasAura("Bloodbath") && Me.CurrentTarget.HealthPercent >= 20) && Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8 * 8) >= 1),

                        Spell.Cast("Thunder Clap", ret => Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8 * 8) >= 2 && Clusters.GetCluster(Me, Unit.NearbyUnfriendlyUnits, ClusterType.Radius, 8).Any(u => !u.HasMyAura("Deep Wounds"))),

                        Spell.Cast("Slam", ret => Me.CurrentTarget.HasMyAura("Colossus Smash") && (Me.CurrentTarget.GetAuraTimeLeft("Colossus Smash").TotalSeconds <= 1 || Me.HasAura("Recklessness")) && Me.CurrentTarget.HealthPercent >= 20),

                        Spell.Cast("Overpower", ret => Me.HasAura("Taste for Blood") && Me.Auras["Taste for Blood"].StackCount >= 3 && Me.CurrentTarget.HealthPercent >= 20),

                        Spell.Cast("Slam", ret => Me.CurrentTarget.HasAura("Colossus Smash") && Me.CurrentTarget.GetAuraTimeLeft("Colossus Smash").TotalSeconds <= 2.5 && Me.CurrentTarget.HealthPercent >= 20),

                        Spell.Cast("Execute", ret => !Me.HasAura("Sudden Execute")),

                        Spell.Cast("Overpower", ret => Me.CurrentTarget.HealthPercent >= 20 || Me.HasAura("Sudden Execute")),

                        Spell.Cast("Slam", ret => Me.CurrentRage >= 40 && Me.CurrentTarget.HealthPercent >= 20),

                        Spell.Cast("Battle Shout"),

                        Spell.Cast("Heroic Throw"),

                       // Don't use this in execute range, unless we need the heal. Thanks!
                        Cast("Impending Victory", ret => Me.CurrentTarget.HealthPercent > 20 || Me.HealthPercent < 50))
                    )
                );
        }
        Composite CreateAoe()
        {
            return new PrioritySelector(

                Spell.Cast("Thunder Clap", ret => Clusters.GetCluster(Me, Unit.NearbyUnfriendlyUnits, ClusterType.Radius, 8).Any(u => !u.HasMyAura("Deep Wounds"))),
                Spell.Cast("Dragon Roar", ret => !Me.CurrentTarget.HasMyAura("Colossus Smash") && Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8 * 8) >= 1),
                Spell.Cast("Wirlwind"),
                Spell.Cast("Bladestorm", ret => Unit.NearbyUnfriendlyUnits.Count(u => u.DistanceSqr <= 8 * 8) >= 4),
                Spell.Cast("Mortal Strike"),
                Spell.Cast("Colossus Smash", ret => !StyxWoW.Me.CurrentTarget.HasMyAura("Colossus Smash")),
                Spell.Cast("Overpower")


                );
        }
        Composite CreateExecuteRange()
        {
            return new PrioritySelector(

                );
        }


        Composite HeroicLeap()
        {
            return new Decorator(ret => StyxWoW.Me.CurrentTarget.HasAura("Colossus Smash") && SpellManager.CanCast("Heroic Leap"),
                new Action(ret =>
                {
                    var tpos = StyxWoW.Me.CurrentTarget.Location;
                    var trot = StyxWoW.Me.CurrentTarget.Rotation;
                    var leapRight = WoWMathHelper.CalculatePointAtSide(tpos, trot, 5, true);
                    var leapLeft = WoWMathHelper.CalculatePointAtSide(tpos, trot, 5, true);


                    var myPos = StyxWoW.Me.Location;


                    var leftDist = leapLeft.Distance(myPos);
                    var rightDist = leapRight.Distance(myPos);


                    var leapPos = WoWMathHelper.CalculatePointBehind(tpos, trot, 8);


                    if (leftDist > rightDist && leftDist <= 40 && leftDist >= 8)
                        leapPos = leapLeft;
                    else if (rightDist > leftDist && rightDist <= 40 && rightDist >= 8)
                        leapPos = leapLeft;


                    SpellManager.Cast("Heroic Leap");
                    SpellManager.ClickRemoteLocation(leapPos);
                    StyxWoW.Me.CurrentTarget.Face();
                }));
        }

        void UseHands()
        {
            var hands = StyxWoW.Me.Inventory.Equipped.Hands;

            if (hands != null && CanUseEquippedItem(hands))
                hands.Use();

        }


        void UseTrinkets()
        {
            var firstTrinket = StyxWoW.Me.Inventory.Equipped.Trinket1;
            var secondTrinket = StyxWoW.Me.Inventory.Equipped.Trinket2;


            if (firstTrinket != null && CanUseEquippedItem(firstTrinket))
                firstTrinket.Use();


            if (secondTrinket != null && CanUseEquippedItem(secondTrinket))
                secondTrinket.Use();


        }
        private static bool CanUseEquippedItem(WoWItem item)
        {
            // Check for engineering tinkers!
            string itemSpell = Lua.GetReturnVal<string>("return GetItemSpell(" + item.Entry + ")", 0);
            if (string.IsNullOrEmpty(itemSpell))
                return false;


            return item.Usable && item.Cooldown <= 0;
        }


        IEnumerable<WoWUnit> UnfriendlyUnits
        {
            get { return ObjectManager.GetObjectsOfType<WoWUnit>(true, false).Where(u => !u.IsDead && u.CanSelect && u.Attackable && !u.IsFriendly && u.IsWithinMeleeRange); }
        }


        private delegate T Selection<out T>(object context);
        Composite Cast(string spell, Selection<bool> reqs = null)
        {
            return
                new Decorator(
                    ret => ((reqs != null && reqs(ret)) || (reqs == null)) && SpellManager.CanCast(spell),
                    new Action(ret => SpellManager.Cast(spell)));
        }


        public static TimeSpan GetSpellCooldown(string spell)
        {
            SpellFindResults results;
            if (SpellManager.FindSpell(spell, out results))
            {
                if (results.Override != null)
                    return results.Override.CooldownTimeLeft;
                return results.Original.CooldownTimeLeft;
            }


            return TimeSpan.MaxValue;
        }

    }
}
