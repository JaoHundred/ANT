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
using System.Threading;
using JikanDotNet.Exceptions;

namespace ANT.Modules
{
    public class AnimeSpecsViewModel : BaseVMExtender, IAsyncInitialization
    {
        public AnimeSpecsViewModel(long malId, Func<Task> func)
        {
            _cancellationToken = new CancellationTokenSource();
            OtherViewModelFunc = func;
            InitializeTask = LoadAsync(malId);
            Init();
        }

        public AnimeSpecsViewModel(long malId)
        {
            _cancellationToken = new CancellationTokenSource();
            InitializeTask = LoadAsync(malId);
            Init();
        }

        private void Init()
        {
            FavoriteCommand = new magno.AsyncCommand(OnFavorite);
            OpenLinkCommand = new magno.AsyncCommand<string>(OnLink);
            CheckAnimeGenresCommand = new magno.AsyncCommand(OnCheckAnimeGenres);
            CheckAnimeCharactersCommand = new magno.AsyncCommand(OnCheckAnimeCharacters);
            OpenAnimeCommand = new magno.AsyncCommand(OnOpenAnime);
            BackButtonCommand = new magno.AsyncCommand<BackButtonOriginEnum>(OnBackButton);
        }

        public Task InitializeTask { get; }
        private FavoritedAnime _favoritedAnime;
        private Func<Task> OtherViewModelFunc;
        private CancellationTokenSource _cancellationToken;
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
                    await App.DelayRequest();
                    var anime = await App.Jikan.GetAnime(id);
                    anime.RequestCached = true;

                    _favoritedAnime = new FavoritedAnime(anime);
                }

                await AddOrUpdateRecentAnimeAsync(_favoritedAnime);
                AnimeContext = _favoritedAnime;
                CanEnable = true;
                IsLoading = false;

                var groupedList = Task.Run(() =>
                {
                    var relatedAnimes = new List<Model.RelatedAnime>();

                    if (_favoritedAnime.Anime.Related.ParentStories != null)
                        relatedAnimes.AddRange(_favoritedAnime.Anime.Related.ParentStories
                            .ConvertMalSubItemToRelatedAnime(Lang.Lang.Parent));

                    if (_favoritedAnime.Anime.Related.Prequels != null)
                        relatedAnimes.AddRange(_favoritedAnime.Anime.Related.Prequels
                            .ConvertMalSubItemToRelatedAnime(Lang.Lang.Prequels));

                    if (_favoritedAnime.Anime.Related.Sequels != null)
                        relatedAnimes.AddRange(_favoritedAnime.Anime.Related.Sequels
                            .ConvertMalSubItemToRelatedAnime(Lang.Lang.Sequels));

                    if (_favoritedAnime.Anime.Related.SideStories != null)
                        relatedAnimes.AddRange(_favoritedAnime.Anime.Related.SideStories
                            .ConvertMalSubItemToRelatedAnime(Lang.Lang.SideStory));

                    if (_favoritedAnime.Anime.Related.SpinOffs != null)
                        relatedAnimes.AddRange(_favoritedAnime.Anime.Related.SpinOffs
                            .ConvertMalSubItemToRelatedAnime(Lang.Lang.SpinOffs));

                    if (_favoritedAnime.Anime.Related.Others != null)
                        relatedAnimes.AddRange(_favoritedAnime.Anime.Related.Others
                            .ConvertMalSubItemToRelatedAnime(Lang.Lang.Others));

                    if (_favoritedAnime.Anime.Related.AlternativeVersions != null)
                        relatedAnimes.AddRange(_favoritedAnime.Anime.Related.AlternativeVersions
                            .ConvertMalSubItemToRelatedAnime(Lang.Lang.AlternativeVersions));
                    //TODO: por aqui todos os outros dados que estão dentro de Anime.Related

                    _favoritedAnime.RelatedAnimes = relatedAnimes;

                    var groupedRelatedAnime = new List<GroupedRelatedAnime>();
                    foreach (var item in _favoritedAnime.RelatedAnimes.GroupBy(p => p.GroupName))
                        groupedRelatedAnime.Add(new GroupedRelatedAnime(item.Key, item.ToList()));

                    return groupedRelatedAnime;
                });
                GroupedRelatedAnimeList = await groupedList;

                if (_favoritedAnime.Episodes == null)
                {
                    IsLoadingEpisodes = true;
                    _favoritedAnime.Episodes = await _favoritedAnime.Anime.GetAllEpisodesAsync(_cancellationToken);
                }

