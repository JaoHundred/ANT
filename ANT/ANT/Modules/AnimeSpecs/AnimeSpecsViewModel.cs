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
            AnimeEpisodes epi = await App.Jikan.GetAnimeEpisodes(_animeRef.MalId);
            epi.RequestCached = true;

            AnimeContext = _animeRef;
            Episodes = epi.EpisodeCollection.ToList();

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

        //TODO: testar com tabbed page, uma página para informações e outra para os episódios, desabilitar o uso do hamburger menu na tabbed page
    }
}
