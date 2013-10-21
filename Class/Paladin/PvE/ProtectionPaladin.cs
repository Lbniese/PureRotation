using System.Linq;
using AdvancedAI.Helpers;
using AdvancedAI.Managers;
using CommonBehaviors.Actions;
using Styx;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

namespace AdvancedAI.Class.Paladin.PvE
{
    class ProtectionPaladin
    {
        static LocalPlayer Me { get { return StyxWoW.Me; } }
        
        public static Composite ProtectionCombat()           
            {
                return new PrioritySelector(
                    //new Decorator(ret => AdvancedAI.PvPRot,
                    //    ProtectionPaladinPvP.CreatePPPvPCombat),
                    new Decorator(ret => !Me.Combat && !Me.CurrentTarget.IsAlive && Me.IsCasting,
                        new ActionAlwaysSucceed()),
                    // Interrupt please.
                    Spell.Cast("Rebuke", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast),
                    Spell.Cast("Avenger's Shield", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast),

                    //Change seals if I need mana or at low health Seals need more work......  
                    Spell.Cast("Seal of Insight", ret => (Me.ManaPercent <= 10 || Me.HealthPercent <= 50) && !Me.HasAura("Seal of Insight")),
                    //Spell.Cast("Seal of Truth", ret => Me.ManaPercent >= 30 && Me.HealthPercent > 50 && Unit.UnfriendlyUnits(8).Count() <= 3 && !Me.HasAura("Seal of Truth")),
                    //Spell.Cast("Seal of Righteousness", ret => Me.ManaPercent >= 30 && Me.HealthPercent > 50 && Unit.UnfriendlyUnits(8).Count() >= 4 && !Me.HasAura("Seal of Righteousness")),

                    Spell.Cast(Seal()),


                    //Staying alive
                    Spell.Cast("Sacred Shield",on => Me, ret => !Me.HasAura("Sacred Shield") && SpellManager.HasSpell("Sacred Shield")),
                    Spell.Cast("Lay on Hands", on => Me, ret => Me.HealthPercent <= 10 && !Me.HasAura("Forbearance")),
                    //Spell.Cast("Guardian of Ancient Kings", ret => StyxWoW.Me.HealthPercent <= 40),
                    Spell.Cast("Ardent Defender", ret => Me.HealthPercent <= 15 && Me.HasAura("Forbearance")),
                    //Nedd to work on this cause its only magic dmg that it stops unless glyphed
                    //Spell.Cast("Divine Protection", ret => Me.HealthPercent <= 80 && !Me.HasAura("Shield of the Righteous") && TalentManager.HasGlyph("Divine Protection")),
                    Spell.Cast("Divine Protection", ret => Me.HealthPercent <= 80 && !Me.HasAura("Shield of the Righteous")),
                    //Spell.Cast("Holy Avenger", ret => StyxWoW.Me.HealthPercent <= 60),

                    Spell.Cast("Word of Glory", ret => Me.HealthPercent < 50 && (Me.CurrentHolyPower >= 3 || Me.HasAura("Divine Purpose"))),
                    Spell.Cast("Word of Glory", ret => Me.HealthPercent < 25 && (Me.CurrentHolyPower >= 2 || Me.HasAura("Divine Purpose"))),
                    Spell.Cast("Word of Glory", ret => Me.HealthPercent < 15 && (Me.CurrentHolyPower >= 1 || Me.HasAura("Divine Purpose"))),

                    //Prot T15 2pc 
                    new Decorator(ret => AdvancedAI.UsefulStuff,
                        new PrioritySelector(
                            Spell.Cast("Word of Glory", ret => Me.HealthPercent < 90 && Me.CurrentHolyPower == 1 && !Me.HasAura("Shield of Glory")),
                            Spell.Cast("Word of Glory", ret => Me.HealthPercent < 75 && Me.CurrentHolyPower <= 2 && !Me.HasAura("Shield of Glory")),
                            Spell.Cast("Word of Glory", ret => Me.HealthPercent < 50 && (Me.CurrentHolyPower >= 3 || Me.HasAura("Divine Purpose")) && !Me.HasAura("Shield of Glory")))),

                    CreateDispelBehavior(),

                    new Decorator(ret => Unit.UnfriendlyUnits(8).Count() >= 2,
                        CreateAoe()),

                    Spell.Cast("Shield of the Righteous", ret => (Me.CurrentHolyPower == 5 || Me.HasAura("Divine Purpose")) && AdvancedAI.Burst),//need hotkey 
                    Spell.Cast("Hammer of the Righteous", ret => !Me.CurrentTarget.ActiveAuras.ContainsKey("Weakened Blows")),
                    Spell.Cast("Judgment", ret => SpellManager.HasSpell("Sanctified Wrath") && Me.HasAura("Avenging Wrath")),
                    Spell.Cast("Avenger's Shield", ret => Me.ActiveAuras.ContainsKey("Grand Crusader")),
                    Spell.Cast("Crusader Strike"),
                    Spell.Cast("Judgment"),
                    //Spell.Cast("Sacred Shield",on => Me, ret => !Me.HasAura("Sacred Shield") && SpellManager.HasSpell("Sacred Shield")),
                    LightsHammer(),
                    //Spell.CastOnGround("Light's Hammer", ret => Me.CurrentTarget.Location),
                    Spell.Cast("Holy Prism", on => Unit.UnfriendlyUnits(15).Count() >= 2 ? Me : Me.CurrentTarget),
                    Spell.Cast("Execution Sentence"),
                    Spell.Cast("Hammer of Wrath"),
                    Spell.Cast("Shield of the Righteous", ret => Me.CurrentHolyPower >= 3 && AdvancedAI.Burst),//need hotkey 
                    Spell.Cast("Avenger's Shield"),
                    Spell.Cast("Consecration", ret => !Me.IsMoving && Me.CurrentTarget.IsWithinMeleeRange),
                    Spell.Cast("Holy Wrath", ret => Me.CurrentTarget.IsWithinMeleeRange)
                    );
            }
        
