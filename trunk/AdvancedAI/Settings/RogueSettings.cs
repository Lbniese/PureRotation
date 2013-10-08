using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedAI.Settings
{
    class RogueSettings : Styx.Helpers.Settings
    {
        public RogueSettings() :  base(Path.Combine(GeneralSettings.AdvancedAISettingsPath, "Rogue.xml")) {}
    }
}
