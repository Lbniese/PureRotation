using System.Windows.Forms;
using Styx.Common;
using Styx.WoWInternals;

namespace AdvancedAI
{
    partial class AdvancedAI
    {
        public static bool InterruptsEnabled { get; set; }
        public static bool PvPRot { get; set; }
        public static bool PvERot { get; set; }
        public static bool Burst { get; set; }
        public static bool HexFocus { get; set; }
        public static bool Movement { get; set; }
        public static bool UsefulStuff { get; set; }
        public static bool Aoe { get; set; }
        public static bool BossMechs { get; set; }
        public static bool Weave { get; set; }
        public static bool Dispell { get; set; }
        public static bool Trace { get; set; }
        public static bool LFRMode { get; set; }
        public static bool ManualContext { get; set; }

        protected virtual void UnregisterHotkeys()
        {
            HotkeysManager.Unregister("Toggle Interrupt");
            HotkeysManager.Unregister("PvP Toggle");
            HotkeysManager.Unregister("PvE Toggle");
            HotkeysManager.Unregister("Burst");
            HotkeysManager.Unregister("Hex Focus");
            HotkeysManager.Unregister("Movement");
            HotkeysManager.Unregister("Useful Stuff");
            HotkeysManager.Unregister("AOE");
            HotkeysManager.Unregister("Boss Mechs");
            HotkeysManager.Unregister("Weave");
            HotkeysManager.Unregister("Dispelling");
            HotkeysManager.Unregister("Trace");
            HotkeysManager.Unregister("LFR Mode");
            HotkeysManager.Unregister("Manual Context");
        }
        protected virtual void RegisterHotkeys()
        {
            HotkeysManager.Register("Manual Context",
                Keys.L,
                ModifierKeys.Alt,
                o =>
                {
                    ManualContext = !ManualContext;
                    Logging.Write("Manual Context enabled: " + ManualContext);
                    Lua.DoString("print('Manual Context Enabled: " + ManualContext + "')");
                });
            ManualContext = false;

            HotkeysManager.Register("Trace",
                Keys.T,
                ModifierKeys.Alt,
                o =>
                {
                    Trace = !Trace;
                    Logging.Write("Trace enabled: " + Trace);
                    Lua.DoString("print('Trace Enabled: " + Trace + "')");
                });
            Trace = false;

            HotkeysManager.Register("Dispelling",
                Keys.D,
                ModifierKeys.Alt,
                o =>
                {
                    Dispell = !Dispell;
                    Logging.Write("Dispelling enabled: " + Dispell);
                    Lua.DoString("print('Dispelling Enabled: " + Dispell + "')");
                });
            Dispell = true;

            HotkeysManager.Register("Toggle Interupt",
                Keys.NumPad1,
                ModifierKeys.Alt,
                o =>
                {
                    InterruptsEnabled = !InterruptsEnabled;
                    Logging.Write("Interrupts enabled: " + InterruptsEnabled);
                    Lua.DoString("print('Interrupts Enabled: " + InterruptsEnabled + "')");
                });
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

            HotkeysManager.Register("PvE Toggle",
            Keys.O,
            ModifierKeys.Alt,
            o =>
            {
                PvERot = !PvERot;
                Logging.Write("PvE enabled: " + PvERot);
                Lua.DoString("print('PvE Enabled: " + PvERot + "')");
            });
            PvERot = false;

            HotkeysManager.Register("Burst",
            Keys.NumPad1,
            ModifierKeys.Control,
            o =>
            {
                Burst = !Burst;
                Logging.Write("Burst enabled: " + Burst);
                Lua.DoString("print('Burst Enabled: " + Burst + "')");
            });
            Burst = true;

            HotkeysManager.Register("Hex Focus",
            Keys.NumPad2,
            ModifierKeys.Control,
            o =>
            {
                HexFocus = !HexFocus;
                Logging.Write("Hex Focus enabled: " + HexFocus);
                Lua.DoString("print('Hex Focus Enabled: " + HexFocus + "')");
            });
            HexFocus = false;

            HotkeysManager.Register("Movement Enabled",
            Keys.M,
            ModifierKeys.Alt,
            o =>
            {
                Movement = !Movement;
                Logging.Write("Movement Enabled: " + Movement);
                Lua.DoString("print('Movement Enabled: " + Movement + "')");
            });
            Movement = false;

            HotkeysManager.Register("Useful Stuff",
            Keys.NumPad3,
            ModifierKeys.Control,
            o =>
            {
                UsefulStuff = !UsefulStuff;
                Logging.Write("Useful Stuff enabled: " + UsefulStuff);
                Lua.DoString("print('Useful Stuff Enabled: " + UsefulStuff + "')");
            });
            UsefulStuff = false;

            HotkeysManager.Register("AOE",
            Keys.NumPad4,
            ModifierKeys.Control,
            o =>
            {
                Aoe = !Aoe;
                Logging.Write("AOE enabled: " + Aoe);
                Lua.DoString("print('AOE Enabled: " + Aoe + "')");
            });
            Aoe = true;

            HotkeysManager.Register("Boss Mechs",
            Keys.NumPad5,
            ModifierKeys.Control,
            o =>
            {
                BossMechs = !BossMechs;
                Logging.Write("Boss Mechs enabled: " + BossMechs);
                Lua.DoString("print('Boss Mechs Enabled: " + BossMechs + "')");
            });
            BossMechs = false;

            HotkeysManager.Register("Weave",
            Keys.NumPad6,
            ModifierKeys.Control,
            o =>
            {
                Weave = !Weave;
                Logging.Write("Weave enabled: " + Weave);
                Lua.DoString("print('Weave Enabled: " + Weave + "')");
            });
            Weave = true;

            HotkeysManager.Register("LFR Mode",
            Keys.NumPad7,
            ModifierKeys.Control,
            o =>
            {
                LFRMode = !LFRMode;
                Logging.Write("LFR Mode enabled: " + LFRMode);
                Lua.DoString("print('LFR Mode Enabled: " + LFRMode + "')");
            });
            LFRMode = false;

        }

    }
}
