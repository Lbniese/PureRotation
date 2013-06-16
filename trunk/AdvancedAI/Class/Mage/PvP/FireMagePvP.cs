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
using Styx.Pathing;
using AdvancedAI.Managers;

namespace AdvancedAI.Spec
{
    class FireMagePvP : AdvancedAI
    {
        public override WoWClass Class { get { return WoWClass.Mage; } }
        //public override WoWSpec Spec { get { return WoWSpec.MageFire; } }
        static LocalPlayer Me { get { return StyxWoW.Me; } }

        protected override Composite CreateCombat()
        {
            return new PrioritySelector(
                #region LossOfControl
                // deal with Ice Block here (a stun of our own doing)
                new Decorator(
                    ret => Me.ActiveAuras.ContainsKey("Ice Block"),
                    new PrioritySelector(
                        new Throttle(10, new Action(r => Logging.Write("^Ice Block for 10 secs"))),
                            //Logger.Write(Color.DodgerBlue, "^Ice Block for 10 secs"))),
                        new Decorator(
                            ret => DateTime.Now < _cancelIceBlockForCauterize && !Me.ActiveAuras.ContainsKey("Cauterize"),
                            new Action(ret =>
                            {
                                Logging.Write("/cancel Ice Block since Cauterize has expired");
                                //Logger.Write(Color.White, "/cancel Ice Block since Cauterize has expired");
                                _cancelIceBlockForCauterize = DateTime.MinValue;
                                // Me.GetAuraByName("Ice Block").TryCancelAura();
                                Me.CancelAura("Ice Block");
                                return RunStatus.Success;
                            })
                            ),
                        new ActionIdle()
                        )
                    ),

                Spell.BuffSelf("Blink", ret => Me.Stunned),
                Spell.BuffSelf("Temporal Shield", ret => Me.Stunned),
                #endregion

                new Decorator(ret => isStrafing && !StyxWoW.Me.Combat,
                    new Action( ret =>
                    {
                        Logging.Write("Stopping Strafe out of Combat");
                        //Logger.Write(Color.White, "Stopping Strafe out of Combat");
                            WoWMovement.MoveStop();
                            isStrafing = false;
                        })
                    ),


                CreateStayAwayBehavior(),
                //Common.CreateChannelWaitBehavior(),
                CreateStayAwayFromFrozenTargetsBehavior(),
                new Decorator(
                    ret => StyxWoW.Me.HasAura("Ice Block") || StyxWoW.Me.HasAura("Greater Invisibility"),
                    new ActionIdle()),
                new Action(ret =>
                {
                    if (!StyxWoW.Me.IsSafelyFacing(StyxWoW.Me.CurrentTarget))
                        StyxWoW.Me.CurrentTarget.Face();
                    return RunStatus.Failure;
                }),
                //Movement.CreateFaceTargetBehavior(),
                StrafeBehavior(),
                Spell.WaitForCast(),
                Movement.CreateMoveToLosBehavior(),

                // Defensive stuff
                CreateMageLivingBombOnAddBehavior(),
                Spell.BuffSelf("Ice Block", ret => StyxWoW.Me.HealthPercent < 30 && !StyxWoW.Me.ActiveAuras.ContainsKey("Hypothermia")),
                Spell.BuffSelf("Slow Fall", ret => StyxWoW.Me.IsFalling),
                Spell.BuffSelf("Alter Time", ret => StyxWoW.Me.HealthPercent <= 45 && Unit.NearbyUnfriendlyUnits.Any(u => u.IsTargetingMeOrPet)),
                Spell.BuffSelf("Blazing Speed", ret => Unit.NearbyUnfriendlyUnits.Any(u => u.IsTargetingMeOrPet && u.DistanceSqr < 8 * 8)),
                Spell.BuffSelf("Blink", ret => Me.IsStunned() || Me.IsRooted() || (Unit.NearbyUnfriendlyUnits.Any(u => u.DistanceSqr < 8 * 8 && !u.IsCrowdControlled()))),
                Spell.BuffSelf("Incanter's Ward", ret => StyxWoW.Me.HealthPercent <= 75),
                CreateUseManaGemBehavior(ret => StyxWoW.Me.ManaPercent < 80),
                //Common.CreateMagePolymorphOnAddBehavior(),
                // Cooldowns
                new Decorator(
                    ret => Unit.NearbyUnfriendlyUnits.Count(u => u.IsTargetingMeOrPet) >= 2 && StyxWoW.Me.HealthPercent < 70,
                    new PrioritySelector(
                        Spell.BuffSelf("Mirror Image")
                        )),
                Spell.BuffSelf("Invisibility", ret => StyxWoW.Me.HealthPercent <= 50),
                Spell.BuffSelf("Ice Barrier"),
                Spell.BuffSelf("Evocation", ret => StyxWoW.Me.ManaPercent < 35),
                new Decorator(
                    ret => Unit.UnfriendlyUnitsNearTarget(10f).Count() >= 2,
                    new PrioritySelector(
                        Spell.CastOnGround("Flamestrike",
                            ret => StyxWoW.Me.CurrentTarget.Location)
                        )),
                new Decorator(
                    ret => Unit.UnfriendlyUnitsNearTarget(15f).Count() >= 2,
                        Spell.CastOnGround("Ring of Frost",
                            ret => StyxWoW.Me.CurrentTarget.Location)),
                Spell.Cast("Pyroblast", ret => StyxWoW.Me.HasAura("Pyroblast!")),
                Spell.Cast("Inferno Blast", ret => StyxWoW.Me.HasAura("Heating Up")),
                Spell.Cast("Spellsteal", ret => Spellsteal()),
                new Decorator(ret => Unit.NearbyUnfriendlyUnits.Any(u => u.DistanceSqr < 10 * 10 && !u.IsCrowdControlled()),
                        Spell.Cast("Frost Nova",
                            ret => Unit.NearbyUnfriendlyUnits.Any(u =>
                                            u.DistanceSqr <= 8 * 8 && !u.HasAura("Freeze") &&
                                            !u.HasAura("Frost Nova") && !u.Stunned))),
                Spell.Cast("Cone of Cold",
                    ret => StyxWoW.Me.IsSafelyFacing(StyxWoW.Me.CurrentTarget, 90) &&
                           StyxWoW.Me.CurrentTarget.DistanceSqr <= 8 * 8),
                Spell.Cast("Deep Freeze", ret => StyxWoW.Me.CurrentTarget.HasAura("Frost Nova")),
                Spell.Cast("Ice Lance", ret => StyxWoW.Me.CurrentTarget.HasAura("Deep Freeze")),
                Spell.Cast("Dragon's Breath",
                    ret => StyxWoW.Me.IsSafelyFacing(StyxWoW.Me.CurrentTarget, 90) &&
                           StyxWoW.Me.CurrentTarget.DistanceSqr <= 8 * 8),
                Spell.Buff("Living Bomb", ret => !StyxWoW.Me.CurrentTarget.HasAura("Living Bomb")),

                Spell.Cast("Fire Blast"),
                // Rotation
                Spell.Cast("Combustion",
                    ret => StyxWoW.Me.CurrentTarget.HasMyAura("Living Bomb") && (StyxWoW.Me.CurrentTarget.HasMyAura("Ignite") && StyxWoW.Me.CurrentTarget.HasMyAura("Pyroblast"))),
                Spell.Cast("Fireball", ret => !Unit.NearbyUnfriendlyUnits.Any(u => u.IsTargetingMeOrPet) && !StyxWoW.Me.IsMoving),
                Spell.Cast("Scorch"),
                //Spell.Cast("Fireball"),
                Movement.CreateMoveToTargetBehavior(true, 30f)
                );
        }

