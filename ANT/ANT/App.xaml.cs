using Android.Graphics;
using ANT.Core;
using ANT.Interfaces;
using ANT.Model;
using ANT.Modules;
using JikanDotNet;
using LiteDB;
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

            MainPage = new AppShell();
            NotificationCenter.Current.NotificationTapped += Current_NotificationTapped;
        }

        public static SettingsPreferences SettingsPreferences;
        public static LiteDatabase liteDB;
        public static IJikan Jikan { get; private set; }
        protected async override void OnStart()
        {
            var settingsTask = JsonStorage.ReadDataAsync<SettingsPreferences>(StorageConsts.LocalAppDataFolder, StorageConsts.SettingsFileName);
            SettingsPreferences = await settingsTask ?? new SettingsPreferences();

            // Handle when your app starts
            await ThemeManager.LoadThemeAsync();

            liteDB = new LiteDatabase($"Filename={System.IO.Path.Combine(StorageConsts.LocalAppDataFolder, "data")}");
            //await CultureManager.LoadCultureAsync();

            Jikan = new Jikan(useHttps: true);
        }

        private async void Current_NotificationTapped(NotificationTappedEventArgs e)
        {
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
