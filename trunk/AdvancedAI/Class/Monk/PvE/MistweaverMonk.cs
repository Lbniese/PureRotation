using Styx;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using AdvancedAI.Helpers;
using System.Linq;
using Action = Styx.TreeSharp.Action;
using AdvancedAI.Managers;

namespace AdvancedAI.Spec
{
    class MistweaverMonk
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        static WoWUnit healtarget { get { return Me.GroupInfo.IsInParty ? HealerManager.FindLowestHealthTarget() : Me; } }
        static WoWPlayer RenewingMistTarget { get { return HealerManager.GetUnbuffedTarget("Renewing Mist"); } }
        private static string[] _doNotHeal;
        public static Composite CreateMMCombat
        {
            get
            {
                HealerManager.NeedHealTargeting = true;
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        MistweaverMonkPvP.CreateMWPvPCombat),
                    new Decorator(ret => Me.Combat || healtarget.Combat || Me.GroupInfo.IsInRaid,
                        new PrioritySelector(
                            //Spell.Cast("Mana Tea", ret => Me.ManaPercent < 90),
                            //Spell.Cast("Invoke Xuen, the White Tiger", ret => Me.CurrentTarget.IsBoss),
                            //Spell.Cast("Touch of Death", ret => Me.HasAura("Death Note")),
                            //Spell.Cast("Renewing Mist", on => healtarget, ret => healtarget.HasAura("Renewing Mist")),
                            //Spell.Cast("Surging Mist", on =>healtarget, ret => healtarget.HealthPercent < 90 && Me.HasAura("Vital Mists", 5)),
                            //Spell.Cast("Chi Wave"),
                            //Spell.Cast("Thunder Focus Tea"),
                            //Spell.Cast("Blackout Kick", ret => !Me.HasAura("Serpent's Zeal") && Me.HasAura("Muscle Memory")),
                            //Spell.Cast("Tiger Palm", ret => Me.HasAura("Muscle Memory")),
                            //Spell.Cast("Uplift", ret => Me.GroupInfo.RaidMembers.Count(u => u.ToPlayer().HasAura("Renewing Mist") && u.ToPlayer().HealthPercent < 90) > 2),
                            //Spell.Cast("Expel Harm"),
                            //Spell.Cast("Jab"),
                            Common.CreateInterruptBehavior(),
                            Dispelling.CreateDispelBehavior(),
                            Spell.Cast("Fortifying Brew", ret => Me.HealthPercent < 30),
                            Spell.Cast("Life Cocoon", on => healtarget, ret => Group.Tanks.Any(u => u.Guid == healtarget.Guid && healtarget.HealthPercent < 35)),
                            Spell.Cast("Revival", on => Me, ret => Me.GroupInfo.RaidMembers.Count(u => u.ToPlayer().HealthPercent < 55) > 4),
                            //Spell.CastOnGround("Healing Sphere", on => healtarget.Location, ret => healtarget.HealthPercent < 55 && Me.ManaPercent > 40, false),
                            new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),
                            new Action(ret => { Item.UseTrinkets(); return RunStatus.Failure; }),
                            Spell.Cast("Mana Tea", ret => Me.ManaPercent < 85),
                            new Throttle(1, 1,
                                new PrioritySelector(
                                    Spell.Cast("Thunder Focus Tea", ret => Me.GroupInfo.RaidMembers.Count(u => u.ToPlayer().HasAura("Renewing Mist") && u.ToPlayer().HealthPercent < 80) >= 3))),
                            //new Decorator(ret => healtarget.HealthPercent < 58,
                            //    new Sequence(
                            //        Spell.Cast("Soothing Mist", on => healtarget),
                            //        Spell.Cast("Enveloping Mist", on => healtarget))),
                            Spell.Cast("Enveloping Mist", on => healtarget, ret => healtarget.HealthPercent < 58 && Me.IsChanneling),
                            new Throttle(1, 1,
                                new PrioritySelector(
                                    Spell.Cast("Uplift", ret => Me.GroupInfo.RaidMembers.Count(u => u.ToPlayer().HasAura("Renewing Mist") && u.ToPlayer().HealthPercent < 90) > 2))),
                            Spell.Cast("Expel Harm", ret => Me.HealthPercent < 90),
                            //new Decorator(ret => healtarget.HealthPercent < 41,
                            //    new Sequence(
                            //        Spell.Cast("Soothing Mist", on => healtarget),
                            //        Spell.Cast("Surging Mist", on => healtarget))),
                            Spell.Cast("Surging Mist", on => healtarget, ret => healtarget.HealthPercent < 41 && Me.IsChanneling),
                            Spell.Cast("Surging Mist", on => healtarget, ret => healtarget.HealthPercent < 85 && Me.HasAura("Vital Mists", 5)),
                            Spell.Cast("Soothing Mist", on => healtarget, ret => healtarget.HealthPercent < 95 && !Me.CurrentTarget.IsWithinMeleeRange),
                            Spell.Cast("Soothing Mist", on => healtarget, ret => healtarget.HealthPercent < 41),
                            Spell.Cast("Renewing Mist", on => RenewingMistTarget),
                            Spell.Cast("Spinning Crane Kick", ret => Me.IsMoving && Me.GroupInfo.RaidMembers.Count(u => u.ToPlayer().HealthPercent < 85) >= 5),
                            Spell.Cast("Chi Wave", on => healtarget, ret => healtarget.HealthPercent < 90),
                            Spell.Cast("Chi Burst", on => healtarget, ret => Clusters.GetClusterCount(healtarget, Unit.NearbyFriendlyPlayers, ClusterType.Path, 5) >= 3 && healtarget.HealthPercent < 80),
                            new Throttle(1, 3,
                                new PrioritySelector(
                                    Spell.Cast("Zen Sphere", on => healtarget, ret => Me.GroupInfo.RaidMembers.Count(u => u.ToPlayer().HasAura("Zen Sphere")) < 2 && healtarget.HealthPercent < 90))),
                            Spell.Cast("Blackout Kick", ret => !Me.HasAura("Serpent's Zeal") && Me.HasAura("Muscle Memory")),
                            Spell.Cast("Tiger Palm", ret => Me.HasAura("Muscle Memory") || (Me.CurrentChi > 3 && TalentManager.IsSelected((int)MonkTalents.Ascension)) || Me.CurrentChi > 4),
                            Spell.Cast("Jab"))));
            }
        }
        //detox
        //stance
        // fortbrew
        //life cocoon
        //revival
        //healing sphere
        //jade serrpent statue
        //trikets/hands
        //mana tea
        //thunder focus more than 3 with renewing mist && they are below 80%
        //enveloping mist
        //uplift
        //epel harm < 90 health
        //surging mist
        //soothing mist
        //renewing mist
        //sck (in if 3 targets around less than x and me.moving)
        //lvl 30 talent
        //fist weave BK TP JAB



        public static Composite CreateMMBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        MistweaverMonkPvP.CreateMWPvPBuffs),
                    Spell.Cast("Stance of the Wise Serpent", ret => !Me.HasAura("Stance of the Wise Serpent")));
            }
        }

        #region statue target
        public static WoWUnit StatueTar
        {
            get
            {
                var tanks = Group.Tanks.FirstOrDefault();
                if (tanks != null && (tanks.IsAlive && tanks.IsValid))
                    return tanks;
                return Me;
            }
        }
        #endregion

        #region MonkTalents
        public enum MonkTalents
        {
            Celerity = 1,//Tier 1
            TigersLust,
            Momentum,
            ChiWave,//Tier 2
            ZenSphere,
            ChiBurst,
            PowerStrikes,//Tier 3
            Ascension,
            ChiBrew,
            RingofPeace,//Tier 4
            ChargingOxWave,
            LegSweep,
            HealingElixirs,//Tier 5
            DampenHarm,
            DiffuseMagic,
            RushingJadeWind,//Tier 6
            InvokeXuen,
            ChiTorpedo
        }
        #endregion
    }
}
