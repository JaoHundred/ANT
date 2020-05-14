using System;
using System.Collections.Generic;
using System.Text;
using JikanDotNet;
using MvvmHelpers;
using Newtonsoft.Json;
using ANT.Core;

namespace ANT.Model
{
    public class FavoritedAnime : ObservableObject
    {
        //TODO: provável de precisar implementar propertychanged em todas as propriedades dessa classe, motivo: atualizar uma tela já aberta com dados novos
        //AnimeSpecsView
        public FavoritedAnime(Anime anime, IList<AnimeEpisode> episodes)
        {
            Anime = anime;
            Episodes = episodes;
        }

        public FavoritedAnime(Anime anime)
        {
            Anime = anime;
        }

        public FavoritedAnime()
        { }

        private bool _isFavorited;
        public bool IsFavorited
        {
            get { return _isFavorited; }
            set { SetProperty(ref _isFavorited, value); }
        }

        public Anime Anime { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        public DateTime? NextStreamDate { get; set; }


        private string _notificationStatus = Lang.Lang.On;
        public string NotificationStatus
        {
            get { return _notificationStatus; }
            set { SetProperty(ref _notificationStatus, value); }
        }
        private bool _canGenerateNotifications = true;
        /// <summary>
        /// O usuário liga ou desliga a exibição de notificações para este anime, por padrão vem ligado(true)
        /// </summary>
        public bool CanGenerateNotifications
        {
            get { return _canGenerateNotifications; }
            set
            {
                //TODO: implementar aqui o que acontece quando o switch dos animes favoritos são marcados ou desmarcados
                //usar as propriedades CanGenerateNotifications e NotificationStatus
                //ligar e desligar notificações, no desligar cancela e no ligar cria uma notificação usando o NotificationManager
                //salvar a mudança em seguida
                SetProperty(ref _canGenerateNotifications, value);
            }
        }

        //TODO: abrir essa propriedade abaixo e implementar o incremento e decremento seguido de save
        public int LastEpisodeSeen { get; set; }

        public int UniqueNotificationID { get; set; }

        public bool? IsR18 { get; set; }

        public IList<AnimeEpisode> Episodes { get; set; }

        public IList<RelatedAnime> RelatedAnimes { get; set; }
    }
}
