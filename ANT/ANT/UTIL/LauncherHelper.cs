using ANT.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ANT.UTIL
{
    public static class LauncherHelper
    {
        /// <summary>
        /// Abre o link especificado no navegador interno no app
        /// </summary>
        /// <param name="link">URL para acessar uma página web</param>
        /// <returns></returns>
        public static async Task OpenLinkAsync(string link)
        {
            var colorDictionary = ThemeManager.GetCurrentTheme();

            await Browser.OpenAsync(new Uri(link), new BrowserLaunchOptions
            {
                LaunchMode = BrowserLaunchMode.SystemPreferred,
                PreferredControlColor = (Color)colorDictionary["TextColor"],
                PreferredToolbarColor = (Color)colorDictionary["StatusBarColor"]
            });
        }

        /// <summary>
        /// Abre um app específico pela sua uri-scheme
        /// </summary>
        /// <param name="appLink">uri-scheme do app</param>
        /// <returns></returns>
        public static async Task OpenAppAsync(string appLink)
        {
            //TODO:https://github.com/JaoHundred/ANT/issues/79
            await Launcher.TryOpenAsync(appLink);
        }
    }
}