        protected override Composite CreateBuffs()
        {
            return new PrioritySelector(
                // Defensive 

                PartyBuff.BuffGroup("Dalaran Brilliance", "Arcane Brilliance"),
                PartyBuff.BuffGroup("Arcane Brilliance", "Dalaran Brilliance"),

                // Additional armors/barriers for BGs. These should be kept up at all times to ensure we're as survivable as possible.
                new Decorator(
                    ret => Me.CurrentMap.IsBattleground,
                    new PrioritySelector(

                        // only FA if in battlegrounds or we have move slightly since last FA (to avoid repeated casts in place when stuck)
                        Spell.BuffSelf("Frost Armor", ret =>
                        {
                            if (!Me.CurrentMap.IsBattleground && Me.Location.Distance(locLastFrostArmor) < 1)
                                return false;
                            locLastFrostArmor = Me.Location;
                            return true;
                        })
                        )
                    )
                );
        }

        private static bool isStrafing = false;
        private static DateTime _cancelIceBlockForCauterize = DateTime.MinValue;
        private static WoWPoint locLastFrostArmor = WoWPoint.Empty;
        private static WoWPoint locLastIceBarrier = WoWPoint.Empty;

        public static Composite CreateMageLivingBombOnAddBehavior()
        {
            return new PrioritySelector(
                    ctx => Unit.NearbyUnfriendlyUnits.Where(
                                u => u.DistanceSqr > 35 * 35 && u.IsPlayer && u.IsAlive && !u.HasMyAura("Living Bomb")).OrderBy(u => u.Distance).FirstOrDefault(),
                    new Decorator(
                        ret => ret != null,
                        Spell.Buff("Living Bomb", ret => (WoWUnit)ret, ret => Unit.NearbyUnfriendlyUnits.Count(u => u.HasMyAura("Living Bomb")) <= 2)));
        }

