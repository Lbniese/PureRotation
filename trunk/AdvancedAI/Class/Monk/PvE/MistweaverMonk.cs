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
        //static WoWUnit healtarget { get { return HealerManager.Instance.FirstUnit; } } //SING
        static WoWUnit healtarget { get { return HealerManager.FindLowestHealthTarget(); } } //SING2
        //static WoWUnit healtarget { get { return HealManager.HealTarget; } } //PURE
        static WoWPlayer RenewingMistTarget { get { return HealerManager.GetUnbuffedTarget("Renewing Mist"); } } //SING
        //static WoWPlayer RenewingMistTarget { get { return HealManager.GetUnbuffedTarget(115151); } } // PURE
        public static Composite CreateMMCombat
        {
            get
            {
                HealerManager.NeedHealTargeting = true;
                return new PrioritySelector(
                    //CachedUnits.Pulse, 
                    //HealManager.PulseHealManager,
                    new Decorator(ret => AdvancedAI.PvPRot,
                        MistweaverMonkPvP.CreateMWPvPCombat),
                    //new Decorator(ret => Me.Combat || healtarget.Combat || healtarget.GetPredictedHealthPercent() <= 99,
                        //new PrioritySelector(
                            Common.CreateInterruptBehavior(),
                            Dispelling.CreateDispelBehavior(),
                            Spell.Cast("Fortifying Brew", ret => Me.HealthPercent < 30),
                            Spell.Cast("Life Cocoon", on => CocoonTar),
                            Spell.Cast("Revival", ret => HealerManager.GetCountWithHealth(55) > 4 && AdvancedAI.Burst),
                            //Spell.CastOnGround("Healing Sphere", on => healtarget.Location, ret => healtarget.HealthPercent < 55 && Me.ManaPercent > 40, false),
                            new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),
                            new Action(ret => { Item.UseTrinkets(); return RunStatus.Failure; }),
                            Spell.Cast("Mana Tea", ret => Me.ManaPercent < 87),
                            // Execute if we can
                            Spell.Cast("Touch of Death", ret => Me.CurrentChi >= 3 && Me.HasAura("Death Note")),
                            new Throttle(1, 1,
                                new Sequence(
                                    Spell.Cast("Thunder Focus Tea", ret => HealerManager.GetCountWithBuffAndHealth("Renewing Mist", 80) >= 3),//Me.GroupInfo.RaidMembers.Count(u => u.ToPlayer().HasAura("Renewing Mist") && u.ToPlayer().HealthPercent < 80) >= 3))),
                                    Spell.Cast("Uplift"))),
                            //OH Crap stuff
                            new Decorator(ret => healtarget.HealthPercent < 70,
                                new Sequence(
                                    Spell.Cast("Soothing Mist", on => healtarget),
                                    Spell.Cast("Enveloping Mist", on => healtarget))),
                            new Throttle(1, 1,
                                new PrioritySelector(
                                    Spell.Cast("Uplift", ret => HealerManager.GetCountWithBuffAndHealth("Renewing Mist", 90) > 2 || Me.CurrentChi >= 4))),//Me.GroupInfo.RaidMembers.Count(u => u.ToPlayer().HasAura("Renewing Mist") && u.ToPlayer().HealthPercent < 90) > 2 || Me.CurrentChi >= 4))),
                            new Decorator(ret => healtarget.HealthPercent < 41,
                                new Sequence(
                                    Spell.Cast("Soothing Mist", on => healtarget),
                                    Spell.Cast("Surging Mist", on => healtarget))),

                            Spell.Cast("Renewing Mist", on => RenewingMistTarget),
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
                            Spell.Cast("Blackout Kick", ret => !Me.HasAura("Serpent's Zeal") && Me.HasAura("Muscle Memory")),
                            Spell.Cast("Tiger Palm", ret => Me.HasAura("Muscle Memory") || (Me.CurrentChi > 3 && TalentManager.IsSelected((int)MonkTalents.Ascension)) || Me.CurrentChi > 4),
                            Spell.Cast("Jab", ret => !Me.HasAura("Muscle Memory")),
                            //Spam
                            Spell.Cast("Soothing Mist", on => healtarget, ret => healtarget.HealthPercent < 95 && !Me.CurrentTarget.IsWithinMeleeRange)                            
                            );
            }
        }

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
