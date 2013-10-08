using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAI.Settings
{
    class HotkeySettings : Styx.Helpers.Settings
    {
        public HotkeySettings() :  base(Path.Combine(GeneralSettings.AdvancedAISettingsPath, "Hotkey.xml")) {}
    }
}
