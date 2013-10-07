using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedAI.Helpers;
using AdvancedAI.Managers;
using CommonBehaviors.Actions;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;

namespace AdvancedAI.Class.Warlock.PvE
{
    static class AfflictionWarlock
    {
        private static LocalPlayer Me { get { return StyxWoW.Me; } }
        private static int _mobCount;
        public static Composite AfflictionPull()
        {
            return new PrioritySelector(
                
                new Decorator(
                    ret => !Spell.IsGlobalCooldown(),
                    new PrioritySelector(
                        ApplyDots(on => Me.CurrentTarget, burn => true)
                        )
                    )
                );
        }

        public static Composite AfflictionCombat()
        {
            return new PrioritySelector(

                new Action(r => { if (Me.GotTarget) Me.CurrentTarget.TimeToDeath(); return RunStatus.Failure; }),

                // cancel an early drain soul if done to proc 1 soulshard
                new Decorator(
                    ret => Me.GotTarget && Me.ChanneledSpell != null,
                    new PrioritySelector(
                        new Decorator(
                            ret => Me.ChanneledSpell.Name == "Drain Soul"
                                && Me.CurrentTarget.HealthPercent > 20 || Reupdots,
                            new Sequence(
                                new Action(ret => Logging.WriteDiagnostic("/cancel Drain Soul on {0} now we have {1} shard", Me.CurrentTarget.SafeName(), Me.CurrentSoulShards)),
                                new Action(ret => SpellManager.StopCasting()),
                                new WaitContinue(TimeSpan.FromMilliseconds(500), ret => Me.ChanneledSpell == null, new ActionAlwaysSucceed())
                                )
                            ),

                        // cancel malefic grasp if target health < 20% and cast drain soul (revisit and add check for minimum # of dots)
                        new Decorator(
                            ret => Me.ChanneledSpell.Name == "Malefic Grasp"
                                && Me.CurrentTarget.HealthPercent <= 20 || Reupdots,
                            new Sequence(
                                new Action(ret => Logging.WriteDiagnostic("/cancel Malefic Grasp on {0} @ {1:F1}%", Me.CurrentTarget.SafeName(), Me.CurrentTarget.HealthPercent)),
                                new Action(ret => SpellManager.StopCasting()),
                                new WaitContinue(TimeSpan.FromMilliseconds(500), ret => Me.ChanneledSpell == null, new ActionAlwaysSucceed())
                                )
                            )
                        )
                    ),
                
                AfflictionCombatBuffs(),

                new Decorator(
                    new PrioritySelector(
                        Common.CreateInterruptBehavior(),

                        new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),
                        new Action(ret => { Item.UseTrinkets(); return RunStatus.Failure; }),

                        //Aoe(),
                        ApplyDots(on => Me.CurrentTarget, ret => !Me.CurrentTarget.HasAnyOfMyAuras("Agony", "Corruption", "Unstable Affliction", "Haunt") || Reupdots),
                        new Decorator(ret => Unit.UnfriendlyUnitsNearTarget(10).Count() >= 4 && AdvancedAI.Aoe,
                            new PrioritySelector(
                                CastSoulburn(req => !Me.CurrentTarget.HasMyAura("Seed of Corruption")),
                                Spell.Cast("Seed of Corruption", ret => !Me.CurrentTarget.HasMyAura("Seed of Corruption")))),
                        Spell.WaitForCastOrChannel(),
                        new Throttle(1,
                            new PrioritySelector(
                                //new Decorator(ret=> Me.IsMoving,
                                //    new PrioritySelector(
                                //        ApplyDots(on => Me.CurrentTarget, ret => !Me.CurrentTarget.HasAnyOfMyAuras("Agony", "Corruption", "Unstable Affliction", "Haunt") || Reupdots),
                                //        Spell.Cast("Malefic Grasp", ret => SpellManager.HasSpell(137587)),
                                //        Spell.Cast("Fel Flame", ret => !SpellManager.HasSpell(137587)))),
                                Spell.Cast("Malefic Grasp", ret => Me.CurrentTarget.HealthPercent > 20 && !Reupdots),
                                Spell.Cast("Drain Soul", ret => Me.CurrentTarget.HealthPercent <= 20 && !Reupdots),
                                Spell.Cast("Fel Flame", ret => !SpellManager.HasSpell(137587) && Me.IsMoving)))
                        )
                    )
                );

        }

