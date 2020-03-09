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
using Xamarin.Essentials;
using ANT.Core;
using ANT.UTIL;
using magno = MvvmHelpers.Commands;
using ANT.Model;

namespace ANT.Modules
{
    public class AnimeSpecsViewModel : BaseVMExtender, IAsyncInitialization
    {
        public AnimeSpecsViewModel(long malId, Func<Task> func)
        {
            OtherViewModelFunc = func;
            InitializeTask = LoadAsync(malId);
            InitCommands();
        }

        public AnimeSpecsViewModel(long malId)
        {
            InitializeTask = LoadAsync(malId);
            InitCommands();
        }

        private void InitCommands()
        {
            FavoriteCommand = new magno.AsyncCommand(OnFavorite);
            OpenLinkCommand = new magno.AsyncCommand<string>(OnLink);
            CheckAnimeGenresCommand = new magno.AsyncCommand(OnCheckAnimeGenres);
            CheckAnimeCharactersCommand = new magno.AsyncCommand(OnCheckAnimeCharacters);
        }

        public Task InitializeTask { get; }
        private FavoritedAnime _favoritedAnime;
        private Func<Task> OtherViewModelFunc;
        public async Task LoadAsync(object param)
        {
            IsLoading = true;
            CanEnable = false;
            try
            {
                long id = (long)param;

                _favoritedAnime = App.FavoritedAnimes.FirstOrDefault(p => p.Anime.MalId == id);
                //TODO: criar no futuro uma rotina de checagem por atualizações dos animes alvos em favoritos(algo semelhante ao tachiyomi
                //pode acontecer todo dia, semanalmente ou até mesmo no dia específico de cada anime), a rotina é chamada como background e atualiza
                //a lista com dados novos se houver

                if (_favoritedAnime == null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(4));
                    Anime anime = await App.Jikan.GetAnime(id);
                    anime.RequestCached = true;

                    IsLoadingEpisodes = true;

                    _favoritedAnime = new FavoritedAnime(anime, await anime.GetAllEpisodesAsync());
                }

                await AddOrUpdateRecentAnimeAsync(_favoritedAnime);

                Episodes = _favoritedAnime.Episodes;
                AnimeContext = _favoritedAnime;

                IsLoading = false;
                IsLoadingEpisodes = false;
                CanEnable = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Problema encontrado em :{ex.Message}");
                DependencyService.Get<IToast>().MakeToastMessageLong(Lang.Lang.Error);
            }
            finally
            {

            }
        }

        #region properties

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        private bool _isLoadingEpisodes;
        public bool IsLoadingEpisodes
        {
            get { return _isLoadingEpisodes; }
            set { SetProperty(ref _isLoadingEpisodes, value); }
        }

        private FavoritedAnime _animeContext;
        public FavoritedAnime AnimeContext
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

        public ICommand FavoriteCommand { get; private set; }
        private async Task OnFavorite()
        {
            string lang = default;

            if (App.FavoritedAnimes.Contains(AnimeContext))
            {
                AnimeContext.IsFavorited = false;
                _favoritedAnime.IsFavorited = false;

                App.FavoritedAnimes.Remove(AnimeContext);
                lang = Lang.Lang.RemovedFromFavorite;
            }
            else
            {
                AnimeContext.IsFavorited = true;
                _favoritedAnime.IsFavorited = true;

                App.FavoritedAnimes.Add(AnimeContext);
                lang = Lang.Lang.AddedToFavorite;
            }

            if (OtherViewModelFunc != null)
                //atualiza a coleção observável de CatalogueViewModel
               await OtherViewModelFunc.Invoke();

            await JsonStorage.SaveDataAsync(App.FavoritedAnimes, StorageConsts.LocalAppDataFolder, StorageConsts.FavoritedAnimesFileName);
            DependencyService.Get<IToast>().MakeToastMessageShort(lang);

#if DEBUG
            Console.WriteLine("Animes favoritados no momento");
            foreach (var anime in App.FavoritedAnimes)
            {
                Console.WriteLine(anime.Anime.Title);
            }
#endif
        }

        public ICommand OpenLinkCommand { get; private set; }
        private async Task OnLink(string link)
        {
            await Launcher.TryOpenAsync(link);
        }


        public ICommand CheckAnimeGenresCommand { get; private set; }
        private async Task OnCheckAnimeGenres()
        {
            bool canNavigate = await NavigationManager.CanPopUpNavigateAsync<AnimeGenrePopupViewModel>();

            if (canNavigate)
                await NavigationManager.NavigatePopUpAsync<AnimeGenrePopupViewModel>(AnimeContext.Anime.Genres.ToList());
        }

        public ICommand CheckAnimeCharactersCommand { get; private set; }
        private async Task OnCheckAnimeCharacters()
        {
            bool canNavigate = await NavigationManager.CanPopUpNavigateAsync<AnimeCharacterPopupViewModel>();

            if (canNavigate)
                await NavigationManager.NavigatePopUpAsync<AnimeCharacterPopupViewModel>(AnimeContext.Anime.MalId);
        }
        #endregion

        #region métodos VM
        private Task AddOrUpdateRecentAnimeAsync(FavoritedAnime recentFavoritedAnime)
        {
            //TODO: devo precisar de um meio para atualizar a página de recentes(na hora que o usuário for voltar com o backbutton)
            return Task.Run(async () =>
            {
                var favoritedSubEntry = App.RecentAnimes.FirstOrDefault(p => p.FavoritedAnime.Anime.MalId == recentFavoritedAnime.Anime.MalId);

                if (favoritedSubEntry != null)
                    favoritedSubEntry.Date = DateTimeOffset.Now;
                else
                {
                    if (App.RecentAnimes.Count == 10)
                    {
                        DateTimeOffset mostAntiqueDate = App.RecentAnimes.Min(p => p.Date);
                        RecentVisualized mostAntiqueVisualized = App.RecentAnimes.First(p => p.Date == mostAntiqueDate);

                        App.RecentAnimes.Remove(mostAntiqueVisualized);
                        App.RecentAnimes.Add(new RecentVisualized(recentFavoritedAnime));
                    }
                    else if (App.RecentAnimes.Count < 10)
                        App.RecentAnimes.Add(new RecentVisualized(recentFavoritedAnime));
                }

                await JsonStorage.SaveDataAsync(App.RecentAnimes, StorageConsts.LocalAppDataFolder, StorageConsts.RecentAnimesFileName);
            });
        }
        #endregion

        //TODO:descobrir como tirar a sombra/linha do navigation bar para esta página(deixar pro futuro)
        //TODO: estilizar o template dos episódios, ver quais dados eu posso exibir a cerca dos episódios e ver o que mostrar
        //TODO: trocar de idioma via configurações do android nesta página resulta em uma exception de fragment, provavel de estar relacionado ao tabbedpage
    }
}
