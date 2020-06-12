using ANT.Themes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using ANT.Model;

namespace ANT.Core
{
    public class ThemeManager
    {


        /// <summary>
        /// Defines the supported themes for the sample app
        /// </summary>
        public enum Themes
        {
            Light,
            Dark,
        }

        /// <summary>
        /// Changes the theme of the app.
        /// Add additional switch cases for more themes you add to the app.
        /// This also updates the local key storage value for the selected theme.
        /// </summary>
        /// <param name="theme"></param>
        public static Task ChangeThemeAsync(Themes theme)
        {
            return Task.Run(async () =>
            {
                var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
                if (mergedDictionaries != null)
                {
                    int themeId = (int)theme;

                    RemoveCurrentTheme(mergedDictionaries);

                    await UpdateSelectedThemeAsync(themeId);

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        switch (theme)
                        {
                            case Themes.Light:
                                mergedDictionaries.Add(new LightTheme());
                                break;
                            case Themes.Dark:
                                mergedDictionaries.Add(new DarkTheme());
                                break;
                            default:
                                mergedDictionaries.Add(new LightTheme());
                                break;
                        }
                    });

                    var bdCol = App.liteDB.GetCollection<SettingsPreferences>();
                    var settings = bdCol.FindById(0);
                    settings.SelectedThemeIndex = themeId;
                }
            });
        }

        private static void RemoveCurrentTheme(ICollection<ResourceDictionary> mergedDictionaries)
        {
            ResourceDictionary currentTheme = null;

            foreach (var item in mergedDictionaries)
            {
                if (!(item is ANT.Icons.FontIcons))
                {
                    currentTheme = item;
                    break;
                }
            }

            mergedDictionaries?.Remove(currentTheme);
        }

        /// <summary>
        /// Reads current theme id from the local storage and loads it.
        /// </summary>
        public static Task LoadThemeAsync()
        {
            return Task.Run(async () =>
           {
               Themes currentTheme = await CurrentThemeOrCreateAsync();
               await ChangeThemeAsync(currentTheme);
           });
        }

        public static ResourceDictionary GetCurrentTheme()
        {
            var dics = Application.Current.Resources.MergedDictionaries;
            return dics.First(p => p is LightTheme || p is DarkTheme);
        }

        /// <summary>
        /// Gives current/last selected theme from the local storage.
        /// </summary>
        /// <returns></returns>

        //TODO:implementar os 2 abaixo quando estiver funcionando o json
        private static Task<Themes> CurrentThemeOrCreateAsync()
        {
            var bdCol = App.liteDB.GetCollection<SettingsPreferences>();
            var settings = bdCol.FindById(0);

            if (settings == null)
                settings = new SettingsPreferences();

            var theme = (Themes)settings.SelectedThemeIndex;
            bdCol.Upsert(0, settings);

            return Task.FromResult(theme);
        }

        private static Task UpdateSelectedThemeAsync(int themeId)
        {
            var bdCol = App.liteDB.GetCollection<SettingsPreferences>();
            var settings = bdCol.FindById(0);


            settings.SelectedThemeIndex = themeId;

            return Task.FromResult(bdCol.Upsert(0, settings));
        }
    }
}