        private static bool HaveHealthStone { get { return StyxWoW.Me.BagItems.Any(i => i.Entry == 5512); } }
        public static Composite AfflictionPreCombatBuffs()
        {
            return new PrioritySelector(
                Spell.WaitForCastOrChannel(),
                new Decorator(
                    ret => !Spell.IsGlobalCooldown() && !Me.Mounted,
                    new PrioritySelector(
                        //SummonPet(),
                        new Throttle(5, Spell.Cast("Create Healthstone", mov => true, on => Me, ret => !HaveHealthStone && !Unit.NearbyUnfriendlyUnits.Any(u => u.Distance < 25), cancel => false)),
                        PartyBuff.BuffGroup("Dark Intent"),
                        Spell.BuffSelf("Grimoire of Sacrifice", ret => GetCurrentPet() != WarlockPet.None && GetCurrentPet() != WarlockPet.Other),
                        Spell.BuffSelf("Unending Breath", req => Me.IsSwimming))));
        }

        #region AfflictionCombatBuffs
        private static Composite AfflictionCombatBuffs()
        {
            return new PrioritySelector(

                // Symbiosis
                Spell.Cast("Rejuvenation", on => Me, ret => Me.HasAuraExpired("Rejuvenation", 1) && Me.HealthPercent < 95),

                // won't live long with no Pet, so try to summon
                //new Decorator(ret => GetCurrentPet() == WarlockPet.None && GetBestPet() != WarlockPet.None,
                //    SummonPet()),

                new Decorator(req => !Me.CurrentTarget.IsTrivial(),
                    new PrioritySelector(
                        Spell.Cast("Twilight Ward", ret => NeedTwilightWard && !Me.CachedHasAura("Twilight Ward")),
                // need combat healing?  check here since mix of buffs and abilities
                // heal / shield self as needed
                        Spell.Cast("Dark Regeneration", ret => Me.HealthPercent < 45),
                        new Decorator(
                            ret => StyxWoW.Me.HealthPercent < 60 || Me.CachedHasAura("Dark Regeneration"),
                            new PrioritySelector(
                                ctx => Item.FindFirstUsableItemBySpell("Healthstone", "Life Spirit"),
                                new Decorator(
                                    ret => ret != null,
                                    new Sequence(
                                        new Action(ret => Logging.Write("Using {0}", ((WoWItem)ret).Name)),
                                        new Action(ret => ((WoWItem)ret).UseContainerItem()),
                                        Common.CreateWaitForLagDuration())))),


                        new PrioritySelector(
                // find an add within 8 yds (not our current target)
                            ctx => Unit.UnfriendlyUnits(8).FirstOrDefault(u => (u.Combat || Battlegrounds.IsInsideBattleground) && !u.IsStunned() && u.CurrentTargetGuid == Me.Guid && Me.CurrentTargetGuid != u.Guid),

                            Spell.CastOnGround("Shadowfury", on => ((WoWUnit)on).Location, ret => ret != null),

                            // treat as a heal, but we cast on what would be our fear target -- allow even when fear use disabled
                            Spell.Cast("Mortal Coil", on => (WoWUnit)on, ret => !((WoWUnit)ret).IsUndead && Me.HealthPercent < 50),

                        new Decorator(ret => (Me.GotTarget && AdvancedAI.Burst && (!Me.HasAura(113860) && SpellManager.CanCast(113860)) && (Me.CurrentTarget.IsPlayer || Me.CurrentTarget.IsBoss() || Me.CurrentTarget.TimeToDeath() > 20)) || Unit.NearbyUnfriendlyUnits.Count(u => u.IsTargetingMeOrPet) >= 3,
                            new PrioritySelector(
                                Spell.Cast("Dark Soul: Misery"))),

                        Spell.Cast("Summon Doomguard", ret => AdvancedAI.Burst && Me.CurrentTarget.IsBoss() && (PartyBuff.WeHaveBloodlust || Me.CurrentTarget.HealthPercent <= 20)),

                        // lower threat if tanks nearby to pickup
                        Spell.Cast("Soulshatter",
                            ret => AdvancedAI.CurrentWoWContext == WoWContext.Instances
                                && Group.AnyTankNearby
                                && Unit.UnfriendlyUnits(30).Any(u => u.CurrentTargetGuid == Me.Guid)),

                        // lower threat if voidwalker nearby to pickup
                        Spell.Cast("Soulshatter",
                            ret => AdvancedAI.CurrentWoWContext != WoWContext.Battlegrounds
                                && !Group.AnyTankNearby
                                && GetCurrentPet() == WarlockPet.Voidwalker
                                && Unit.UnfriendlyUnits(30).Any(u => u.CurrentTargetGuid == Me.Guid)),

                        Spell.Cast("Dark Bargain", ret => Me.HealthPercent < 45),
                        Spell.Cast("Sacrificial Pact", ret => Me.HealthPercent < 60 && GetCurrentPet() != WarlockPet.None && GetCurrentPet() != WarlockPet.Other && Me.Pet.HealthPercent > 50),

                        new Decorator(ret => Me.HealthPercent < 40 && !Group.AnyHealerNearby,
                            new Sequence(
                                new PrioritySelector(
                                    CastSoulburn(ret => Spell.CanCastHack("Drain Life", Me.CurrentTarget)),
                                    new ActionAlwaysSucceed()),
                                Spell.Cast("Drain Life"))),

                        new Decorator(ret => Unit.NearbyUnfriendlyUnits.Count(u => u.IsTargetingMeOrPet) >= 3 || Unit.NearbyUnfriendlyUnits.Any(u => u.IsPlayer && u.IsTargetingMeOrPet),
                            new PrioritySelector(
                                Spell.Cast("Dark Soul: Misery"),
                                Spell.Cast("Unending Resolve"),
                                new Decorator(ret => TalentManager.IsSelected((int)WarlockTalents.GrimoireOfService),
                                    new PrioritySelector(
                                        Spell.Cast("Grimoire: Felhunter", ret => AdvancedAI.CurrentWoWContext == WoWContext.Battlegrounds),
                                        //Spell.Cast("Grimoire: Voidwalker", ret => GetCurrentPet() != WarlockPet.Voidwalker),
                                        Spell.Cast("Grimoire: Felhunter", ret => GetCurrentPet() != WarlockPet.Felhunter))))),
                        new Decorator(ret=> !Me.IsInGroup(),
                                HealthFunnel(40, 95)),
                        Spell.Cast("Life Tap", ret => Me.ManaPercent < 30 && Me.HealthPercent > 85),
                        PartyBuff.BuffGroup("Dark Intent"),

                        new Decorator(ret => Me.GotTarget && Unit.ValidUnit(Me.CurrentTarget) && (Me.CurrentTarget.IsPlayer || Me.CurrentTarget.TimeToDeath() > 45),
                            new Throttle(2,
                                new PrioritySelector(
                                    Spell.Cast("Curse of the Elements",
                                        ret => !Me.CurrentTarget.CachedHasAura("Curse of the Elements")
                                            && !Me.CurrentTarget.HasMyAura("Curse of Enfeeblement")
                                            && !Me.CurrentTarget.HasAuraWithEffect(WoWApplyAuraType.ModDamageTaken)),
                                    Spell.Cast("Curse of Enfeeblement",
                                        ret => !Me.CurrentTarget.CachedHasAura("Curse of Enfeeblement")
                                            && !Me.CurrentTarget.HasMyAura("Curse of the Elements")
                                            && !Me.CurrentTarget.HasDemoralizing())))),

                        // mana restoration - match against survival cooldowns
                        new Decorator(ret => Me.ManaPercent < 60,
                            new PrioritySelector(
                                Spell.BuffSelf("Life Tap", ret => Me.HealthPercent > 50 && Me.HasAnyAura("Unending Resolve")),
                                Spell.BuffSelf("Life Tap", ret => Me.HasAnyAura("Sacrificial Pact")),
                                Spell.BuffSelf("Life Tap", ret => Me.HasAnyAura("Dark Bargain"))))
                                ))));
        } 
        #endregion

