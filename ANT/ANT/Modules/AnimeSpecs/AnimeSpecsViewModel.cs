using JikanDotNet;
using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;
using ANT.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace ANT.Modules
{
    public class AnimeSpecsViewModel : BaseViewModel, IAsyncInitialization
    {
        public AnimeSpecsViewModel(AnimeSubEntry anime)
        {
            InitializeTask = LoadAync(anime);

        }

        private AnimeSubEntry _animeRef;
        public Task InitializeTask { get; }
        public async Task LoadAync(object anime)
        {
            _animeRef = (AnimeSubEntry) anime;
            IsLoading = true;
            try
            {
                
                AnimeEpisodes episodes = await App.Jikan.GetAnimeEpisodes(_animeRef.MalId);
                episodes.RequestCached = true;

                
                Episodes = episodes.EpisodeCollection.ToList();
                IsLoading = false;

                AnimeContext = _animeRef;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #region properties

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        private AnimeSubEntry _animeContext;
        public AnimeSubEntry AnimeContext
        {
            get { return _animeContext; }
            set { SetProperty(ref _animeContext, value); }
        }


        private IList<AnimeEpisode> _episodes;
        public IList<AnimeEpisode> Episodes
        {
            get { return _episodes; }
            set { SetProperty(ref _episodes, value); }
        }
        #endregion

        #region commands
        public ICommand FavoriteCommand => new Command(() => 
        {
            //TODO:implementar classe modelo e serviço de favoritar
        });
        #endregion

        //TODO:descobrir como tirar a sombra/linha do navigation bar para esta página
        //TODO: botão de favorito no meio(campo de título/faxada do anime)

    }
}
