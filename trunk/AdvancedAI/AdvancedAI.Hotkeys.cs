using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Styx.Common;
using Styx.WoWInternals;

namespace AdvancedAI
{
    partial class AdvancedAI
    {
        public static bool InterruptsEnabled { get; set; }
        public static bool PvPRot { get; set; }
        public static bool Burst { get; set; }
        public static bool HexFocus { get; set; }
        public static bool Movement { get; set; }
        public static bool TierBonus { get; set; }
        public static bool Aoe { get; set; }
        public static bool BossMechs { get; set; }
        public static bool FistWeave { get; set; }
        public static bool Dispell { get; set; }

        protected virtual void UnregisterHotkeys()
        {
            HotkeysManager.Unregister("Toggle Interrupt");
            HotkeysManager.Unregister("PvP Toggle");
            HotkeysManager.Unregister("Burst");
            HotkeysManager.Unregister("Hex Focus");
            HotkeysManager.Unregister("Movement");
            HotkeysManager.Unregister("Tier Bonus");
            HotkeysManager.Unregister("AOE");
            HotkeysManager.Unregister("Boss Mechs");
            HotkeysManager.Unregister("Fist Weave");
            HotkeysManager.Unregister("Dispelling");
        }
        protected virtual void RegisterHotkeys()
        {
            HotkeysManager.Register("Dispelling",
                Keys.D,
                ModifierKeys.Alt,
                o =>
                {
                    Dispell = !Dispell;
                    Logging.Write("Dispelling enabled: " + Dispell);
                });
            Dispell = true;

            HotkeysManager.Register("Toggle Interupt",
                Keys.NumPad1,
                ModifierKeys.Alt,
                o =>
                {
                    InterruptsEnabled = !InterruptsEnabled;
                    Logging.Write("Interrupts enabled: " + InterruptsEnabled);
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

            HotkeysManager.Register("Burst",
            Keys.NumPad1,
            ModifierKeys.Control,
            o =>
            {
                Burst = !Burst;
                Logging.Write("Burst enabled: " + Burst);
                Lua.DoString("print('Burst Enabled: " + Burst + "')");
            });
            Burst = false;

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

            HotkeysManager.Register("Tier Bonus",
            Keys.NumPad3,
            ModifierKeys.Control,
            o =>
            {
                TierBonus = !TierBonus;
                Logging.Write("Tier Bonus enabled: " + TierBonus);
                Lua.DoString("print('Tier Bonus Enabled: " + TierBonus + "')");
            });
            TierBonus = false;

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

            HotkeysManager.Register("Fist Weave",
            Keys.NumPad6,
            ModifierKeys.Control,
            o =>
            {
                FistWeave = !FistWeave;
                Logging.Write("Fist Weave enabled: " + FistWeave);
                Lua.DoString("print('Fist Weave Enabled: " + FistWeave + "')");
            });
            FistWeave = true;
        }

    }
}