        #region Dot timers

        private static bool Reupdots
        {
            get { return Me.CurrentTarget.HasAuraExpired("Agony", 3) || Me.CurrentTarget.HasAuraExpired("Corruption", 3) || Me.CurrentTarget.HasAuraExpired("Unstable Affliction", 3); }
        }

        #endregion

        #region VoidwalkerDisarm
        private static Composite VoidwalkerDisarm()
        {
            if (GetBestPet() != WarlockPet.Voidwalker)
                return new ActionAlwaysFail();

            return new Decorator(
                req => GetCurrentPet() == WarlockPet.Voidwalker,
                PetManager.CastAction("Disarm", on => Unit.UnfriendlyUnits(10).FirstOrDefault(u => u.IsTargetingMeOrPet && !Me.CurrentTarget.Disarmed && !Me.CurrentTarget.IsCrowdControlled() && Me.IsSafelyFacing(u, 150))));
        } 
        #endregion

        #region HealthFunnel
        private static Composite HealthFunnel(int petMinHealth, int petMaxHealth = 99)
        {
            return new Decorator(
                ret => GetCurrentPet() != WarlockPet.None
                    && Me.Pet.HealthPercent < petMinHealth
                    && !Spell.IsSpellOnCooldown("Health Funnel")
                    && Me.Pet.Distance < 45
                    && Me.Pet.InLineOfSpellSight
                    && !TalentManager.IsSelected((int)WarlockTalents.SoulLink),
                new Sequence(
                    new PrioritySelector(
                // glyph of health funnel prevents Soulburn: Health Funnel from being used
                        new Decorator(ret => TalentManager.HasGlyph("Health Funnel"), new ActionAlwaysSucceed()),
                        CastSoulburn(ret =>
                        {
                            if (Me.Specialization == WoWSpec.WarlockAffliction)
                            {
                                if (Me.CurrentSoulShards > 0 && Spell.CanCastHack("Soulburn", Me))
                                {
                                    Logging.WriteDiagnostic("Soulburn should follow to make instant health funnel");
                                    return true;
                                }
                                Logging.WriteDiagnostic("soulburn not available, shards={0}", Me.CurrentSoulShards);
                            }
                            return false;
                        }),

                        // neither of instant funnels available, so stop moving
                        new Sequence(
                            new Action(ctx => StopMoving.Now()),
                            new Wait(1, until => !Me.IsMoving, new ActionAlwaysSucceed()))),
                    new Decorator(ret => Spell.CanCastHack("Health Funnel", Me.Pet), new ActionAlwaysSucceed()),
                    new Action(ret => Logging.WriteDiagnostic("Casting Health Funnel on Pet @ {0:F1}%", Me.Pet.HealthPercent)),
                    new PrioritySelector(
                        Spell.Cast(ret => "Health Funnel", mov => false, on => Me.Pet, req => Me.HasAura("Soulburn") || TalentManager.HasGlyph("Health Funnel")),
                        Spell.Cast(ret => "Health Funnel", mov => true, on => Me.Pet, req => true, cancel => !Me.GotAlivePet || Me.Pet.HealthPercent >= petMaxHealth)),
                    Common.CreateWaitForLagDuration()));
        } 
        #endregion

