using System.Windows.Forms;
using Styx;
using Styx.Common;
using Styx.CommonBot.Routines;
using Styx.TreeSharp;
using Styx.WoWInternals;
using CommonBehaviors.Actions;
using AdvancedAI.Managers;
using AdvancedAI.Helpers;


namespace AdvancedAI
{
    public abstract class AdvancedAI : CombatRoutine
    {
        #region init
        public override void Initialize()
        {
            RegisterHotkeys();
            CombatandBuffSelection();
            MovementManager.Init();
            Dispelling.Init();
            //base.Initialize();
        }

        public override sealed string Name { get { return "AdvancedAI [" + StyxWoW.Me.Specialization + "]"; } }
        public override WoWClass Class { get { return StyxWoW.Me.Class; } }
        //public WoWSpec Spec { get { return TalentManager.CurrentSpec; } }
        private Composite _combat, _buffs;
        public override Composite CombatBehavior { get { return _combat; } }
        public override Composite PreCombatBuffBehavior { get { return _buffs; } }
        #endregion

        #region Overrides            

        internal Composite CombatandBuffSelection()
        {
            if (_combat == null)
            {
                Logging.Write("Initializing combat behaviors.");
                _combat = null;
            }
            if (_buffs == null)
            {
                Logging.Write("Initializing buffs behaviors.");
                _buffs = null;
            }
            switch (StyxWoW.Me.Specialization)
            {
                case WoWSpec.DeathKnightBlood:
                    if (_combat == null) { _combat = Spec.BloodDeathknight.CreateBDKCombat; }
                    if (_buffs == null) { _buffs = Spec.BloodDeathknight.CreateBDKBuffs; }
                    break;
                case WoWSpec.DeathKnightFrost:
                    if (_combat == null) { _combat = Spec.FrostDeathknight.CreateFDKCombat; }
                    if (_buffs == null) { _buffs = Spec.FrostDeathknight.CreateFDKBuffs; }
                    break;
                case WoWSpec.DeathKnightUnholy:
                    if (_combat == null) { _combat = Spec.UnholyDeathknight.CreateUDKCombat; }
                    if (_buffs == null) { _buffs = Spec.UnholyDeathknight.CreateUDKBuffs; }
                    break;
                case WoWSpec.DruidBalance:
                    if (_combat == null) { _combat = Spec.BallanceDruid.CreateBDCombat; }
                    if (_buffs == null) { _buffs = Spec.BallanceDruid.CreateBDBuffs; }
                    break;
                case WoWSpec.DruidFeral:
                    if (_combat == null) { _combat = Spec.FeralDruid.CreateFDCombat; }
                    if (_buffs == null) { _buffs = Spec.FeralDruid.CreateFDBuffs; }
                    break;
                case WoWSpec.DruidGuardian:
                    if (_combat == null) { _combat = Spec.GuardianDruid.CreateGDCombat; }
                    if (_buffs == null) { _buffs = Spec.GuardianDruid.CreateGDBuffs; }
                    break;
                case WoWSpec.DruidRestoration:
                    if (_combat == null) { _combat = Spec.RestorationDruid.CreateRDCombat; }
                    if (_buffs == null) { _buffs = Spec.RestorationDruid.CreateRDBuffs; }
                    break;
                case WoWSpec.HunterBeastMastery:
                    if (_combat == null) { _combat = Spec.BeastmasterHunter.CreateBMHCombat; }
                    if (_buffs == null) { _buffs = Spec.BeastmasterHunter.CreateBMHBuffs; }
                    break;
                case WoWSpec.HunterMarksmanship:
                    if (_combat == null) { _combat = Spec.MarksmanshipHunter.CreateMHCombat; }
                    if (_buffs == null) { _buffs = Spec.MarksmanshipHunter.CreateMHKBuffs; }
                    break;
                case WoWSpec.HunterSurvival:
                    if (_combat == null) { _combat = Spec.SurvivalHunter.CreateSHCombat; }
                    if (_buffs == null) { _buffs = Spec.SurvivalHunter.CreateSHBuffs; }
                    break;
                case WoWSpec.MageArcane:
                    if (_combat == null) { _combat = Spec.ArcaneMage.CreateAMCombat; }
                    if (_buffs == null) { _buffs = Spec.ArcaneMage.CreateAMBuffs; }
                    break;
                case WoWSpec.MageFire:
                    if (_combat == null) { _combat = Spec.FireMage.CreateFMCombat; }
                    if (_buffs == null) { _buffs = Spec.FireMage.CreateFMBuffs; }
                    break;
                case WoWSpec.MageFrost:
                    if (_combat == null) { _combat = Spec.FrostMage.CreateFMCombat; }
                    if (_buffs == null) { _buffs = Spec.FrostMage.CreateFMBuffs; }
                    break;
                case WoWSpec.MonkBrewmaster:
                    if (_combat == null) { _combat = Spec.BrewmasterMonk.CreateBMCombat; }
                    if (_buffs == null) { _buffs = Spec.BrewmasterMonk.CreateBMBuffs; }
                    break;
                case WoWSpec.MonkMistweaver:
                    if (_combat == null) { _combat = Spec.MistweaverMonk.CreateMMCombat; }
                    if (_buffs == null) { _buffs = Spec.MistweaverMonk.CreateMMBuffs; }
                    break;
                case WoWSpec.MonkWindwalker:
                    if (_combat == null) { _combat = Spec.WindwalkerMonk.CreateWMCombat; }
                    if (_buffs == null) { _buffs = Spec.WindwalkerMonk.CreateWMBuffs; }
                    break;
                case WoWSpec.PaladinHoly:
                    if (_combat == null) { _combat = Spec.HolyPaladin.CreateHPCombat; }
                    if (_buffs == null) { _buffs = Spec.HolyPaladin.CreateHPBuffs; }
                    break;
                case WoWSpec.PaladinProtection:
                    if (_combat == null) { _combat = Spec.ProtectionPaladin.CreatePPCombat; }
                    if (_buffs == null) { _buffs = Spec.ProtectionPaladin.CreatePPBuffs; }
                    break;
                case WoWSpec.PaladinRetribution:
                    if (_combat == null) { _combat = Spec.RetributionPaladin.CreateRPCombat; }
                    if (_buffs == null) { _buffs = Spec.RetributionPaladin.CreateRPBuffs; }
                    break;
                case WoWSpec.PriestDiscipline:
                    if (_combat == null) { _combat = Spec.DisciplinePriest.CreateDPCombat; }
                    if (_buffs == null) { _buffs = Spec.DisciplinePriest.CreateDPBuffs; }
                    break;
                case WoWSpec.PriestHoly:
                    if (_combat == null) { _combat = Spec.HolyPriest.CreateHPCombat; }
                    if (_buffs == null) { _buffs = Spec.HolyPriest.CreateHPBuffs; }
                    break;
                case WoWSpec.PriestShadow:
                    if (_combat == null) { _combat = Spec.ShadowPriest.CreateSPCombat; }
                    if (_buffs == null) { _buffs = Spec.ShadowPriest.CreateSPBuffs; }
                    break;
                case WoWSpec.RogueAssassination:
                    if (_combat == null) { _combat = Spec.AssassinationRogue.CreateARCombat; }
                    if (_buffs == null) { _buffs = Spec.AssassinationRogue.CreateARBuffs; }
                    break;
                case WoWSpec.RogueCombat:
                    if (_combat == null) { _combat = Spec.CombatRogue.CreateCRCombat; }
                    if (_buffs == null) { _buffs = Spec.CombatRogue.CreateCRBuffs; }
                    break;
                case WoWSpec.RogueSubtlety:
                    if (_combat == null) { _combat = Spec.SubtletyRogue.CreateSRCombat; }
                    if (_buffs == null) { _buffs = Spec.SubtletyRogue.CreateSRBuffs; }
                    break;
                case WoWSpec.ShamanElemental:
                    if (_combat == null) { _combat = Spec.ElementalShaman.CreateESCombat; }
                    if (_buffs == null) { _buffs = Spec.ElementalShaman.CreateESBuffs; }
                    break;
                case WoWSpec.ShamanEnhancement:
                    if (_combat == null) { _combat = Spec.EnhancementShaman.CreateESCombat; }
                    if (_buffs == null) { _buffs = Spec.EnhancementShaman.CreateESBuffs; }
                    break;
                case WoWSpec.ShamanRestoration:
                    if (_combat == null) { _combat = Spec.RestorationShaman.CreateRSCombat; }
                    if (_buffs == null) { _buffs = Spec.RestorationShaman.CreateRSBuffs; }
                    break;
                case WoWSpec.WarlockAffliction:
                    if (_combat == null) { _combat = Spec.AfflictionWarlock.CreateAWCombat; }
                    if (_buffs == null) { _buffs = Spec.AfflictionWarlock.CreateAWBuffs; }
                    break;
                case WoWSpec.WarlockDemonology:
                    if (_combat == null) { _combat = Spec.DemonologyWarlock.CreateDWCombat; }
                    if (_buffs == null) { _buffs = Spec.DemonologyWarlock.CreateDWBuffs; }
                    break;
                case WoWSpec.WarlockDestruction:
                    if (_combat == null) { _combat = Spec.DestructionWarlock.CreateDWCombat; }
                    if (_buffs == null) { _buffs = Spec.DestructionWarlock.CreateDWBuffs; }
                    break;
                case WoWSpec.WarriorArms:
                    if (_combat == null) { _combat = Spec.ArmsWarrior.CreateAWCombat; }
                    if (_buffs == null) { _buffs = Spec.ArmsWarrior.CreateAWBuffs; }
                    break;
                case WoWSpec.WarriorFury:
                    if (_combat == null) { _combat = Spec.FuryWarrior.CreateFWCombat; }
                    if (_buffs == null) { _buffs = Spec.FuryWarrior.CreateFWBuffs; }
                    break;
                case WoWSpec.WarriorProtection:
                    if (_combat == null) { _combat = Spec.ProtectionWarrior.CreatePWCombat; }
                    if (_buffs == null) { _buffs = Spec.ProtectionWarrior.CreatePWBuffs; }
                    break;
            }
            return null;
        }

