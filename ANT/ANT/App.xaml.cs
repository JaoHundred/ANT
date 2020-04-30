using ANT.Core;
using ANT.Interfaces;
using ANT.Model;
using ANT.Modules;
using JikanDotNet;
using MvvmHelpers;
using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ANT
{
    public partial class App : Application
    {
        //TODO: a checagem periódica de internet pode ser feito por aqui, existe um plugin de conectividade e classes próprias do android para pegar
        //informações da rede

        public App()
        {
            InitializeComponent();

            Device.SetFlags(new[] 
            { 
                "RadioButton_Experimental", "Expander_Experimental" , //"SwipeView_Experimental"
            });
            NotificationCenter.Current.NotificationTapped += Current_NotificationTapped;

            MainPage = new AppShell();
        }

        public static SettingsPreferences SettingsPreferences;
        public static List<FavoritedAnime> FavoritedAnimes;
        public static List<FavoritedAnimeCharacter> FavoritedAnimeCharacters;
        public static List<RecentVisualized> RecentAnimes;
        public static IJikan Jikan { get; private set; }
        protected async override void OnStart()
        {
            var settingsTask = JsonStorage.ReadDataAsync<SettingsPreferences>(StorageConsts.LocalAppDataFolder, StorageConsts.SettingsFileName);
            var favoritedAnimesTask = JsonStorage.ReadDataAsync<List<FavoritedAnime>>(StorageConsts.LocalAppDataFolder, StorageConsts.FavoritedAnimesFileName);
            var favoritedAnimesCharacterTask = JsonStorage.ReadDataAsync<List<FavoritedAnimeCharacter>>
                (StorageConsts.LocalAppDataFolder, StorageConsts.FavoritedAnimesCharacterFileName);
            var recentTask = JsonStorage.ReadDataAsync<List<RecentVisualized>>(StorageConsts.LocalAppDataFolder, StorageConsts.RecentAnimesFileName);

            SettingsPreferences = await settingsTask ?? new SettingsPreferences();

            FavoritedAnimes = await favoritedAnimesTask ?? new List<FavoritedAnime>();
            FavoritedAnimeCharacters = await favoritedAnimesCharacterTask ?? new List<FavoritedAnimeCharacter>();
            RecentAnimes = await recentTask ?? new List<RecentVisualized>();

            // Handle when your app starts
            await ThemeManager.LoadThemeAsync();

            //await CultureManager.LoadCultureAsync();

            Jikan = new Jikan(useHttps: true);
        }

        private async void Current_NotificationTapped(NotificationTappedEventArgs e)
        {
            //TODO: a chamada ao clicar na notificação não funciona, descobrir o que pode ser
            int malId = int.Parse(e.Data);
            await NavigationManager.NavigateShellAsync<AnimeSpecsViewModel>(malId);
        }

        //TODO: resolução(talvez) da seleção de idioma https://forums.xamarin.com/discussion/79379/update-language-translation-on-the-fly-using-translateextension

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        /// <summary>
        /// Usado para acrescentar um tempo antes da chamada a API jikan, use para não floodar jikan com requisições
        /// </summary>
        /// <param name="seconds">usado para ajustar em segundos o tempo do delay, não usar tempo menor que 2 segundos</param>
        /// <returns></returns>
        public static Task DelayRequest(int seconds = 2)
        {
            if (seconds < 2)
                throw new Exception("Usar tempo menor que 2 vai prejudicar a API jikan e o MAL");

            return Task.Delay(TimeSpan.FromSeconds(seconds));
        }

    }
}
