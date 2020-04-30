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
        public static Task CreateNotificationAsync(FavoritedAnime favoritedAnime)
        {
            return Task.Run(async () =>
            {

                DateTime? nextEpisode = await NextEpisodeDateAsync(favoritedAnime);

                if (nextEpisode == null)
                    return;

                FavoritedAnime lastFavorited = App.FavoritedAnimes.LastOrDefault();

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
                    //Android = 
                    //{
                    //    IconName= "nome_da_imagem_sem_extensao"
                    //},
                    ReturningData = favoritedAnime.Anime.MalId.ToString(), // Returning data when tapped on notification.
                    NotifyTime = nextEpisode, // TODO: vai ser necessário criar uma rotina para rodar a cada X dias e verificar se data dos animes mudou
                    //se mudou, cancelar a notificação antiga usando o uniqueId dentro do favoritedAnime e criar uma nova, se não mudar, não fazer nada
                    //para a rotina, pesquisar como fazer, xamarin forms background worker ou algo assim
                };
                NotificationCenter.Current.Show(notification);
            });
        }

        private static Task<DateTime?> NextEpisodeDateAsync(FavoritedAnime favoritedAnime)
        {

            return Task.Run(() =>
            {
                //TODO: extrair a string do dia da semana que passa, converter o dia da semana para datetime.dayofweek, e ver qual é o próximo dia(em data) que acontece
                //esse dia da semana, esse é o resultado que precisa ser retornado, se não houver nada para retornar, retorne nulo
                var daysOfWeek = Enum.GetNames(typeof(DayOfWeek)).Select(p => new string(p.Append('s').ToArray()).ToString().ToLowerInvariant()).ToList();
                DayOfWeek? nextEpisodeDay = null;

                foreach (var day in daysOfWeek)
                {
                    string broadCastDay = favoritedAnime.Anime.Broadcast.Split(' ').FirstOrDefault(p => p.ToLowerInvariant() == day);

                    if (!string.IsNullOrWhiteSpace(broadCastDay))
                    {
                        broadCastDay = broadCastDay.ToLowerInvariant();
                        switch (day)
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
                }

                if (nextEpisodeDay == null)
                    return null;

                int daysToSchedule = ((int)nextEpisodeDay + 7) - (int)DateTime.Today.DayOfWeek;
                DateTime? nextEpisodeDate = DateTime.Today.AddDays(daysToSchedule).AddHours(12);
                //TODO: testar aqui e vê se a matématica está dando certo(parece que está certo, ficar de olho)
                return nextEpisodeDate;
            });
        }

        public static Task CancelNotificationAsync(FavoritedAnime favoritedAnime)
        {
            return Task.Run(() =>
            {
                NotificationCenter.Current.Cancel(favoritedAnime.UniqueNotificationID);
            });
        }

        //TODO: só deve ser criado notificação para animes que estão em andamento, não criar para os que já estão terminados
        //TODO:preencher aqui a StreamData, ela via vir via o serviço de notificação, criar método para isso
    }
}