        private static Composite CreateSlowMeleeBehavior()
        {
            return new Decorator(
                ret => Unit.NearbyUnfriendlyUnits.Any(u => u.SpellDistance() <= 8 && !u.Stunned && !u.Rooted && !u.IsSlowed()),
                new PrioritySelector(
                    new Decorator(
                        ret => Me.Specialization == WoWSpec.MageFrost,
                        CastFreeze(on => Clusters.GetBestUnitForCluster(Unit.NearbyUnfriendlyUnits.Where(u => u.SpellDistance() < 8), ClusterType.Radius, 8))
                        ),
                    Spell.Buff("Frost Nova"),
                    Spell.Buff("Frostjaw"),
                    Spell.CastOnGround("Ring of Frost", loc => Me.Location, req => true, false),
                    Spell.Buff("Cone of Cold")
                    )
                );
        }

        private static WoWPoint _locFreeze;
        public static Composite CastFreeze(UnitSelectionDelegate onUnit)
        {
            return new Sequence(
                new Decorator(
                    ret => onUnit != null && onUnit(ret) != null,
                    new Action(ret => _locFreeze = onUnit(ret).Location)
                    ),
                Pet.CreateCastPetActionOnLocation(
                    "Freeze",
                    on => _locFreeze,
                    ret => Me.Pet.ManaPercent >= 12
                        && Me.Pet.Location.Distance(_locFreeze) < 45
                        && !Me.CurrentTarget.IsFrozen()
                    )
                );
        }

        private const uint ArcanePowder = 17020;
        private static readonly uint[] MageFoodIds = new uint[]
            { 65500, 65515, 65516, 65517, 43518, 43523, 65499, 80610, 80618 };

        private static bool ShouldSummonTable
        {
            get
            {
                return SpellManager.HasSpell("Conjure Refreshment Table")
                    && Unit.NearbyGroupMembers.Any(p => !p.IsMe);
            }
        }

        static readonly uint[] RefreshmentTableIds = new uint[]
            { 186812, 207386, 207387 };

        static private WoWGameObject MageTable
            { get { return ObjectManager.GetObjectsOfType<WoWGameObject>().FirstOrDefault(i => RefreshmentTableIds.Contains(i.Entry) && (StyxWoW.Me.PartyMembers.Any(p => p.Guid == i.CreatedByGuid) || StyxWoW.Me.Guid == i.CreatedByGuid)); } }
        
        private static int CarriedMageFoodCount
            { get { return (int)StyxWoW.Me.CarriedItems.Sum(i => i != null
                           && i.ItemInfo != null
                           && i.ItemInfo.ItemClass == WoWItemClass.Consumable
                           && i.ItemSpells != null
                           && i.ItemSpells.Count > 0
                           && i.ItemSpells[0].ActualSpell.Name.Contains("Refreshment")
                           ? i.StackCount : 0); } }