        #region NeedTwilightWard
        private static bool NeedTwilightWard
        {
            get
            {
                if (AdvancedAI.CurrentWoWContext == WoWContext.Battlegrounds)
                {
                    if (Unit.NearbyUnfriendlyUnits.Any(u => u.IsPlayer && u.CurrentTargetGuid == Me.Guid && (u.Class == WoWClass.Priest || u.Class == WoWClass.Warlock)))
                    {
                        return true;
                    }
                }
                else
                {
                    if (Me.GotAlivePet && Me.HasAura("Soul Link"))
                    {
                        return true;
                    }
                }

                return Me.HasAura("Dark Bargain");
            }
        } 
        #endregion

        #region SummonPet
        private static Composite SummonPet()
        {
            return new Decorator(
                ret => !Me.HasAura("Grimoire of Sacrifice")
                    && GetBestPet() != GetCurrentPet()
                    && Spell.CanCastHack("Summon Imp"),

                new Sequence(
                // wait for possible auto-spawn if supposed to have a pet and none present
                    new DecoratorContinue(
                        ret => GetCurrentPet() == WarlockPet.None && GetBestPet() != WarlockPet.None && !PetManager.PetSummonAfterDismountTimer.IsFinished,
                        new Sequence(
                            new Action(ret => Logging.WriteDiagnostic("Summon Pet:  waiting {0:F0} on dismount timer for live {1} to appear", PetManager.PetSummonAfterDismountTimer.TimeLeft.TotalMilliseconds, GetBestPet().ToString())),
                            new WaitContinue(
                                TimeSpan.FromDays(1),    // really large value... use PetSummonAfterDismountTimer to control wait duration instead
                                ret => GetCurrentPet() != WarlockPet.None || GetBestPet() == WarlockPet.None || PetManager.PetSummonAfterDismountTimer.IsFinished,
                                new Sequence(
                                    new Action(ret => Logging.WriteDiagnostic("Summon Pet:  found '{0}' after waiting", GetCurrentPet().ToString())),
                                    new Action(r => GetBestPet() == GetCurrentPet() ? RunStatus.Failure : RunStatus.Success))))),

                    // dismiss pet if wrong one is alive
                    new DecoratorContinue(
                        ret => GetCurrentPet() != GetBestPet() && GetCurrentPet() != WarlockPet.None,
                        new Sequence(
                            new Action(ret => Logging.WriteDiagnostic("Summon Pet:  dismissing {0}", GetCurrentPet().ToString())),
                            new Action(ctx => Lua.DoString("PetDismiss()")),
                            new WaitContinue(
                                TimeSpan.FromMilliseconds(1000),
                                ret => GetCurrentPet() == WarlockPet.None,
                                new Action(ret =>
                                {
                                    Logging.WriteDiagnostic("Summon Pet:  dismiss complete");
                                    return RunStatus.Success;
                                })))),

                    // summon pet best pet (unless best is none)
                    new DecoratorContinue(
                        ret => GetBestPet() != WarlockPet.None && GetBestPet() != GetCurrentPet(),
                        new Sequence(
            #region Instant Pet Summon Check
new PrioritySelector(
                                new Decorator(
                                    req => StyxWoW.Me.HasAura("Soulburn"),
                                    new Action(r => Logging.Write("^Instant Summon Pet: Soulburn already active - should work!"))),
                                CastSoulburn(ret =>
                                {

                                    if (Me.CurrentSoulShards == 0)
                                        Logging.WriteDiagnostic("CreateWarlockSummonPet:  no shards so instant pet summon not available");
                                    else if (!Me.Combat && !Unit.NearbyUnfriendlyUnits.Any(u => u.Combat || u.IsPlayer))
                                        Logging.WriteDiagnostic("CreateWarlockSummonPet:  not in combat and no imminent danger nearby, so saving shards");
                                    else if (!Spell.CanCastHack("Soulburn", Me))
                                        Logging.WriteDiagnostic("soulburn not available, shards={0}", Me.CurrentSoulShards);
                                    else
                                    {
                                        Logging.Write("^Instant Summon Pet: Soulburn - hope it works!");
                                        return true;
                                    }

                                    return false;
                                }),
                                new Action(r => Logging.WriteDiagnostic("instant summon not active, continuing..."))),
            #endregion Instant Pet Summon Check

 new Action(ret => Logging.WriteDiagnostic("Summon Pet:  about to summon{0}", GetBestPet().ToString().CamelToSpaced())),
                            Spell.Cast(n => "Summon" + GetBestPet().ToString().CamelToSpaced(),
                                chkMov => true,
                                onUnit => Me,
                                req => true,
                                cncl => GetBestPet() == GetCurrentPet()),

                            // make sure we see pet alive before continuing
                            new Wait(1, ret => GetCurrentPet() != WarlockPet.None, new ActionAlwaysSucceed())
                            )
                        )
                    )
                );
        } 
        #endregion

