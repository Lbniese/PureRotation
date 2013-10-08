using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAI.Settings
{
    class HunterSettings : Styx.Helpers.Settings
    {
        public HunterSettings() :  base(Path.Combine(GeneralSettings.AdvancedAISettingsPath, "Hunter.xml")) {}
    }
}