        private static Composite CreateAoe()
        {
            return new PrioritySelector(
                Spell.Cast("Shield of the Righteous", ret => (Me.CurrentHolyPower == 5 || Me.HasAura("Divine Purpose")) && AdvancedAI.Burst),//need hotkey 
                Spell.Cast("Judgment", ret => SpellManager.HasSpell("Sanctified Wrath") && Me.HasAura("Avenging Wrath")),
                Spell.Cast("Hammer of the Righteous"),
                Spell.Cast("Judgment"),
                Spell.Cast("Avenger's Shield", ret => Me.ActiveAuras.ContainsKey("Grand Crusader")),
                //Spell.Cast("Sacred Shield", on => Me, ret => !Me.HasAura("Sacred Shield") && SpellManager.HasSpell("Sacred Shield")),
                LightsHammer(),
                //Spell.CastOnGround("Light's Hammer", ret => Me.CurrentTarget.Location, ret => true, false),
                Spell.Cast("Holy Prism", on => Me, ret => Me.HealthPercent <= 90),
                Spell.Cast("Execution Sentence"),
                Spell.Cast("Hammer of Wrath"),
                Spell.Cast("Shield of the Righteous", ret => Me.CurrentHolyPower >= 3 && AdvancedAI.Burst),//need hotkey 
                Spell.Cast("Consecration", ret => !Me.IsMoving && Me.CurrentTarget.IsWithinMeleeRange),
                Spell.Cast("Avenger's Shield"),
                Spell.Cast("Holy Wrath", ret => Me.CurrentTarget.IsWithinMeleeRange),
                new ActionAlwaysSucceed());
        }


        public static Composite ProtectionPreCombatBuffs()
        {
                return new PrioritySelector(
                    //new Decorator(ret => AdvancedAI.PvPRot,
                    //    ProtectionPaladinPvP.CreatePPPvPBuffs)
                        );
         }

        public static WoWUnit dispeltar
        {
            get
            {
                var dispelothers = (from unit in ObjectManager.GetObjectsOfType<WoWPlayer>(false)
                                    where unit.IsAlive
                                    where Dispelling.CanDispel(unit)
                                    select unit).OrderByDescending(u => u.HealthPercent).LastOrDefault();
                return dispelothers;
            }
        }

        public static Composite CreateDispelBehavior()
        {
            return new PrioritySelector(
                Spell.Cast("Cleanse", on => Me, ret => Dispelling.CanDispel(Me)),
                Spell.Cast("Cleanse", on => dispeltar, ret => Dispelling.CanDispel(dispeltar)));
        }

        private static string Seal()
        {
            if (Me.ManaPercent >= 30 && Me.HealthPercent > 50)
            {
                if (Unit.UnfriendlyUnits(8).Count() >= 5)
                {
                    if (!Me.HasAura("Seal of Righteousness"))
                    {
                        return "Seal of Righteousness";
                    }
                }
                if (!Me.HasAura("Seal of Truth"))
                {
                    return "Seal of Truth";
                }
            }
            return null;
        }

        #region Light's Hammer
        private static Composite LightsHammer()
        {
            return new Decorator(ret => SpellManager.HasSpell("Light's Hammer") && Unit.UnfriendlyUnits(10).Any(),
                new Action(ret =>
                {
                    var tpos = Me.CurrentTarget.Location;
                    var mpos = Me.Location;

                    SpellManager.Cast("Light's Hammer");
                    SpellManager.ClickRemoteLocation(mpos);
                }));
        }
        #endregion

        #region PaladinTalents
        public enum PaladinTalents
        {
            SpeedofLight = 1,//Tier 1
            LongArmoftheLaw,
            PersuitofJustice,
            FistofJustice,//Tier 2
            Repentance,
            BurdenofGuilt,
            SelflessHealer,//Tier 3
            EternalFlame,
            SacredShield,
            HandofPurity,//Tier 4
            UnbreakableSpirit,
            Clemency,
            HolyAvenger,//Tier 5
            SanctifiedWrath,
            DivinePurpose,
            HolyPrism,//Tier 6
            LightsHammer,
            ExecutionSentence
        }
        #endregion
    }
}
