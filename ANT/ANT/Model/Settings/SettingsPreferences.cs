using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;
using Java.Sql;

namespace ANT.Model
{
    public class SettingsPreferences
    {
        public SettingsPreferences()
        {
            AutomaticTranslate = true;
            NotificationsIsOn = true;
        }

        [BsonId(autoId: true)]
        public int Id { get; set; }

        public bool AutomaticTranslate { get; set; }

        public int SelectedThemeIndex { get; set; }

        public int SelectedLanguageIndex { get; set; }

        public bool NotificationsIsOn { get; set; }
        

    }
}