        public static bool Gotfood { get { return StyxWoW.Me.BagItems.Any(item => MageFoodIds.Contains(item.Entry)); } }
        private static bool HaveManaGem { get { return StyxWoW.Me.BagItems.Any(i => i.Entry == 36799 || i.Entry == 81901); } }
        public static Composite CreateUseManaGemBehavior() { return CreateUseManaGemBehavior(ret => true); }

        public static Composite CreateUseManaGemBehavior(SimpleBooleanDelegate requirements)
        {
            return new PrioritySelector(
                ctx => StyxWoW.Me.BagItems.FirstOrDefault(i => i.Entry == 36799 || i.Entry == 81901),
                new Decorator(
                    ret => ret != null && StyxWoW.Me.ManaPercent < 100 && ((WoWItem)ret).Cooldown == 0 && requirements(ret),
                    new Sequence(
                        new Action(ret => Logging.Write("Using {0}", ((WoWItem)ret).Name)),
                            //Logger.Write("Using {0}", ((WoWItem)ret).Name)),
                        new Action(ret => ((WoWItem)ret).Use())))
                );
        }

        public static Composite CreateChannelWaitBehavior()
        {
            return
                new Action(ret =>
                {
                    if ((StyxWoW.Me.ActiveAuras.ContainsKey("Invisibility") || StyxWoW.Me.ActiveAuras.ContainsKey("Evocation")) && StyxWoW.Me.CurrentHealth > 20)
                        return RunStatus.Failure;
                    return RunStatus.Running;
                });

        }

