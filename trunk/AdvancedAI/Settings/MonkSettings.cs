using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAI.Settings
{
    class MonkSettings : Styx.Helpers.Settings
    {
        public MonkSettings() :  base(Path.Combine(GeneralSettings.AdvancedAISettingsPath, "Monk.xml")) {}
    }
}
