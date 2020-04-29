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
            return Task.Run(() =>
            {
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
                    Title = "Test",
                    Description = $"Episódio novo de {favoritedAnime.Anime.Title}",
                    //Android = 
                    //{
                    //    IconName= "nome_da_imagem_sem_extensao"
                    //},
                    ReturningData = favoritedAnime.Anime.MalId.ToString(), // Returning data when tapped on notification.
                    NotifyTime = DateTime.Now.AddSeconds(1), // TODO: trocar essa data para o dia que vai acontecer o próximo stream do anime
                    //a opção de repetição pode não servir, já que tem animes que exibem irregularmente (semana tem, outra não)
                    //vai ser útil recriar sempre uma notificação para a próxima data desse anime(tratar todos da mesma forma)
                    //vai ser necessário criar uma rotina para rodar a cada X dias e verificar se data dos animes mudou
                    //se mudou, cancelar a notificação antiga usando o uniqueId dentro do favoritedAnime e criar uma nova, se não mudar, não fazer nada
                    //para a rotina, pesquisar como fazer, xamarin forms background worker ou algo assim
                };
                NotificationCenter.Current.Show(notification);
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
