using Styx;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals.WoWObjects;
using AdvancedAI.Helpers;
using Action = Styx.TreeSharp.Action;
using CommonBehaviors.Actions;

namespace AdvancedAI.Spec
{
    class UnholyDeathknightPvP
    {
        //public override WoWClass Class { get { return WoWClass.DeathKnight; } }
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        private const int SuddenDoom = 81340;

        internal static int BloodRuneSlotsActive { get { return Me.GetRuneCount(0) + Me.GetRuneCount(1); } }
        internal static int FrostRuneSlotsActive { get { return Me.GetRuneCount(2) + Me.GetRuneCount(3); } }
        internal static int UnholyRuneSlotsActive { get { return Me.GetRuneCount(4) + Me.GetRuneCount(5); } }

        public static Composite CreateUDKPvPBuffs
        {
            get
            {
                return new PrioritySelector(
                Spell.Cast("Raise Dead", ret => !StyxWoW.Me.GotAlivePet),
                Spell.Cast("Horn of Winter", ret => !Me.HasAura("Horn of Winter")),
                new ActionAlwaysSucceed()
                );
            }
        }


        public static Composite CreateUDKPvPCombat
        {
            get
            {
                return new PrioritySelector(
                    // Interrupt please.
                    //Spell.Cast("Mind Freeze", ret => StyxWoW.Me.CurrentTarget.IsCasting && StyxWoW.Me.CurrentTarget.CanInterruptCurrentSpellCast),
                    //Spell.Cast("Strangulate", ret => StyxWoW.Me.CurrentTarget.IsCasting && StyxWoW.Me.CurrentTarget.CanInterruptCurrentSpellCast),

                    //Staying Alive
                         //Item.CreateUsePotionAndHealthstone(60, 40),

                        Spell.Cast("Conversion",
                            ret => Me.HealthPercent < 50 && Me.RunicPowerPercent >= 20 && !Me.HasAura("Conversion")),

                         Spell.Cast("Death Pact",
                            ret => StyxWoW.Me.HealthPercent < 45),

                         Spell.Cast("Death Siphon",
                            ret => StyxWoW.Me.HealthPercent < 50),

                         Spell.Cast("Icebound Fortiude",
                            ret => StyxWoW.Me.HealthPercent < 40),

                         Spell.Cast("Death Strike",
                            ret => StyxWoW.Me.GotTarget &&
                                   StyxWoW.Me.HealthPercent < 15),

                        Spell.Cast("Lichborne",
                             ret => // use it to heal with deathcoils.
                                    (StyxWoW.Me.HealthPercent < 25
                                    && StyxWoW.Me.CurrentRunicPower >= 60)),

                        Spell.Cast("Death Coil", at => Me,
                              ret => StyxWoW.Me.HealthPercent < 50 &&
                                     StyxWoW.Me.HasAura("Lichborne")),

                    new Throttle(2,
                        new PrioritySelector(
                        Spell.Cast("Blood Tap",
                              ret => Me.HasAura("Blood Charge", 10) &&
                              (Me.UnholyRuneCount == 0 || Me.DeathRuneCount == 0 || Me.FrostRuneCount == 0)))),

                    //Dispells
                   new Decorator(ret => Me.CurrentTarget.HasAnyAura("Power Word: Shield", "Dark Soul: Instability", "Dark Soul: Knowledge", "Dark Soul: Misery", "Icy Veins",
                                                                    "Hand of Protection", "Innervate", "Incanter's Ward", "Alter Time", "Power Infusion", "Stay of Execution",
                                                                    "Eternal Flame", "Spiritwalker's Grace", "Ancestral Swiftness"),
                        new PrioritySelector(
                            Spell.Cast("Icy Touch"))),

                    // AOE
                    //new Decorator (ret => UnfriendlyUnits.Count() >= 2, CreateAoe()),

                    // Execute
                            Spell.Cast("Soul Reaper",
                            ret => StyxWoW.Me.CurrentTarget.HealthPercent < 37),

                    // Diseases
                            Spell.Cast("Outbreak",
                               ret => !StyxWoW.Me.CurrentTarget.HasMyAura("Frost Fever") ||
                                      !StyxWoW.Me.CurrentTarget.HasMyAura("Blood Plague")),

                            Spell.Cast("Plague Strike",
                                ret => !StyxWoW.Me.CurrentTarget.HasMyAura("Blood Plague") || !StyxWoW.Me.CurrentTarget.HasMyAura("Frost Fever")),

                            //Spell.Cast("Unholy Blight",
                    //    ret => Unit.UnfriendlyUnitsNearTarget(12f).Count() >= 2 &&
                    //           TalentManager.IsSelected((int)Singular.ClassSpecific.DeathKnight.Common.DeathKnightTalents.UnholyBlight) &&
                    //           StyxWoW.Me.CurrentTarget.DistanceSqr <= 10 * 10 &&
                    //           !StyxWoW.Me.HasAura("Unholy Blight")),

                            //Spell.Cast("Blood Boil",
                    //    ret => Unit.UnfriendlyUnitsNearTarget(12f).Count() >= 2 &&
                    //           TalentManager.IsSelected((int)Singular.ClassSpecific.DeathKnight.Common.DeathKnightTalents.RollingBlood) &&
                    //           !StyxWoW.Me.HasAura("Unholy Blight") &&
                    //           //StyxWoW.Me.CurrentTarget.DistanceSqr <= 15 * 15 && 
                    //           Singular.ClassSpecific.DeathKnight.Common.ShouldSpreadDiseases),

                    //Cooldowns

                    new Decorator(ret => StyxWoW.Me.CurrentTarget.IsWithinMeleeRange && Me.CurrentTarget.HealthPercent <= 40,
                        new PrioritySelector(

                            Spell.Cast("Unholy Frenzy",
                                ret => StyxWoW.Me.CurrentTarget.IsWithinMeleeRange &&
                                      !PartyBuff.WeHaveBloodlust),

                            new Action(ret => { Item.UseTrinkets(); return RunStatus.Failure; }),
                            new Action(ret => { Item.UseHands(); return RunStatus.Failure; }),

                            Spell.Cast("Summon Gargoyle"))),

                            //Spell.Cast("Pestilence",
                    //    ret => Unit.UnfriendlyUnitsNearTarget(12f).Count() >= 2 &&
                    //           !StyxWoW.Me.HasAura("Unholy Blight") &&
                    //           Singular.ClassSpecific.DeathKnight.Common.ShouldSpreadDiseases),

                    //Kill Time
                            Spell.Cast("Dark Transformation",
                               ret => Me.GotAlivePet &&
                                      !Me.Pet.ActiveAuras.ContainsKey("Dark Transformation") &&
                                      Me.HasAura("Shadow Infusion", 5)),

                            Spell.Cast("Necrotic Strike"),

                            Spell.CastOnGround("Death and Decay", ret => StyxWoW.Me.CurrentTarget.Location, ret => true, false),

                            Spell.Cast("Scourge Strike",
                                ret => StyxWoW.Me.UnholyRuneCount == 2 || StyxWoW.Me.DeathRuneCount > 0),

                            Spell.Cast("Festering Strike",
                                ret => StyxWoW.Me.BloodRuneCount == 2 && StyxWoW.Me.FrostRuneCount == 2),

                            Spell.Cast("Death Coil",
                               ret => (StyxWoW.Me.HasAura(SuddenDoom) || StyxWoW.Me.CurrentRunicPower >= 90)),// && StyxWoW.Me.Auras["Shadow Infusion"].StackCount < 5),

                            Spell.Cast("Scourge Strike"),

                            Spell.Cast("Plague Leech",
                                ret => SpellManager.Spells["Outbreak"].CooldownTimeLeft.Seconds <= 1),

                            Spell.Cast("Festering Strike"),

                            //Blood Tap
                            Spell.Cast("Blood Tap", ret =>
                                Me.HasAura("Blood Charge", 5)
                                && (BloodRuneSlotsActive == 0 || FrostRuneSlotsActive == 0 || UnholyRuneSlotsActive == 0)),

                            Spell.Cast("Death Coil",
                                ret => SpellManager.Spells["Lichborne"].CooldownTimeLeft.Seconds >= 4 && Me.CurrentRunicPower < 60 || !Me.HasAura("Conversion")), // || StyxWoW.Me.Auras["Shadow Infusion"].StackCount == 5),

                            Spell.Cast("Horn of Winter"),
                            new ActionAlwaysSucceed()

                            //Spell.Cast("Empower Rune Weapon",
                    //    ret => StyxWoW.Me.BloodRuneCount == 0 && StyxWoW.Me.FrostRuneCount == 0 && StyxWoW.Me.UnholyRuneCount == 0)
                    );
            }
        }
    }
}
