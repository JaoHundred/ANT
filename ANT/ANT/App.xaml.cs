using ANT.Core;
using ANT.Model;
using ANT.Modules;
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
        public static INavigation Navigation;
        protected async override void OnStart()
        {
            Navigation = MainPage.Navigation;

            var settings = await JsonStorage.ReadSettingsAsync(StorageConsts.LocalAppDataFolder, StorageConsts.SettingsFileName);
            SettingsPreferences = settings ?? new SettingsPreferences();

            // Handle when your app starts
            await ThemeManager.LoadThemeAsync();
            //await CultureManager.LoadCultureAsync();
        }

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
