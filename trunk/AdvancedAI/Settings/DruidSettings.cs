using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAI.Settings
{
    class DruidSettings : Styx.Helpers.Settings
    {
        public DruidSettings() :  base(Path.Combine(GeneralSettings.AdvancedAISettingsPath, "Druid.xml")) {}
    }
}
