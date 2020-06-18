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
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

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

        public static LiteDatabase liteDB;
        public static IJikan Jikan { get; private set; }
        protected async override void OnStart()
        {
            if (liteDB == null)
            {
                StartLiteDB();
            }

            // Handle when your app starts
            await ThemeManager.LoadThemeAsync();

            Jikan = new Jikan(useHttps: true);

            var bd = liteDB.GetCollection<SettingsPreferences>();
            var settings = bd.FindById(0);

            if (settings == null)
            {
                var settingPrefs = new SettingsPreferences();
                liteDB.GetCollection<SettingsPreferences>().Upsert(0, settingPrefs);
            }

            if (settings.NotificationsIsOn)
            {
                if (Device.RuntimePlatform == Device.Android)
                {
                    await Task.Run(() =>
                    {
                        DependencyService.Get<IWork>().CreateWorkAndKeep(WorkManagerConsts.AnimesNotificationWorkId, TimeSpan.FromDays(1));
                    });
                }
            }

            //TODO: repetir o mesmo procedimento acima para essa parte, para o work de atualização de animes na lista de favoritos
            //(repetir também no BootBroadcastReceiver)
        }

        /// <summary>
        /// Método para iniciar o LiteDB
        /// </summary>
        public static void StartLiteDB()
        {
            string newLocation = DependencyService.Get<IGetFolder>().GetApplicationDocumentsFolder();

            string fullPath = System.IO.Path.Combine(newLocation, "data");
            liteDB = new LiteDatabase($"Filename={fullPath}");
            //await CultureManager.LoadCultureAsync();
        }

        private async void Current_NotificationTapped(NotificationTappedEventArgs e)
        {
            while (liteDB == null)
                await Task.Delay(TimeSpan.FromMilliseconds(100));

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
