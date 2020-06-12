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

namespace ANT.Droid.Scheduler
{
    [Service(Permission = PermissionBind)]
    public class NotificationSheduler : Android.App.Job.JobService
    {
        public override bool OnStartJob(JobParameters @params)
        {

            if (App.liteDB == null)
            {
                string newLocation = Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath;

                string fullPath = System.IO.Path.Combine(newLocation, "data");
                App.liteDB = new LiteDatabase($"Filename={fullPath}");
            }

            try
            {
                Task.Run(async () =>
                   {
                       var animes = App.liteDB.GetCollection<FavoritedAnime>().FindAll();

                       var animesToNotify = animes.Where(p =>
                              p.NextStreamDate?.DayOfWeek == DateTime.Now.DayOfWeek &&
                              p.CanGenerateNotifications);

                       foreach (var anime in animesToNotify)
                           await ANT.Core.NotificationManager.CreateNotificationAsync(anime, Consts.NotificationChannelTodayAnime, DateTime.Now);
                   });
            }
            catch (Exception ex)
            {
                //TODO: colocar futuramente um meio para armazenar os logs de erro pelo litedb
                throw;
            }

            return true;
        }

        public override bool OnStopJob(JobParameters @params)
        {
            //TODO: ler todo acima
            return false;
        }

        //https://debruyn.dev/2018/tips-for-developing-android-jobscheduler-jobs/
    }
}