using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAI.Settings
{
    class PriestSettings : Styx.Helpers.Settings
    {
        public PriestSettings() :  base(Path.Combine(GeneralSettings.AdvancedAISettingsPath, "Priest.xml")) {}
    }
}