        #region Aoe
        private static Composite Aoe()
        {
            return new Decorator(
                ret => AdvancedAI.Aoe,
                new PrioritySelector(
                    new Decorator(
                        ret => Unit.UnfriendlyUnitsNearTarget(10).Count() >= 4 && SpellManager.HasSpell("Seed of Corruption"),
                        new PrioritySelector(
                // if current target doesn't have CotE, then Soulburn+CotE
                            new Decorator(
                                req => !Me.CurrentTarget.HasAura("Curse of the Elements"),
                                new Sequence(
                                    CastSoulburn(req => true),
                                    Spell.Buff("Curse of the Elements"))),
                // roll SoC on targets in combat that we are facing
                            new PrioritySelector(
                                ctx => TargetsInCombat.FirstOrDefault(m => !m.HasAura("Seed of Corruption")),
                                new Sequence(
                                    new PrioritySelector(
                                        CastSoulburn(req => req != null),
                                        new ActionAlwaysSucceed()),
                                    Spell.Cast("Seed of Corruption", on => (WoWUnit)on))))),
                    new Decorator(
                        ret => Unit.UnfriendlyUnitsNearTarget(10).Count() >= 2,
                        new PrioritySelector(
                            ApplyDots(on => TargetsInCombat.FirstOrDefault(m => m.HasAuraExpired("Agony")), soulBurn => true),
                            ApplyDots(on => TargetsInCombat.FirstOrDefault(m => m.HasAuraExpired("Unstable Affliction")), soulBurn => true)))));
        } 
        #endregion

