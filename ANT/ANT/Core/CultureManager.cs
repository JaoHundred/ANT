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
                //TODO: ver o que pode ser feito na parte de baixo para atualizar o idioma quando trocar de opção no picker
                switch (culture)
                {
                    default:
                    case Culture.English:

                        Lang.Lang.Culture = new CultureInfo("en-US");
                        //_service.SetLocale(new CultureInfo("en-US"));

                        break;

                    case Culture.Portuguese:

                        Lang.Lang.Culture = new CultureInfo("pt-BR");
                        //_service.SetLocale(new CultureInfo("pt-BR"));

                        break;
                }
            });
        }

        public static async Task LoadCultureAsync()
        {
            bool hasKey = App.Current.Properties.ContainsKey(StorageConsts.CultureKey);

            if (hasKey)
            {
                Culture culture = (Culture)GetCultureIndex();
                await SetCultureAsync(culture);
            }
        }

        //TODO: implementar abaixo quando estiver funcionando o json
        private static int GetCultureIndex()
        {
            return 0;
        }
    }
}
