using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAI.Settings
{
    class WarlockSettings : Styx.Helpers.Settings
    {
        public WarlockSettings() : base(Path.Combine(GeneralSettings.AdvancedAISettingsPath, "Warlock.xml")) {}
    }
}
