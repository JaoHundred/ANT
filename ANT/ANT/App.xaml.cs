using Android.Graphics;
using ANT.Core;
using ANT.Interfaces;
using ANT.Model;
using ANT.Modules;
using JikanDotNet;
using LiteDB;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Resources;
using Shiny;
using Shiny.Jobs;
using Shiny.Notifications;
using Microsoft.Extensions.DependencyInjection;
using ANT.ShinyStart;

namespace ANT
{
    public partial class App : Application
    {
        //TODO: a checagem periódica de internet pode ser feito por aqui, existe um plugin de conectividade e classes próprias do android para pegar
        //informações da rede

        public App()
        {
            InitializeComponent();

            Xamarin.Essentials.VersionTracking.Track();

            Device.SetFlags(new[]
            {
                "RadioButton_Experimental", "Expander_Experimental" , //"SwipeView_Experimental"
            });

            MainPage = new AppShell();
        }

        public static LiteDatabase liteDB;
        public static LiteDatabase liteErrorLogDB;
        public static IJikan Jikan { get; private set; }
        protected async override void OnStart()
        {
            LiteDBHelper.MigrateDatabase();

            if (liteDB == null)
                LiteDBHelper.StartLiteDB();

            if (liteErrorLogDB == null)
                LiteDBHelper.StartErrorLogLiteDB();

            // Handle when your app starts
            await ThemeManager.LoadThemeAsync();

            Jikan = new Jikan(useHttps: true);

            SettingsPreferences settings = StartSettings();

            if (settings.NotificationsIsOn)
            {
                if (Device.RuntimePlatform == Device.Android)
                    await RunJobAsync(typeof(NotificationJob), WorkManagerConsts.AnimesNotificationWorkId);
            }


            //TODO: repetir o mesmo procedimento acima para essa parte, para o work de atualização de animes na lista de favoritos
            //(repetir também no BootBroadcastReceiver)
        }

        /// <summary>
        /// Verifica se o job já existe criando um se não houver e roda em seguida
        /// </summary>
        /// <param name="jobType">tipo que herda de Shiny.IJob</param>
        /// <returns></returns>
        public static async Task RunJobAsync(Type jobType, string jobIdentifier, bool repeat = true)
        {
            var jobManager = ShinyHost.Resolve<IJobManager>();

            var job = await jobManager.GetJob(jobIdentifier);

            if (job == null)
            {
                job = new JobInfo(jobType, jobIdentifier)
                {
                    Repeat = repeat,
                };
            }

#if DEBUG
            foreach (var item in await ShinyHost.Resolve<IJobManager>().GetJobs())
            {
                Debug.WriteLine($"Job Name: {item.Identifier}");
            }
#endif

            await ShinyHost.Resolve<IJobManager>().Schedule(job);

        }

        /// <summary>
        /// Cancela um job através de seu identificador
        /// </summary>
        /// <param name="jobIdentifier">Nome do job para se cancelar</param>
        /// <returns></returns>
        public static async Task CancelJobAsync(string jobIdentifier)
        {
            var jobManager = ShinyHost.Resolve<IJobManager>();

            var job = await jobManager.GetJob(jobIdentifier);

            if (job != null)
                await jobManager.Cancel(job.Identifier);
        }

        /// <summary>
        /// Usar após garantir que o LiteDB já foi iniciado
        /// </summary>
        /// <returns></returns>
        public static SettingsPreferences StartSettings()
        {
            var bd = liteDB.GetCollection<SettingsPreferences>();
            var settings = bd.FindById(0);

            if (settings == null)
            {
                settings = new SettingsPreferences();
                liteDB.GetCollection<SettingsPreferences>().Upsert(0, settings);
            }

            return settings;
        }

        /// <summary>
        /// Método para iniciar o LiteDB
        /// </summary>
      

        //TODO:se o shiny não precisar do código comentado abaixo, deletar
        //private async void Current_NotificationTapped(NotificationTappedEventArgs e)
        //{
        //    while (liteDB == null)
        //        await Task.Delay(TimeSpan.FromMilliseconds(100));

        //    int malId = int.Parse(e.Data);
        //    await NavigationManager.NavigateShellAsync<AnimeSpecsViewModel>(malId);
        //}

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
