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
        public static Task CreateNotificationAsync(FavoritedAnime favoritedAnime, string notificationChannelId, DateTime? time = null)
        {
            if (time == null)
                return Task.Run(async () =>
                {
                    if (!(favoritedAnime.Anime.Airing && IsTVSeries(favoritedAnime)))
                        return;

                    DateTime? nextEpisode = await NextEpisodeDateAsync(favoritedAnime);

                    if (nextEpisode == null)
                        return;

                    favoritedAnime.NextStreamDate = nextEpisode;

                    FavoritedAnime lastFavorited = App.liteDB.GetCollection<FavoritedAnime>().FindAll()
                    .OrderBy(p => p.UniqueNotificationID).LastOrDefault();

                    if (lastFavorited == null)
                        favoritedAnime.UniqueNotificationID = 0;

                    else if (lastFavorited.UniqueNotificationID == int.MaxValue)
                        favoritedAnime.UniqueNotificationID = 0;

                    else
                        favoritedAnime.UniqueNotificationID = lastFavorited.UniqueNotificationID + 1;

                    var notification = new NotificationRequest
                    {
                        NotificationId = favoritedAnime.UniqueNotificationID,
                        Title = favoritedAnime.Anime.Title,
                        Description = Lang.Lang.EpisodeToday,
                        Repeats = NotificationRepeat.Weekly,
                        //Android = 
                        //{
                        //    IconName= "nome_da_imagem_sem_extensao"
                        //},
                        ReturningData = favoritedAnime.Anime.MalId.ToString(), // Returning data when tapped on notification.
                        NotifyTime = nextEpisode, // TODO: vai ser necessário criar uma rotina para rodar a cada X dias e verificar se data dos animes mudou
                                                  //se mudou, cancelar a notificação antiga usando o uniqueId dentro do favoritedAnime e criar uma nova, se não mudar, não fazer nada
                                                  //para a rotina, pesquisar como fazer, xamarin forms background worker ou algo assim
                    };
                    notification.Android.ChannelId = notificationChannelId;
                    NotificationCenter.Current.Show(notification);
                    favoritedAnime.HasNotificationReady = true;
                });
            else // usado para fins de debugar
                return Task.Run(async() => 
                {
                    if (!(favoritedAnime.Anime.Airing && IsTVSeries(favoritedAnime)))
                        return;

                    DateTime? nextEpisode = await NextEpisodeDateAsync(favoritedAnime);

                    if (nextEpisode == null)
                        return;

                    favoritedAnime.NextStreamDate = nextEpisode;


                    var notification = new NotificationRequest
                    {
                        NotificationId = favoritedAnime.UniqueNotificationID,
                        Title = favoritedAnime.Anime.Title,
                        Description = Lang.Lang.EpisodeToday,
                        Repeats = NotificationRepeat.Weekly,
                        //Android = 
                        //{
                        //    IconName= "nome_da_imagem_sem_extensao"
                        //},
                        ReturningData = favoritedAnime.Anime.MalId.ToString(), // Returning data when tapped on notification.
                        NotifyTime = time,
                    };
                    notification.Android.ChannelId = notificationChannelId;
                    NotificationCenter.Current.Show(notification);
                });
        }

        /// <summary>
        /// Retorna a data do próximo dia de semana que o anime irá passar, se não houver data, retorna null
        /// </summary>
        /// <param name="favoritedAnime"></param>
        /// <returns></returns>
        private static Task<DateTime?> NextEpisodeDateAsync(FavoritedAnime favoritedAnime)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrEmpty(favoritedAnime.Anime.Broadcast))
                    return null;

                var daysOfWeek = Enum.GetNames(typeof(DayOfWeek)).Select(p => new string(p.Append('s').ToArray()).ToString().ToLowerInvariant()).ToList();
                DayOfWeek? nextEpisodeDay = null;

                string[] broadCastVector = favoritedAnime.Anime.Broadcast.Split(' ');

                foreach (var day in daysOfWeek)
                {
                    string broadCastDay = broadCastVector.FirstOrDefault(p => p.ToLowerInvariant() == day);

                    if (string.IsNullOrWhiteSpace(broadCastDay))
                        continue;

                    broadCastDay = broadCastDay.ToLowerInvariant();

                    switch (broadCastDay)
                    {
                        case "sundays":
                            nextEpisodeDay = DayOfWeek.Sunday;
                            break;
                        case "mondays":
                            nextEpisodeDay = DayOfWeek.Monday;
                            break;
                        case "tuesdays":
                            nextEpisodeDay = DayOfWeek.Tuesday;
                            break;
                        case "wednesdays":
                            nextEpisodeDay = DayOfWeek.Wednesday;
                            break;
                        case "thursdays":
                            nextEpisodeDay = DayOfWeek.Thursday;
                            break;
                        case "fridays":
                            nextEpisodeDay = DayOfWeek.Friday;
                            break;
                        case "saturdays":
                            nextEpisodeDay = DayOfWeek.Saturday;
                            break;
                    }
                }

                if (nextEpisodeDay == null)
                    return null;

                int daysToSchedule = 0;

                if (nextEpisodeDay > DateTime.Today.DayOfWeek)
                    daysToSchedule = (int)nextEpisodeDay - (int)DateTime.Today.DayOfWeek;

                else if (nextEpisodeDay <= DateTime.Today.DayOfWeek)
                    daysToSchedule = ((int)nextEpisodeDay + 7) - (int)DateTime.Today.DayOfWeek;
                // TODO: ficar de olho nessa condição, suspeito que se acontecer do dia de atualização coincidir com o mesmo dia que passa o anime, nenhuma notificação será gerada para a próxima semana

                DateTime? nextEpisodeDate = DateTime.Today.AddDays(daysToSchedule).AddHours(12);

                return nextEpisodeDate;
            });
        }


        private static bool IsTVSeries(FavoritedAnime favoritedAnime)
        {
            return favoritedAnime.Anime.Type == "TV";
        }

        public static Task CancelNotificationAsync(FavoritedAnime favoritedAnime)
        {
            return Task.Run(() =>
            {
                NotificationCenter.Current.Cancel(favoritedAnime.UniqueNotificationID);
                favoritedAnime.HasNotificationReady = false;
            });
        }
    }
}
