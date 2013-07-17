using System.Drawing;
using Bots.BGBuddy.Helpers;
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
    class BeastmasterHunter
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        static WoWUnit Pet { get { return StyxWoW.Me.Pet; } }
        public static Composite CreateBMHCombat 
        { 
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        BeastmasterHunterPvP.CreateBMPvPCombat),

                        Spell.Cast("Silencing Shot", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast),

                        CreateHunterTrapBehavior("Explosive Trap", true, ret => Me.CurrentTarget, ret => Unit.UnfriendlyUnitsNearTarget(10f).Count() >= 2),
                        Spell.BuffSelf("Focus Fire", ctx => Me.HasAura("Frenzy", 5)),
                        Spell.Buff("Serpent Sting"),
                        Spell.Cast("Fervor", ctx => Me.CurrentFocus < 65),
                        Spell.Buff("Bestial Wrath", true, ret => Me.CurrentFocus > 60 && Spell.GetSpellCooldown("Kill Command") == TimeSpan.Zero && !Me.HasAura("Rapid Fire"), "The Beast Within"),

                        Spell.Cast("Tranquilizing Shot", ctx => Me.CurrentTarget.HasAura("Enraged")),

                        Spell.Buff("Concussive Shot",
                            ret => Me.CurrentTarget.CurrentTargetGuid == Me.Guid 
                                && Me.CurrentTarget.Distance > Spell.MeleeRange),

                        // AoE Rotation
                        new Decorator(ret => AdvancedAI.Aoe && Unit.UnfriendlyUnitsNearTarget(8f).Count() >= 5,
                            new PrioritySelector(
                                Spell.Cast( "Multi-Shot", ctx => Clusters.GetBestUnitForCluster( Unit.NearbyUnfriendlyUnits.Where( u => u.Distance < 40 && u.InLineOfSpellSight && Me.IsSafelyFacing(u)), ClusterType.Radius, 8f)),
                                Spell.Cast( "Kill Shot", onUnit => Unit.NearbyUnfriendlyUnits.FirstOrDefault(u => u.HealthPercent < 20 && u.Distance < 40 && u.InLineOfSpellSight && Me.IsSafelyFacing(u))),
                                Spell.Cast( "Cobra Shot"))),

                        Spell.Buff("Rapid Fire", ret => !Me.HasAura("The Beast Within") && !Me.CurrentTarget.IsBoss()),
                        Spell.Cast("Rabid", ret => Me.HasAura("The Beast Within")),
                        Spell.BuffSelf("Exhilaration", ret => Me.HealthPercent < 35 || (Pet != null && Pet.HealthPercent < 25)),
                        Spell.Buff("Mend Pet", onUnit => Pet, ret => Me.GotAlivePet && Pet.HealthPercent < 60),
                        Spell.Cast("Stampede", ret => (PartyBuff.WeHaveBloodlust || Me.CurrentTarget.TimeToDeath() <= 25 || Me.HasAura("Rapid Fire")) && !Me.CurrentTarget.IsBoss()),
                        Spell.Cast("Kill Shot", ctx => Me.CurrentTarget.HealthPercent < 20),
                        Spell.Cast("Kill Command", ctx => Me.GotAlivePet && Pet.GotTarget && Pet.Location.Distance(Pet.CurrentTarget.Location) < 25f),
                        Spell.Cast("A Murder of Crows"),
                        Spell.Cast("Glaive Toss"),
                        Spell.Cast("Lynx Rush", ret => Pet != null && Unit.NearbyUnfriendlyUnits.Any(u => Pet.Location.Distance(u.Location) <= 10)),
                        Spell.Cast("Dire Beast", ret => Me.CurrentFocus <= 90),
                        Spell.Cast("Barrage"),
                        Spell.Cast("Powershot"),
                        Spell.Cast("Blink Strike", on => Me.CurrentTarget, ret => Me.GotAlivePet && Me.Pet.SpellDistance(Me.CurrentTarget) < 40),
                        Spell.Cast("Arcane Shot", ret => Me.HasAura("Thrill of the Hunt")),  
                        Spell.BuffSelf("Focus Fire", ctx => Me.HasAura("Frenzy", 5) && !Me.HasAura("The Beast Within")),                        
                        Spell.Cast("Cobra Shot", ret => Me.CurrentTarget.GetAuraTimeLeft("Serpent Sting", true).TotalSeconds < 6),
                        Spell.Cast("Arcane Shot", ret => Me.CurrentFocus >= 61 || Me.HasAura("The Beast Within")),
                        Spell.Cast("Cobra Shot")

                    //8	1.00	virmens_bite_potion,if=buff.bloodlust.react|target.time_to_die<=60
                    //9	1.00	auto_shot
                    //A	0.00	explosive_trap,if=active_enemies>1
                    //B	10.19	focus_fire,five_stacks=1
                    //C	1.02	serpent_sting,if=!ticking
                    //D	4.31	blood_fury
                    //E	0.00	fervor,if=enabled&!ticking&focus<=65
                    //F	8.97	bestial_wrath,if=focus>60&!buff.beast_within.up
                    //G	0.00	multi_shot,if=active_enemies>5
                    //H	0.00	cobra_shot,if=active_enemies>5
                    //I	4.64	rapid_fire,if=!buff.rapid_fire.up
                    //J	2.00	stampede,if=buff.rapid_fire.up|buff.bloodlust.react|target.time_to_die<=25
                    //K	16.60	kill_shot
                    //L	68.42	kill_command
                    //M	5.25	a_murder_of_crows,if=enabled&!ticking
                    //N	29.65	glaive_toss,if=enabled
                    //O	0.00	lynx_rush,if=enabled&!dot.lynx_rush.ticking
                    //P	14.70	dire_beast,if=enabled&focus<=90
                    //Q	0.00	barrage,if=enabled
                    //R	0.00	powershot,if=enabled
                    //S	1.86	readiness,wait_for_rapid_fire=1
                    //T	0.00	arcane_shot,if=buff.thrill_of_the_hunt.react
                    //U	0.00	focus_fire,five_stacks=1,if=!ticking&!buff.beast_within.up
                    //V	5.33	cobra_shot,if=dot.serpent_sting.remains<6
                    //W	151.50	arcane_shot,if=focus>=61|buff.beast_within.up
                    //X	104.88	cobra_shot
                    );
            }
        }

        public static Composite CreateBMHBuffs
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => AdvancedAI.PvPRot,
                        BeastmasterHunterPvP.CreateBMPvPBuffs));
            }
        }

        #region Traps
        public static Composite CreateHunterTrapBehavior(string trapName, bool useLauncher, UnitSelectionDelegate onUnit, SimpleBooleanDelegate req = null)
        {
            return new PrioritySelector(
                new Decorator(
                    ret => onUnit != null && onUnit(ret) != null
                        && (req == null || req(ret))
                        && onUnit(ret).DistanceSqr < (40 * 40)
                        && SpellManager.HasSpell(trapName) && Spell.GetSpellCooldown(trapName) == TimeSpan.Zero,
                    new Sequence(
                        new Action(ret => Logger.WriteDebug("Trap: use trap launcher requested: {0}", useLauncher)),
                        new PrioritySelector(
                            new Decorator(ret => useLauncher && Me.HasAura("Trap Launcher"), new ActionAlwaysSucceed()),
                            Spell.BuffSelf("Trap Launcher", ret => useLauncher),
                            new Decorator(ret => !useLauncher, new Action(ret => Me.CancelAura("Trap Launcher")))
                            ),
                // new Wait(TimeSpan.FromMilliseconds(500), ret => !useLauncher && Me.HasAura("Trap Launcher"), new ActionAlwaysSucceed()),
                        new Wait(TimeSpan.FromMilliseconds(500),
                            ret => (!useLauncher && !Me.HasAura("Trap Launcher"))
                                || (useLauncher && Me.HasAura("Trap Launcher")),
                            new ActionAlwaysSucceed()),
                        new Action(ret => Logger.WriteDebug("Trap: launcher aura present = {0}", Me.HasAura("Trap Launcher"))),
                        new Action(ret => Logger.WriteDebug("Trap: cancast = {0}", SpellManager.CanCast(trapName, onUnit(ret)))),

                // Spell.Cast( trapName, ctx => onUnit(ctx)),
                        new Action(ret => SpellManager.Cast(trapName, onUnit(ret))),

                        Helpers.Common.CreateWaitForLagDuration(),
                        new Action(ctx => SpellManager.ClickRemoteLocation(onUnit(ctx).Location)),
                        new Action(ret => Logger.WriteDebug("Trap: Complete!"))
                        )
                    )
                );
        }
        
        #endregion

        #region HunterTalents
        public enum HunterTalents
        {
            Posthaste = 1,//Tier 1
            NarrowEscape,
            CrouchingTiger,
            SilencingShot,//Tier 2
            WyvernSting,
            Intimidation,
            Exhilaration,//Tier 3
            AspectoftheIronHawk,
            SpiritBond,
            Fervor,//Tier 4
            DireBeast,
            ThrilloftheHunt,
            AMurderofCrows,//Tier 5
            BlinkStrikes,
            LynxRush,
            GlaiveToss,//Tier 6
            Powershot,
            Barrage
        }
        #endregion
    }
}