        public static Composite StrafeBehavior()
        {
            return new PrioritySelector(
                        new Decorator(ret => StyxWoW.Me.CurrentTarget.Distance > 35 || !StyxWoW.Me.CurrentTarget.IsAlive || !StyxWoW.Me.CurrentTarget.InLineOfSight,
                            new Action(
                                ret =>
                                {
                                    Logging.WriteDiagnostic("Stop Strafe - Check Fail");
                                    //Logger.WriteDebug("Stop Strafe - Check Fail");
                                    if (StyxWoW.Me.MovementInfo.MovingStrafeRight)
                                        WoWMovement.MoveStop(WoWMovement.MovementDirection.StrafeRight);

                                    if (StyxWoW.Me.MovementInfo.MovingStrafeLeft)
                                        WoWMovement.MoveStop(WoWMovement.MovementDirection.StrafeLeft);
                                    Navigator.MoveTo(StyxWoW.Me.CurrentTarget.Location);
                                })),
                        new Decorator(
                            ret => StyxWoW.Me.CurrentTarget.InLineOfSight && !StyxWoW.Me.IsMoving && StyxWoW.Me.CurrentTarget.Distance < 30 && StyxWoW.Me.CurrentTarget.IsPlayer &&
                            ((StyxWoW.Me.CastingSpell != null && StyxWoW.Me.CastingSpell.Name.Contains("Scorch")) || Unit.NearbyUnfriendlyUnits.Any(u => u.IsTargetingMeOrPet)),
                            new Sequence(
                                new Action(
                                    ret =>
                                    {
                                        Random dir1 = new Random();
                                        int leftright = dir1.Next(1, 100);
                                        if (!StyxWoW.Me.IsMoving)
                                        {
                                            var obj = ObjectManager.GetObjectsOfType<WoWGameObject>(true, false).
                                            Where(x => x.DistanceSqr <= 15 * 15).
                                            OrderBy(x => x.DistanceSqr).
                                            FirstOrDefault();

                                            var strafeloc = ObjectManager.GetObjectsOfType<WoWPlayer>(true, false).
                                            Where(x => x.DistanceSqr <= 45 * 45).
                                            OrderBy(x => x.DistanceSqr).
                                            FirstOrDefault();

                                            var from = StyxWoW.Me.Location;
                                            isStrafing = true;
                                            if (obj == null)
                                            {
                                                Logging.Write("Random Strafe");
                                                //Logger.Write(Color.White, "Random Strafe");
                                                var frontVector = new Vector2(strafeloc.X - from.X, strafeloc.Y - from.Y);
                                                frontVector.Normalize();
                                                frontVector.X *= strafeloc.InteractRange + 5 + Navigator.PathPrecision;
                                                frontVector.Y *= strafeloc.InteractRange + 5 + Navigator.PathPrecision;

                                                var sideVector = new Vector2(-frontVector.Y, frontVector.X);

                                                var leftSpot = from.Add(sideVector.X, sideVector.Y, 0);
                                                var rightSpot = from.Add(-sideVector.X, -sideVector.Y, 0);
                                                if (Navigator.CanNavigateFully(from, rightSpot) &&
                                                    Navigator.CanNavigateFully(from, leftSpot))
                                                {
                                                    WoWMovement.MovementDirection way = (Environment.TickCount & 1) == 0
                                                                ? WoWMovement.MovementDirection.StrafeLeft
                                                                : WoWMovement.MovementDirection.StrafeRight;
                                                    WoWMovement.Move(way, TimeSpan.FromSeconds(dir1.Next(5500, 9500)));
                                                }
                                                else if (leftright < 50 && Navigator.CanNavigateFully(from, rightSpot))
                                                {
                                                    WoWMovement.Move(WoWMovement.MovementDirection.StrafeRight, TimeSpan.FromSeconds(dir1.Next(5500, 9500)));
                                                }
                                                else if (Navigator.CanNavigateFully(from, leftSpot))
                                                {
                                                    WoWMovement.Move(WoWMovement.MovementDirection.StrafeLeft, TimeSpan.FromSeconds(dir1.Next(5500, 9500)));
                                                }
                                            }
                                            else
                                            {
                                                var frontVector = new Vector2(obj.X - from.X, obj.Y - from.Y);
                                                frontVector.Normalize();
                                                frontVector.X *= obj.InteractRange + 5 + Navigator.PathPrecision;
                                                frontVector.Y *= obj.InteractRange + 5 + Navigator.PathPrecision;

                                                var sideVector = new Vector2(-frontVector.Y, frontVector.X);

                                                var leftSpot = from.Add(sideVector.X, sideVector.Y, 0);
                                                var rightSpot = from.Add(-sideVector.X, -sideVector.Y, 0);
                                                Logging.Write("Planned Strafe");
                                                //Logger.Write(Color.White, "Planned Strafe");

                                                if (Navigator.CanNavigateFully(from, rightSpot) &&
                                                    Navigator.CanNavigateFully(from, leftSpot))
                                                {
                                                    WoWMovement.MovementDirection way = (Environment.TickCount & 1) == 0
                                                                ? WoWMovement.MovementDirection.StrafeLeft
                                                                : WoWMovement.MovementDirection.StrafeRight;
                                                    WoWMovement.Move(way, TimeSpan.FromSeconds(dir1.Next(5500, 9500)));
                                                }
                                                else if (Navigator.CanNavigateFully(from, leftSpot))
                                                {
                                                    WoWMovement.Move(WoWMovement.MovementDirection.StrafeLeft, TimeSpan.FromSeconds(dir1.Next(5500, 9500)));
                                                }
                                                else if (Navigator.CanNavigateFully(from, rightSpot))
                                                {
                                                    WoWMovement.Move(WoWMovement.MovementDirection.StrafeRight, TimeSpan.FromSeconds(dir1.Next(5500, 9500)));
                                                }
                                            }
                                        }
                                    }),

                                    new WaitContinue(TimeSpan.FromMilliseconds(1850), ret =>
                                    {
                                        StyxWoW.Me.CurrentTarget.Face();
                                        if (!StyxWoW.Me.CurrentTarget.IsAlive || !StyxWoW.Me.CurrentTarget.InLineOfSpellSight)
                                        {
                                            if (StyxWoW.Me.MovementInfo.MovingStrafeRight)
                                                WoWMovement.MoveStop(WoWMovement.MovementDirection.StrafeRight);

                                            if (StyxWoW.Me.MovementInfo.MovingStrafeLeft)
                                                WoWMovement.MoveStop(WoWMovement.MovementDirection.StrafeLeft);

                                            if (StyxWoW.Me.MovementInfo.MovingForward)
                                                WoWMovement.MoveStop(WoWMovement.MovementDirection.Forward);

                                            if (StyxWoW.Me.MovementInfo.MovingBackward)
                                                WoWMovement.MoveStop(WoWMovement.MovementDirection.Backwards);
                                            return false;
                                        }

                                        if (StyxWoW.Me.CurrentTarget.DistanceSqr >= 35)
                                        {
                                            Movement.CreateMoveToTargetBehavior(true, 20f);
                                        }
                                        return true;
                                    }, new ActionAlwaysSucceed())
                                    )));
        }

