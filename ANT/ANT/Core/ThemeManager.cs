using ANT.Themes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ANT.Core
{
    public class ThemeManager
    {

        private static readonly string _themeKey = "SelectedTheme";
        public static int _currentThemeIndex;

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
                    mergedDictionaries.Clear();

                    await UpdateSelectedThemeAsync(theme);
                    _currentThemeIndex = (int)theme;

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
                }
            });
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

        /// <summary>
        /// Gives current/last selected theme from the local storage.
        /// </summary>
        /// <returns></returns>
        private static Task<Themes> CurrentThemeOrCreateAsync()
        {
            return Task<Themes>.Run(async () =>
            {
                bool hasKey = App.Current.Properties.ContainsKey(_themeKey);

                if (hasKey)
                    return (Themes)App.Current.Properties[_themeKey];

                App.Current.Properties.Add(_themeKey, (int)Themes.Light);
                await App.Current.SavePropertiesAsync();

                return Themes.Light;
            });
        }

        private static Task UpdateSelectedThemeAsync(Themes theme)
        {
            return Task.Run(async () =>
            {

                var themeId = (int)theme;
                App.Current.Properties[_themeKey] = themeId;

                await App.Current.SavePropertiesAsync();
            });
        }
    }
}
