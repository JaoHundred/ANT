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
            if (App.liteDB == null)
            {
                string newLocation = Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath;

                string fullPath = System.IO.Path.Combine(newLocation, "data");
                App.liteDB = new LiteDatabase($"Filename={fullPath}");
            }

            try
            {
                var animes = App.liteDB.GetCollection<FavoritedAnime>().Find(p => p.CanGenerateNotifications && p.NextStreamDate != null);
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
                return new Result.Failure();
            }

            return new Result.Success();
        }

    }
}