        #region GetBestPet
        private static WarlockPet GetBestPet()
        {
            var currPet = GetCurrentPet();
            if (currPet == WarlockPet.Other)
                return currPet;

            WarlockPet bestPet;
            if (AdvancedAI.CurrentWoWContext != WoWContext.Instances)
                bestPet = WarlockPet.Felhunter;
            else if (Me.Level >= 30)
                bestPet = WarlockPet.Felhunter;
            else
                bestPet = WarlockPet.Imp;

            return bestPet;
        } 
        #endregion

        #region GetCurrentPet
        private static WarlockPet GetCurrentPet()
        {
            if (!Me.GotAlivePet)
                return WarlockPet.None;

            if (Me.Pet == null)
            {
                Logging.WriteDiagnostic("????? GetCurrentPet unstable - have live pet but Me.Pet == null !!!!!");
                return WarlockPet.None;
            }

            try
            {
                // following will fail when we have a non-creature warlock pet
                // .. this happens in quests where we get a pet assigned as Me.Pet (like Eric "The Swift")
            }
            catch
            {
                return WarlockPet.Other;
            }

            switch ((WarlockGrimoireOfSupremecyPets)Me.Pet.CreatureFamilyInfo.Id)
            {
                case (WarlockGrimoireOfSupremecyPets)WarlockPet.Imp:
                case (WarlockGrimoireOfSupremecyPets)WarlockPet.Felguard:
                case (WarlockGrimoireOfSupremecyPets)WarlockPet.Voidwalker:
                case (WarlockGrimoireOfSupremecyPets)WarlockPet.Felhunter:
                case (WarlockGrimoireOfSupremecyPets)WarlockPet.Succubus:
                    return (WarlockPet)Me.Pet.CreatureFamilyInfo.Id;

                case WarlockGrimoireOfSupremecyPets.FelImp:
                    return WarlockPet.Imp;
                case WarlockGrimoireOfSupremecyPets.Wrathguard:
                    return WarlockPet.Felguard;
                case WarlockGrimoireOfSupremecyPets.Voidlord:
                    return WarlockPet.Voidwalker;
                case WarlockGrimoireOfSupremecyPets.Observer:
                    return WarlockPet.Felhunter;
                case WarlockGrimoireOfSupremecyPets.Shivarra:
                    return WarlockPet.Succubus;
            }

            return WarlockPet.Other;
        } 
        #endregion

        #region WarlockPets
        private enum WarlockPet
        {
            None = 0,
            Imp = 23,       // Pet.CreatureFamily.Id
            Voidwalker = 16,
            Succubus = 17,
            Felhunter = 15,
            Felguard = 29,
            Other = 99999     // a quest or other pet forced upon us for some reason
        } 
        #endregion

        #region GrimoirePets
        private enum WarlockGrimoireOfSupremecyPets
        {
            FelImp = 100,
            Wrathguard = 104,
            Voidlord = 101,
            Observer = 103,
            Shivarra = 102
        }  
        #endregion

        #region TargetsInCombat
        private static IEnumerable<WoWUnit> TargetsInCombat
        {
            get
            {
                return Unit.UnfriendlyUnits(40).Where(u => u.Combat && u.IsTargetingUs() && !u.IsCrowdControlled() && Me.IsSafelyFacing(u));
            }
        }  
        #endregion
        
