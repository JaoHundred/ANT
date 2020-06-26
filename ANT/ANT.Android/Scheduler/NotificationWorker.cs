using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.App.Job;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ANT.Interfaces;
using LiteDB;
using Xamarin.Forms;
using ANT.Model;
using ANT.Core;
using AndroidX.Work;
using Google.Common.Util.Concurrent;
using ANT.Droid.Helpers;

namespace ANT.Droid.Scheduler
{
    public class NotificationWorker : Worker
    {
        public NotificationWorker(Context context, WorkerParameters workerParameters) : base(context, workerParameters)
        {

        }

        //https://devblogs.microsoft.com/xamarin/getting-started-workmanager/
        public override Result DoWork()
        {
            try
            {
                if (App.liteDB == null)
                    App.StartLiteDB();

                var animes = App.liteDB.GetCollection<FavoritedAnime>()
                    .Find(p => p.CanGenerateNotifications && p.NextStreamDate != null && p.Anime.Airing);
                DateTime now = DateTime.Now;

                foreach (var anime in animes)
                {
                    DateTime nextStream = anime.NextStreamDate.Value;

                    if (nextStream.DayOfWeek == now.DayOfWeek)
                        ANT.Core.NotificationManager.CreateNotificationAsync(anime, Consts.NotificationChannelTodayAnime, DateTime.Now);
                }

            }
            catch (Exception ex)
            {
                //TODO:salvar aqui os dados do que deu errado
                return new Result.Success();
            }

            Console.WriteLine($"Disparou em {DateTime.Now}");

            return new Result.Success();
        }

    }
}