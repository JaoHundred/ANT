using ANT.Lang;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ANT.Core
{
    public static class CultureManager
    {

        public enum Culture
        {
            English,
            Portuguese,
        };

        public static int _currentCultureIndex;
        private static ILocalize _service = DependencyService.Get<ILocalize>();

        public static Task SetCultureAsync(Culture culture)
        {
            return Task.Run(async () =>
            {
                _currentCultureIndex = (int)culture;

                switch (culture)
                {
                    default:
                    case Culture.English:

                        Lang.Lang.Culture = new CultureInfo("en-US");
                        //_service.SetLocale(new CultureInfo("en-US"));

                        App.Current.Properties.AddOrUpdate(AppPropertiesConsts.CultureKey, _currentCultureIndex);
                        await App.Current.SavePropertiesAsync();
                        break;

                    case Culture.Portuguese:

                        Lang.Lang.Culture = new CultureInfo("pt-BR");
                        //_service.SetLocale(new CultureInfo("pt-BR"));

                        App.Current.Properties.AddOrUpdate(AppPropertiesConsts.CultureKey, _currentCultureIndex);
                        await App.Current.SavePropertiesAsync();
                        break;
                }
            });
        }

        public static async Task LoadCultureAsync()
        {
            bool hasKey = App.Current.Properties.ContainsKey(AppPropertiesConsts.CultureKey);

            if (hasKey)
            {
                Culture culture = (Culture)GetCultureIndex();
                await SetCultureAsync(culture);
            }
        }

        private static int GetCultureIndex()
        {
            return (int)App.Current.Properties[AppPropertiesConsts.CultureKey];
        }
    }
}
