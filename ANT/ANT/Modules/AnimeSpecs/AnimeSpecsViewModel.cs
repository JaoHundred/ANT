using JikanDotNet;
using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;
using ANT.Interfaces;
using System.Threading.Tasks;
using System.Linq;

namespace ANT.Modules
{
    public class AnimeSpecsViewModel : BaseViewModel, IAsyncInitialization
    {
        public AnimeSpecsViewModel(AnimeSubEntry anime)
        {
            _animeRef = anime;
            InitializeTask = LoadAync();

        }

        private AnimeSubEntry _animeRef;
        public Task InitializeTask { get; }
        public async Task LoadAync()
        {
            try
            {
                AnimeEpisodes episodes = await App.Jikan.GetAnimeEpisodes(_animeRef.MalId);
                episodes.RequestCached = true;

                AnimeContext = _animeRef;
                Episodes = episodes.EpisodeCollection.ToList();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

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

        //TODO:descobrir como tirar a sombra/linha do navigation bar para esta página
        //TODO: campo da pontuação por no canto superior esquerdo(ao lado da imagem de capa do anime)
        //TODO: botão de favorito no meio(campo de título/faxada do anime)
        
    }
}