        #region ApplyDots
        private static int _dotCount;
        private static Composite ApplyDots(UnitSelectionDelegate onUnit, SimpleBooleanDelegate soulBurn)
        {
            return new PrioritySelector(
                    new Decorator(ret => !Me.HasAura("Soulburn"),
                        new Throttle(2,
                        new PrioritySelector(
                            // target below 20% we have a higher prior on Haunt (but skip if soulburn already up...)
                            Spell.Cast("Haunt",
                                on => onUnit(on),
                                req => Me.CurrentSoulShards > 0
                                    && Me.CurrentTarget.HealthPercent < 20
                                    && !Me.HasAura("Soulburn") && !onUnit(req).HasAura("Haunt")),

                            // otherwise, save 2 shards for Soulburn and instant pet rez if needed (unless Misery buff up)
                            new Throttle(2,
                            Spell.Cast("Haunt", on => onUnit(on), ret => Me.CurrentSoulShards > 3 || Me.HasAura("Dark Soul: Misery") && !onUnit(ret).HasAura("Haunt")))))),

                    new Sequence(
                        CastSoulburn(
                            ret => soulBurn(ret)
                                && onUnit != null && onUnit(ret) != null
                                //&& onUnit(ret).CurrentHealth > 1
                                && (onUnit(ret).HasAuraExpired("Agony", 3) || onUnit(ret).HasAuraExpired("Corruption", 3) || onUnit(ret).HasAuraExpired("Unstable Affliction", 3))
                                && onUnit(ret).InLineOfSpellSight
                                && Me.CurrentSoulShards > 0),
                        CastSoulSwap(onUnit)),

                    new Action(ret =>
                    {
                        _dotCount = 0;
                        if (onUnit != null && onUnit(ret) != null)
                        {
                            if (!onUnit(ret).HasAuraExpired("Agony", 3))
                                ++_dotCount;
                            if (!onUnit(ret).HasAuraExpired("Corruption", 3))
                                ++_dotCount;
                            if (!onUnit(ret).HasAuraExpired("Unstable Affliction", 3))
                                ++_dotCount;
                            if (!onUnit(ret).HasAuraExpired("Haunt", 3))
                                ++_dotCount;

                             //if mob dying very soon, skip DoTs
                            //if (onUnit(ret).TimeToDeath() < 4)
                            //    _dotCount = 4;
                        }
                        return RunStatus.Failure;
                    }),
                    
                    new Decorator(req => _dotCount < 4,
                        new Throttle(1,
                        new PrioritySelector(
                            CastSoulSwap(onUnit),
                            Spell.Cast("Agony", onUnit, ret => !onUnit(ret).CachedHasAura("Agony") || onUnit(ret).HasAuraExpired("Agony", 3)),
                            Spell.Cast("Corruption", onUnit, ret => !onUnit(ret).CachedHasAura("Corruption") || onUnit(ret).HasAuraExpired("Corruption", 3)),
                            new Throttle(2,
                                Spell.Cast("Unstable Affliction", onUnit, ret => !onUnit(ret).HasAura("Unstable Affliction") || onUnit(ret).HasAuraExpired("Unstable Affliction", 3)))))));
            
        }
        #endregion

        #region CastSoulBurn
        private static Composite CastSoulburn(SimpleBooleanDelegate requirements)
        {
            return new Throttle(2,
                new Sequence(
                Spell.Cast("Soulburn", on => Me, ret => Me.CurrentSoulShards > 0  && requirements(ret) && (Me.CurrentTarget.Elite || Me.CurrentTarget.IsBoss()) && !Me.HasAura("Soulburn")),
                new Wait(TimeSpan.FromMilliseconds(500), ret => Me.HasAura("Soulburn"), new Action(ret => RunStatus.Success))
                ));
        }
        #endregion

        #region CastSoulSwap
        private static Composite CastSoulSwap(UnitSelectionDelegate onUnit)
        {
            return new Throttle(1,
                new Decorator(
                    ret => Me.HasAura("Soulburn")
                        && onUnit != null && onUnit(ret) != null
                        && onUnit(ret).IsAlive
                        && (onUnit(ret).HasAuraExpired("Agony", 3) || onUnit(ret).HasAuraExpired("Corruption", 3) || onUnit(ret).HasAuraExpired("Unstable Affliction", 3))
                        && onUnit(ret).Distance <= 40
                        && onUnit(ret).InLineOfSpellSight,
                    new Action(ret =>
                    {
                        Logging.Write("Casting Soul Swap on {0}", onUnit(ret).SafeName());
                        SpellManager.Cast("Soul Swap", onUnit(ret));
                    })));
        }
        #endregion
        
        #region Talents
        enum WarlockTalents
        {
            None = 0,
            DarkRegeneration,
            SoulLeech,
            HarvestLife,
            DemonicBreath,
            MortalCoil,
            Shadowfury,
            SoulLink,
            SacrificialPact,
            DarkBargain,
            BloodHorror,
            BurningRush,
            UnboundWill,
            GrimoireOfSupremacy,
            GrimoireOfService,
            GrimoireOfSacrifice,
            ArchimondesDarkness,
            KiljadensCunning,
            MannorothsFury
        }
        #endregion
    }
}
