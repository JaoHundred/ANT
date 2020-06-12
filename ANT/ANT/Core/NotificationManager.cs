using System;
using System.Collections.Generic;
using System.Text;
using Plugin.LocalNotification;
using ANT.Model;
using System.Threading.Tasks;
using System.Linq;

namespace ANT.Core
{
    public static class NotificationManager
    {
        public static Task CreateNotificationAsync(FavoritedAnime favoritedAnime, string notificationChannelId, DateTime notifityTime)
        {
            return Task.Run(() =>
            {
                if (!(favoritedAnime.Anime.Airing && IsTVSeries(favoritedAnime)))
                    return;

                if (favoritedAnime.NextStreamDate == null)
                    return;

                var notification = new NotificationRequest
                {
                    NotificationId = favoritedAnime.UniqueNotificationID,
                    Title = favoritedAnime.Anime.Title,
                    Description = Lang.Lang.EpisodeToday,
                    //Android = 
                    //{
                    //    IconName= "nome_da_imagem_sem_extensao"
                    //},
                    ReturningData = favoritedAnime.Anime.MalId.ToString(), // Returning data when tapped on notification.
                    NotifyTime = notifityTime.AddSeconds(1), 
                };
                notification.Android.ChannelId = notificationChannelId;
                NotificationCenter.Current.Show(notification);
            });
        }

        private static bool IsTVSeries(FavoritedAnime favoritedAnime)
        {
            return favoritedAnime.Anime.Type == "TV";
        }
    }
}
