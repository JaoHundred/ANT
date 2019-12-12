﻿using JikanDotNet;
using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;
using ANT.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Essentials;
using ANT.Helpers;

namespace ANT.Modules
{
    public class AnimeSpecsViewModel : BaseViewModel, IAsyncInitialization
    {
        public AnimeSpecsViewModel(long animeId)
        {
            InitializeTask = LoadAync(animeId);
        }

        public Task InitializeTask { get; }
        public async Task LoadAync(object animeId)
        {
            long id = (long)animeId;

            IsLoading = true;
            CanEnable = false;
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                AnimeEpisodes episodes = await App.Jikan.GetAnimeEpisodes(id);
                episodes.RequestCached = true;
                Episodes = episodes.EpisodeCollection.ToList();

                await Task.Delay(TimeSpan.FromSeconds(4));

                Anime anime = await App.Jikan.GetAnime(id);
                anime.RequestCached = true;
                AnimeContext = anime;

                IsLoading = false;
                CanEnable = true;
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

        private Anime _animeContext;
        public Anime AnimeContext
        {
            get { return _animeContext; }
            set { SetProperty(ref _animeContext, value); }
        }

        private bool _canEnable;
        public bool CanEnable
        {
            get { return _canEnable; }
            set { SetProperty(ref _canEnable, value); }
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

        public ICommand OpenImageInBrowserCommand => new Command(async () =>
        {
            await Launcher.TryOpenAsync(AnimeContext.ImageURL);
        });


        public ICommand CheckAnimeGenresCommand => new Command(async () =>
        {
            bool canNavigate = await NavigateHelper.CanPopUpNavigateAsync<AnimeGenrePopupView>();

            if (canNavigate)
                await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(new AnimeGenrePopupView(AnimeContext.Genres.ToList()));
        });


        public ICommand OpenAnimeInBrowserCommand => new Command(async () =>
        {
            await Launcher.TryOpenAsync(AnimeContext.LinkCanonical);
        });


        #endregion

        //TODO:descobrir como tirar a sombra/linha do navigation bar para esta página(deixar pro futuro)
        //TODO: estilizar os botões da parte de baixo da tela(gênero e My anime list), ver o que por e se por algo mais no espaço em branco restante
    }
}
