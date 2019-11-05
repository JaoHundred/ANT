using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ANT.Model.Settings
{
    [JsonObject]
    public class SettingsPreferences
    {
        [JsonProperty(PropertyName = "AuTT")]
        public bool AutomaticTranslate { get; set; }
        
        [JsonProperty(PropertyName = "SelecTI")]
        public int SelectedThemeIndex { get; set; }
        
        [JsonProperty(PropertyName = "SelecLI")]
        public int SelectedLanguageIndex { get; set; }
    }
}
