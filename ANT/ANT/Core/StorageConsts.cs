using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ANT.Core
{
    public static class StorageConsts
    {
        public const string SettingsFileName = "settings";
        public const string FavoritedAnimesFileName = "favorited_animes";
        public const string FavoritedAnimesCharacterFileName = "favorited_characters";
        public const string RecentAnimesFileName = "recent_animes";
        public static readonly string LocalAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }
}
