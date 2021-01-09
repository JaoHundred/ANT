using System;
using System.Collections.Generic;
using System.Text;
using JikanDotNet;
using MvvmHelpers;
using Newtonsoft.Json;
using ANT.Core;
using System.Linq;

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
            IsNSFW =  UTIL.AnimeExtension.HasAnySpecifiedGenresAsync(this, UTIL.AnimeExtension.FillNSFWGenres().Select(p => p.Genre).ToArray()).Result;
        }

        public FavoritedAnime(Anime anime)
        {
            Anime = anime;
            IsNSFW = UTIL.AnimeExtension.HasAnySpecifiedGenresAsync(this, UTIL.AnimeExtension.FillNSFWGenres().Select(p => p.Genre).ToArray()).Result;
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

        private DateTime? _lastUpdateDate;
        public DateTime? LastUpdateDate { get => _lastUpdateDate; set => _lastUpdateDate = value?.ToLocalTime(); }

        private DateTime? _nextStreamDate;
        /// <summary>
        /// propriedade para ser usado somente para pegar o dia da semana, se houver
        /// para usos de início e término de anime usar Anime.Aired
        /// </summary>
        public DateTime? NextStreamDate { get => _nextStreamDate; set => _nextStreamDate = value?.ToLocalTime(); }


        private string _notificationStatus;
        /// <summary>
        /// String de exibição de status causado pela propriedade CanGenerateNotifications
        /// </summary>
        public string NotificationStatus
        {
            get { return _notificationStatus; }
            private set { SetProperty(ref _notificationStatus, value); }
        }
        private bool _canGenerateNotifications;
        /// <summary>
        /// O usuário liga ou desliga a exibição de notificações para este anime, por padrão vem ligado(true)
        /// </summary>
        public bool CanGenerateNotifications
        {
            get { return _canGenerateNotifications; }
            set
            {
                NotificationStatus = value ? Lang.Lang.On : Lang.Lang.Off;
                SetProperty(ref _canGenerateNotifications, value);
            }
        }

        public int LastEpisodeSeen { get; set; }

        public int UniqueNotificationID { get; set; }

        public bool IsNSFW { get; set; }

        public bool IsArchived { get; set; }

        public IList<AnimeEpisode> Episodes { get; set; }

        public IList<RelatedAnime> RelatedAnimes { get; set; }
    }
}