        public static Composite CreateStayAwayBehavior()
        {
            WoWPoint moveTo = StyxWoW.Me.Location;
            WoWObject obj = null;
            obj = ObjectManager.GetObjectsOfType<WoWPlayer>().Where(
                                u => !u.IsFriendly && u.IsPlayer && u.IsAlive).OrderBy(u => u.Distance).
                                FirstOrDefault();
            return
                new Decorator(
                    ret => (
                        StyxWoW.Me.ActiveAuras.ContainsKey("Greater Invisibility") ||
                        StyxWoW.Me.ActiveAuras.ContainsKey("Invisibility") ||
                        StyxWoW.Me.ActiveAuras.ContainsKey("Blazing Speed") ||
                        StyxWoW.Me.ActiveAuras.ContainsKey("Alter Time")),
                    new Action(
                        ret =>
                        {
                            if (obj != null)
                            {
                                moveTo = WoWMathHelper.CalculatePointFrom(StyxWoW.Me.Location, obj.Location, Spell.MeleeRange + 25f);
                            }
                            else
                            {
                                moveTo = WoWMathHelper.CalculatePointFrom(StyxWoW.Me.Location, StyxWoW.Me.Location, Spell.MeleeRange + 25f);
                            }
                            if (Navigator.CanNavigateFully(StyxWoW.Me.Location, moveTo))
                            {
                                Navigator.MoveTo(moveTo);
                                return RunStatus.Success;
                            }

                            return RunStatus.Failure;
                        }));
        }

        public static Composite CreateStayAwayFromFrozenTargetsBehavior()
        {
            return new PrioritySelector(
                ctx => Unit.NearbyUnfriendlyUnits.
                           Where(u => u.IsFrozen() && u.Distance < Spell.MeleeRange + 3f).
                           OrderBy(u => u.DistanceSqr).FirstOrDefault(),
                new Decorator(
                    ret => ret != null,
                    new PrioritySelector(
                        Disengage.CreateDisengageBehavior("Blink", Disengage.Direction.Frontwards, 20, null),
                        Disengage.CreateDisengageBehavior("Rocket Jump", Disengage.Direction.Frontwards, 20, null),
                        new Action(
                            ret =>
                            {
                                WoWPoint moveTo =
                                    WoWMathHelper.CalculatePointBehind(
                                        ((WoWUnit)ret).Location,
                                        ((WoWUnit)ret).Rotation,
                                        -(Spell.MeleeRange + 5f));

                                if (Navigator.CanNavigateFully(StyxWoW.Me.Location, moveTo))
                                {
                                    Logging.Write("Getting away from frozen target");
                                    //Logger.Write("Getting away from frozen target");
                                    Navigator.MoveTo(moveTo);
                                    return RunStatus.Success;
                                }

                                return RunStatus.Failure;
                            }))));
        }

        public static string[] StealBuffs = { "Innervate", "Hand of Freedom", "Hand of Protection", "Regrowth", "Rejuvenation", "Lifebloom", "Renew", 
                                      "Hand of Salvation", "Power Infusion", "Power Word: Shield", "Arcane Power", "Hot Streak!", "Avenging Wrath", 
                                      "Elemental Mastery", "Nature's Swiftness", "Divine Plea", "Divine Favor", "Icy Veins", "Ice Barrier", "Holy Shield", 
                                      "Divine Aegis", "Bloodlust", "Time Warp", "Holy Shield", "Brain Freeze"};