        public sealed override void Combat() { base.Combat(); }
        public sealed override void CombatBuff() { base.CombatBuff(); }
        public sealed override void Death() { base.Death(); }
        public sealed override void Heal() { base.Heal(); }
        public sealed override Composite MoveToTargetBehavior { get { return base.MoveToTargetBehavior; } }
        public sealed override bool NeedCombatBuffs { get { return base.NeedCombatBuffs; } }
        public sealed override bool NeedDeath { get { return base.NeedDeath; } }
        public sealed override bool NeedHeal { get { return base.NeedHeal; } }
        public sealed override bool NeedPreCombatBuffs { get { return base.NeedPreCombatBuffs; } }
        public sealed override bool NeedPullBuffs { get { return base.NeedPullBuffs; } }
        public sealed override bool NeedRest { get { return base.NeedRest; } }
        public sealed override void PreCombatBuff() { base.PreCombatBuff(); }
        public sealed override void Pull() { base.Pull(); }
        public sealed override void Rest() { base.Rest(); }
        #endregion

        public static bool InterruptsEnabled { get; set; }
        public static bool PvPRot { get; set; }
        public static bool PvPBurst { get; set; }
        protected virtual void UnregisterHotkeys()
        {
            HotkeysManager.Unregister("Ares Toggle Interrupt");
        }
        protected virtual void RegisterHotkeys()
        {
            HotkeysManager.Register("Ares Toggle Interrupt",
                Keys.NumPad1,
                ModifierKeys.Alt,
                o =>
                {
                    InterruptsEnabled = !InterruptsEnabled;
                    Logging.Write("Interrupts enabled: " + InterruptsEnabled);
                });
            // Default this to true please. Thanks!
            InterruptsEnabled = true;

            HotkeysManager.Register("PvP Toggle",
            Keys.P,
            ModifierKeys.Alt,
            o =>
            {
                PvPRot = !PvPRot;
                Logging.Write("PvP enabled: " + PvPRot);
                Lua.DoString("print('PvP Enabled: " + PvPRot + "')");
            });
            PvPRot = false;

            HotkeysManager.Register("PvP Burst",
            Keys.NumPad1,
            ModifierKeys.Control,
            o =>
            {
                PvPBurst = !PvPBurst;
                Logging.Write("PvP Burst enabled: " + PvPBurst);
                Lua.DoString("print('PvP Burst Enabled: " + PvPBurst + "')");
            });
            PvPBurst = false;
        }

        #region Requirements
        protected virtual Composite CreateCombat()
        {
            return new HookExecutor("AdvancedAI_Combat_Root",
                "Root composite for AdvancedAI combat. Rotations will be plugged into this hook.",
                new ActionAlwaysFail());
        }
        protected virtual Composite CreateBuffs()
        {
            return new HookExecutor("AdvancedAI_Buffs_Root",
                "Root composite for AdvancedAI buffs. Rotations will be plugged into this hook.",
                new ActionAlwaysFail());
        }
        #endregion
    }    
}
