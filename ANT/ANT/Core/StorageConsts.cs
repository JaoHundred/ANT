using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ANT.Core
{
    public static class StorageConsts
    {
        //public const string AutomaticTranslateKey = "AutomaticTranslate";
        //public const string ThemeKey = "SelectedTheme";
        //public const string CultureKey = "CULTURE";
        public const string SettingsFileName = "settings.json";

        public static readonly string LocalAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }
}