                Episodes = _favoritedAnime.Episodes;
                IsLoadingEpisodes = false;

                await Task.Run(async () =>
                {
                    foreach (var group in GroupedRelatedAnimeList)
                    {
                        foreach (var relatedAnime in group)
                        {
                            foreach (var item in _favoritedAnime.RelatedAnimes)
                            {
                                if (relatedAnime.Anime.MalId == item.Anime.MalId)
                                {
                                    if (_cancellationToken.IsCancellationRequested)
                                        _cancellationToken.Token.ThrowIfCancellationRequested();

                                    await App.DelayRequest(4);
                                    var anime = await App.Jikan.GetAnime(item.Anime.MalId);

                                    relatedAnime.ImageURL = anime.ImageURL;
                                }
                            }
                        }
                    }
                }, _cancellationToken.Token);
            }
            catch (JikanRequestException ex)
            {
                Console.WriteLine($"Problema encontrado em :{ex.Message}");
                _cancellationToken.Cancel();
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine($"Tasks canceladas {Environment.NewLine} " +
                    $"{ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Problema encontrado em :{ex.Message}");
                DependencyService.Get<IToast>().MakeToastMessageLong(Lang.Lang.Error);
                _cancellationToken.Cancel();
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

        private Model.RelatedAnime _selectedAnime;
        public Model.RelatedAnime SelectedAnime
        {
            get { return _selectedAnime; }
            set { SetProperty(ref _selectedAnime, value); }
        }

        private List<GroupedRelatedAnime> _groupedRelatedAnimeList;
        public List<GroupedRelatedAnime> GroupedRelatedAnimeList
        {
            get { return _groupedRelatedAnimeList; }
            set { SetProperty(ref _groupedRelatedAnimeList, value); }
        }
        #endregion

        #region commands

        public ICommand FavoriteCommand { get; private set; }
        private async Task OnFavorite()
        {
            //TODO: o usuário pode escolher desativar pela tela de FavoritedView e AnimeSpecsView as notificações
            string lang = default;

            var taskResult = Task.Run(async () =>
            {
                if (App.FavoritedAnimes.Contains(AnimeContext))
                {
                    AnimeContext.IsFavorited = false;
                    _favoritedAnime.IsFavorited = false;
                    await NotificationManager.CancelNotificationAsync(AnimeContext);


                    App.FavoritedAnimes.Remove(AnimeContext);
                    lang = Lang.Lang.RemovedFromFavorite;
                }
                else
                {
                    AnimeContext.IsFavorited = true;
                    _favoritedAnime.IsFavorited = true;
                    AnimeContext.LastUpdateDate = DateTime.Now;

                    if (AnimeContext.CanGenerateNotifications)
                        await NotificationManager.CreateNotificationAsync(AnimeContext);

                    App.FavoritedAnimes.Add(AnimeContext);
                    lang = Lang.Lang.AddedToFavorite;
                }

                if (OtherViewModelFunc != null)
                    //atualiza a coleção observável de CatalogueViewModel
                    await OtherViewModelFunc.Invoke();
            });


            await taskResult;
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

        public ICommand OpenAnimeCommand { get; private set; }
        private async Task OnOpenAnime()
        {
            if (IsNotBusy && SelectedAnime != null)
            {
                IsBusy = true;
                await NavigationManager.NavigateShellAsync<AnimeSpecsViewModel>(SelectedAnime.Anime.MalId);
                IsBusy = false;
            }

            SelectedAnime = null;
        }

        public ICommand BackButtonCommand { get; private set; }
        private async Task OnBackButton(BackButtonOriginEnum origin)
        {
            _cancellationToken.Cancel();

            if (origin == BackButtonOriginEnum.NavigationBar)
                await NavigationManager.PopShellPageAsync();
        }
        #endregion

        #region métodos VM
        private Task AddOrUpdateRecentAnimeAsync(FavoritedAnime recentFavoritedAnime)
        {
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

                if (_cancellationToken.IsCancellationRequested)
                    _cancellationToken.Token.ThrowIfCancellationRequested();

                await JsonStorage.SaveDataAsync(App.RecentAnimes, StorageConsts.LocalAppDataFolder, StorageConsts.RecentAnimesFileName);

            }, _cancellationToken.Token);
        }
        #endregion

        //TODO:descobrir como tirar a sombra/linha do navigation bar para esta página(deixar pro futuro)
        //TODO: trocar de idioma via configurações do android nesta página resulta em uma exception de fragment, provavel de estar relacionado ao tabbedpage
    }
}
