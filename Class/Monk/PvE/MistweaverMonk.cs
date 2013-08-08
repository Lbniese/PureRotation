using CommonBehaviors.Actions;
using Styx;
using Styx.CommonBot;
using Styx.TreeSharp;
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
        static WoWUnit healtarget { get { return HealerManager.FindLowestHealthTarget(); } }
        static WoWPlayer RenewingMistTarget { get { return HealerManager.GetUnbuffedTarget("Renewing Mist"); } }
        public static Composite CreateMMCombat
        {
            get
            {
                HealerManager.NeedHealTargeting = true;
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        MistweaverMonkPvP.CreateMMPvPCombat),
                    Common.CreateInterruptBehavior(),
                    Dispelling.CreateDispelBehavior(),
                    Spell.Cast("Fortifying Brew", ret => Me.HealthPercent < 30),
                    Spell.Cast("Life Cocoon", on => CocoonTar),
                    Spell.Cast("Revival", ret => HealerManager.GetCountWithHealth(55) > 4 && AdvancedAI.Burst),
                    //Spell.CastOnGround("Healing Sphere", on => healtarget.Location, ret => healtarget.HealthPercent < 55 && Me.ManaPercent > 40, false),
                    new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),
                        new Decorator(ret => Me.ManaPercent < 87,
                            new PrioritySelector(
                    new Action(ret => { Item.UseTrinkets(); return RunStatus.Failure; }),
                    Spell.Cast("Mana Tea"))),
                    // Execute if we can
                    Spell.Cast("Touch of Death", ret => Me.CurrentChi >= 3 && Me.HasAura("Death Note")),
                    new Throttle(1, 1,
                        new PrioritySelector(
                            Spell.Cast("Thunder Focus Tea", ret => HealerManager.GetCountWithBuff("Renewing Mist") >= 6))),
                    Spell.Cast("Uplift", ret => Me.HasAura("Thunder Focus Tea")),
                    //OH Crap stuff
                    // need to see if i want it drop on only tanks or everyone TFT is to big to waste
                    //new Decorator(ret => healtarget.HealthPercent < 20,
                    //    new Sequence(
                    //        Spell.Cast("Thunder Focus Tea"),
                    //        Spell.Cast("Soothing Mist", on => healtarget),
                    //        Spell.Cast("Surging Mist", on => healtarget))),

                    //new Decorator(ret => healtarget.HealthPercent < 70,
                    //    new Sequence(
                    //        Spell.Cast("Soothing Mist", on => healtarget),
                    //        Spell.Cast("Enveloping Mist", on => healtarget))),
                    //doing some testing here to see which way is better
                    Spell.Cast("Enveloping Mist", ret => healtarget.HealthPercent < 75 && Me.IsChanneling && ChannelCheck()),
                    //new Decorator(ret => healtarget.HealthPercent < 41,
                    //    new Sequence(
                    //        Spell.Cast("Soothing Mist", on => healtarget),
                    //        Spell.Cast("Surging Mist", on => healtarget))),
                    Spell.Cast("Surging Mist", ret => healtarget.HealthPercent < 41 && Me.IsChanneling && ChannelCheck()),

                    Spell.Cast("Renewing Mist", on => RenewingMistTarget, ret => Me.CurrentChi < Me.MaxChi),
                    new Throttle(1, 1,
                        new PrioritySelector(
                            Spell.Cast("Uplift", ret => HealerManager.GetCountWithBuffAndHealth("Renewing Mist", 90) > 2 || Me.CurrentChi >= 4/*(TalentManager.IsSelected((int)MonkTalents.Ascension) ? 4 : 3)*/))),//Me.GroupInfo.RaidMembers.Count(u => u.ToPlayer().HasAura("Renewing Mist") && u.ToPlayer().HealthPercent < 90) > 2 || Me.CurrentChi >= 4))),
                    Spell.Cast("Surging Mist", on => healtarget, ret => healtarget.HealthPercent < 85 && Me.HasAura("Vital Mists", 5)),

                    //needs more work to dial in SCK it cost alot of mana
                    //Spell.Cast("Spinning Crane Kick", ret => Me.IsMoving && Me.GroupInfo.RaidMembers.Count(u => u.ToPlayer().HealthPercent < 85) >= 5),

                    //LvL 30 Talents
                    Spell.Cast("Chi Wave", on => healtarget, ret => healtarget.HealthPercent < 90),
                    Spell.Cast("Chi Burst", on => healtarget, ret => Clusters.GetClusterCount(healtarget, Unit.NearbyFriendlyPlayers, ClusterType.Path, 5) >= 3 && healtarget.HealthPercent < 80),
                    new Throttle(1, 3,
                        new PrioritySelector(
                            Spell.Cast("Zen Sphere", on => healtarget, ret => HealerManager.GetCountWithBuff("Zen Sphere") < 2 && healtarget.HealthPercent < 90))),
                    Spell.Cast("Expel Harm", ret => Me.HealthPercent < 90),

                    //FW                                 
                    new Decorator(ret => AdvancedAI.FistWeave,
                        new PrioritySelector(
                            Spell.Cast("Blackout Kick", ret => !Me.HasAura("Serpent's Zeal") && Me.HasAura("Muscle Memory")),
                            Spell.Cast("Tiger Palm", ret => Me.HasAura("Muscle Memory") || (Me.CurrentChi > 3 && TalentManager.IsSelected((int)MonkTalents.Ascension)) || Me.CurrentChi > 4),
                            Spell.Cast("Jab", ret => !Me.HasAura("Muscle Memory") && Me.CurrentChi < Me.MaxChi))),
                    //Spam
                    new Decorator(ret => Me.CastingSpell.Name == "Chi Burst",
                        new ActionAlwaysSucceed()),
                    new Decorator(ret => !ChannelCheck() && healtarget.HealthPercent < 95,
                        new Sequence(
                            new Action(ret => SpellManager.StopCasting()),
                            Spell.Cast("Soothing Mist", on => healtarget, ret => !AdvancedAI.FistWeave && Me.CurrentChi < Me.MaxChi/*&& (Me.CurrentTarget.IsValid && Me.CurrentTarget != null && !Me.CurrentTarget.IsWithinMeleeRange)*/))),
                    //Spell.Cast("Soothing Mist", on => healtarget, ret => healtarget.HealthPercent < 95 && !AdvancedAI.FistWeave, cancel => !ChannelCheck()),
                    //Spell.Cast("Soothing Mist", on => healtarget, ret => healtarget.HealthPercent < 95 && (Me.CurrentTarget.IsValid && Me.CurrentTarget != null && !Me.CurrentTarget.IsWithinMeleeRange))
                    new ActionAlwaysSucceed());
            }
        }

        public static Composite CreateMMBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        MistweaverMonkPvP.CreateMMPvPBuffs),
                    Spell.Cast("Stance of the Wise Serpent", ret => !Me.HasAura("Stance of the Wise Serpent")));
            }
        }

        #region cocoon target
        public static WoWUnit CocoonTar
        {
            get
            {
                var tanks = Group.Tanks.OrderByDescending(u => u.HealthPercent).LastOrDefault();
                if (tanks != null && tanks.IsAlive && tanks.IsValid && tanks.HealthPercent < 35 && tanks.Distance < 40)
                    return tanks;
                return null;
            }
        }
        #endregion

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

        #region Channel Check
        static  bool ChannelCheck()
        {
            return healtarget.Guid == Me.ChannelObjectGuid;
        }
        #endregion


    }
}
