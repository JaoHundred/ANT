using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ANT.Core
{
    public static class StorageConsts
    {
        public const string SettingsFileName = "settings.json";
        public const string FavoritedAnimesFileName = "favorited_animes.json";
        public const string FavoritedSubEntryAnimesFileName = "favorited_Sub_animes.json";
        public static readonly string LocalAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }
}
