using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ANT.Core
{
    public static class StorageConsts
    {
        public const string SettingsFileName = "settings.json";
        public static readonly string LocalAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }
}
