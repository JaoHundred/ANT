using ANT.Core;
using ANT.Interfaces;
using ANT.Model;
using ANT.Modules;
using JikanDotNet;
using MvvmHelpers;
using System;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ANT
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }

        public static SettingsPreferences SettingsPreferences;
        public static IJikan Jikan { get; private set; }
        protected async override void OnStart()
        {
            var settings = await JsonStorage.ReadSettingsAsync(StorageConsts.LocalAppDataFolder, StorageConsts.SettingsFileName);
            SettingsPreferences = settings ?? new SettingsPreferences();

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
