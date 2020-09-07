using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ANT.Model;
using Shiny;
using Shiny.Jobs;
using Shiny.Notifications;

namespace ANT.Core
{
    public class NotificationJob : IJob
    {

        public NotificationJob(INotificationManager notificationManager)
        {
            _notificationManager = notificationManager;
        }

        private INotificationManager _notificationManager;

        public async Task<bool> Run(JobInfo jobInfo, CancellationToken cancelToken)
        {
            //TODO: fazer testes reais de notificação(com app no background e force closed)
            if (App.liteDB == null)
                App.StartLiteDB();

            var settings = App.StartSettings();
            
            bool canRun = false;

            if (jobInfo.LastRunUtc == null)
            {
                var initialDelay = InitialDelay(settings.HourToNotify);

                if (initialDelay >= TimeSpan.FromMinutes(15))
                    jobInfo.PeriodicTime = initialDelay;
                else
                    jobInfo.PeriodicTime = initialDelay.Add(TimeSpan.FromMinutes(15));

                canRun = false;
            }

            //tem pelo menos o tempo especificado decorrido no HourToNotify
            else if (DateTime.Now >= jobInfo.LastRunUtc?.ToLocalTime().Add(jobInfo.PeriodicTime.Value))
            {
                jobInfo.PeriodicTime = TimeSpan.FromDays(1);
                canRun = true;
            }

            if(canRun)
            {
                await Task.Run(() =>
                {
                    if (App.liteDB == null)
                        App.StartLiteDB();

                    var animes = App.liteDB.GetCollection<FavoritedAnime>()
                        .Find(p => p.CanGenerateNotifications && p.NextStreamDate != null && p.Anime.Airing);

                    foreach (var anime in animes)
                    {
                        DateTime nextStream = anime.NextStreamDate.Value;

                        if (nextStream.DayOfWeek == DateTime.Now.DayOfWeek)
                        {
                            Console.WriteLine($"Disparou em {DateTime.Now}");

                            jobInfo.SetParameter(anime.Anime.MalId.ToString(), anime);

                            CreateNotification(anime, Consts.NotificationChannelTodayAnime);
                        }
                    }
                });
            }
            return false;
        }

        private TimeSpan InitialDelay(TimeSpan triggerAt)
        {
            TimeSpan duration;
            
            if (DateTime.Now.TimeOfDay == triggerAt)
                duration = TimeSpan.FromMinutes(15);
            
            else if (DateTime.Now.TimeOfDay > triggerAt)
                duration = (DateTime.Now.TimeOfDay - triggerAt).Add(TimeSpan.FromDays(1)).Duration();
            else
                duration = (DateTime.Now.TimeOfDay - triggerAt).Duration();

            return duration;
        }

        private void CreateNotification(FavoritedAnime favoritedAnime, string notificationChannelId)
        {
            if (!(favoritedAnime.Anime.Airing && IsTVSeries(favoritedAnime)))
                return;

            var notification = new Notification
            {
                Android = new AndroidOptions
                {
                    ChannelId = notificationChannelId,
                    Channel = "Today Animes",
                    ChannelDescription = "General",
                    AutoCancel = true,
                },

                Id = favoritedAnime.UniqueNotificationID,
                Title = favoritedAnime.Anime.Title,
                Message = Lang.Lang.EpisodeToday,
                Sound = NotificationSound.None,
            };

            _notificationManager.Send(notification);
        }

        private static bool IsTVSeries(FavoritedAnime favoritedAnime)
        {
            return favoritedAnime.Anime.Type == "TV";
        }
    }
}