        public static bool Spellsteal()
        {
            string[] StealBuffs = { "Innervate", "Hand of Freedom", "Hand of Protection", "Regrowth", "Rejuvenation", "Lifebloom", "Renew", 
                                      "Hand of Salvation", "Power Infusion", "Power Word: Shield", "Arcane Power", "Hot Streak!", "Avenging Wrath", 
                                      "Elemental Mastery", "Nature's Swiftness", "Divine Plea", "Divine Favor", "Icy Veins", "Ice Barrier", "Holy Shield", 
                                      "Divine Aegis", "Bloodlust", "Time Warp", "Holy Shield", "Brain Freeze"};

            foreach (string spell in StealBuffs)
            {
                if (StyxWoW.Me.CurrentTarget.HasAura(spell) && !StyxWoW.Me.HasAura(spell) && StyxWoW.Me.ManaPercent > 40)
                {
                    return true;
                }
            }
            return false;
        }

        public static Composite CreateMageSpellstealBehavior()
        {
            return Spell.Cast("Spellsteal",
                on =>
                {
                    WoWUnit unit = GetSpellstealTarget();
                    if (unit != null)
                        Logging.WriteDiagnostic("Spellsteal:  found {0} with a triggering aura, cancast={1}", unit.SafeName(), SpellManager.CanCast("Spellsteal", unit));
                        //Logger.WriteDebug("Spellsteal:  found {0} with a triggering aura, cancast={1}", unit.SafeName(), SpellManager.CanCast("Spellsteal", unit));
                    return unit;
                });
        }

        public static WoWUnit GetSpellstealTarget()
        {
                if (Me.GotTarget && null != GetSpellstealAura(Me.CurrentTarget))
                {
                    return Me.CurrentTarget;
                }

                if (!Me.GotTarget)
                {
                    WoWUnit target = Unit.NearbyUnfriendlyUnits.FirstOrDefault(u => Me.IsSafelyFacing(u) && null != GetSpellstealAura(u));
                    return target;
                }
            return null;
        }

        public static WoWAura GetSpellstealAura(WoWUnit target)
        {
            return target.GetAllAuras().FirstOrDefault(a => a.TimeLeft.TotalSeconds > 5 && a.Equals(StealBuffs) && !Me.HasAura(a.SpellId));
        }

        public static Composite CreateMagePolymorphOnAddBehavior()
        {
            return
                new PrioritySelector(
                    ctx => Unit.NearbyUnfriendlyUnits.OrderByDescending(u => u.CurrentHealth).FirstOrDefault(IsViableForPolymorph),
                    new Decorator(
                        ret => ret != null && Unit.NearbyUnfriendlyUnits.All(u => !u.HasMyAura("Polymorph")),
                        new PrioritySelector(
                            Spell.Buff("Polymorph", ret => (WoWUnit)ret))));
        }

        private static bool IsViableForPolymorph(WoWUnit unit)
        {
            if (unit.IsCrowdControlled())
                return false;

            if (unit.CreatureType != WoWCreatureType.Beast && unit.CreatureType != WoWCreatureType.Humanoid)
                return false;

            if (StyxWoW.Me.CurrentTarget != null && StyxWoW.Me.CurrentTarget == unit)
                return false;

            if (!unit.Combat)
                return false;

            if (!unit.IsTargetingMeOrPet && !unit.IsTargetingMyPartyMember)
                return false;

            if (StyxWoW.Me.GroupInfo.IsInParty && StyxWoW.Me.PartyMembers.Any(p => p.CurrentTarget != null && p.CurrentTarget == unit))
                return false;

            return true;
        }

        public static bool HasTalent(MageTalents tal)
        {
            return TalentManager.IsSelected((int)tal);
        }

        public enum MageTalents
        {
            None = 0,
            PresenceOfMind,
            BlazingSpeed,
            IceFloes,
            TemporalShield,
            Flameglow,
            IceBarrier,
            RingOfFrost,
            IceWard,
            Frostjaw,
            GreaterInivisibility,
            Cauterize,
            ColdSnap,
            NetherTempest,
            LivingBomb,
            FrostBomb,
            Invocation,
            RuneOfPower,
            IncantersWard
        }

    }
}
