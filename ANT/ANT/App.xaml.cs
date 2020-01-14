using ANT.Core;
using ANT.Interfaces;
using ANT.Model;
using ANT.Modules;
using JikanDotNet;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            MainPage = new AppShell();
        }

        public static SettingsPreferences SettingsPreferences;
        public static List<FavoritedAnime> FavoritedAnimes;
        public static List<FavoritedAnimeSubEntry> FavoritedSubEntryAnimes;
        public static IJikan Jikan { get; private set; }
        protected async override void OnStart()
        {
            var settingsTask = JsonStorage.ReadDataAsync<SettingsPreferences>(StorageConsts.LocalAppDataFolder, StorageConsts.SettingsFileName);
            var favoritedAnimesTask = JsonStorage.ReadDataAsync<List<FavoritedAnime>>(StorageConsts.LocalAppDataFolder, StorageConsts.FavoritedAnimesFileName);
            var favoritedSubEntryAnimesTask = 
                JsonStorage.ReadDataAsync<List<FavoritedAnimeSubEntry>>(StorageConsts.LocalAppDataFolder, StorageConsts.FavoritedAnimesFileName);

            SettingsPreferences = await settingsTask ?? new SettingsPreferences();
            FavoritedAnimes = await favoritedAnimesTask ?? new List<FavoritedAnime>();
            FavoritedSubEntryAnimes = await favoritedSubEntryAnimesTask ?? new List<FavoritedAnimeSubEntry>();

            // Handle when your app starts
            await ThemeManager.LoadThemeAsync();
            //await CultureManager.LoadCultureAsync();

            Jikan = new Jikan(useHttps: true);
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

    }
}
