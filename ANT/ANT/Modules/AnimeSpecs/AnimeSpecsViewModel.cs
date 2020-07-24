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
            OpenNewsCommand = new magno.AsyncCommand(OnLinkNews);
            OpenAnimeCommand = new magno.AsyncCommand(OnOpenAnime);
            BackButtonCommand = new magno.AsyncCommand<BackButtonOriginEnum>(OnBackButton);
            OpenAnimeCharacterCommand = new magno.AsyncCommand(OnOpenAnimeCharacter);
            GenreSearchCommand = new magno.AsyncCommand<string>(OnGenreSearch);
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

                _favoritedAnime = App.liteDB.GetCollection<FavoritedAnime>().FindOne(p => p.Anime.MalId == id);


                if (_favoritedAnime == null)
                {
                    await App.DelayRequest();
                    var anime = await App.Jikan.GetAnime(id);
                    anime.RequestCached = true;

                    _favoritedAnime = new FavoritedAnime(anime);
                }

                AnimeGenres = _favoritedAnime.Anime.Genres.ToList();

                IsLoadingNews = true;
                await App.DelayRequest(2);
                var loadAnimeNewsTask = Task.Run(async () =>
                {

                    if (_cancellationToken.IsCancellationRequested)
                        _cancellationToken.Token.ThrowIfCancellationRequested();

                    AnimeNews animeNews = await App.Jikan.GetAnimeNews(_favoritedAnime.Anime.MalId);

                    News = animeNews.News.ToList();
                }, _cancellationToken.Token);

                await AddOrUpdateRecentAnimeAsync(_favoritedAnime);
                AnimeContext = _favoritedAnime;
                CanEnable = true;
                IsLoading = false;

                IsLoadingCharacters = true;
                await App.DelayRequest(2);
                var loadCharactersTask = Task.Run(async () =>
                {

                    if (_cancellationToken.IsCancellationRequested)
                        _cancellationToken.Token.ThrowIfCancellationRequested();

                    AnimeCharactersStaff animeCharactersStaff = await App.Jikan.GetAnimeCharactersStaff(_favoritedAnime.Anime.MalId);
                    Characters = animeCharactersStaff.Characters.ToList();
                }, _cancellationToken.Token);

                IsLoadingRelated = true;
                var relatedAnimeTask = Task.Run(() =>
                {
                    if (_cancellationToken.IsCancellationRequested)
                        _cancellationToken.Token.ThrowIfCancellationRequested();

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

                    GroupedRelatedAnimeList = groupedRelatedAnime;
                }, _cancellationToken.Token);

                if (_favoritedAnime.Episodes == null)
                {
                    IsLoadingEpisodes = true;
                    _favoritedAnime.Episodes = await _favoritedAnime.Anime.GetAllEpisodesAsync(_cancellationToken);
                }

                Episodes = _favoritedAnime.Episodes;
                IsLoadingEpisodes = false;

                await loadCharactersTask;
                IsLoadingCharacters = false;

                await loadAnimeNewsTask;
                IsLoadingNews = false;

                await relatedAnimeTask;
                IsLoadingRelated = false;
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
                ex.SaveExceptionData();

                _cancellationToken.Cancel();
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine($"Tasks canceladas {Environment.NewLine} " +
                    $"{ex.Message}");
            }
            catch (Exception ex)
            {
                ex.SaveExceptionData();
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

        private bool _isLoadingCharecters;
        public bool IsLoadingCharacters
        {
            get { return _isLoadingCharecters; }
            set { SetProperty(ref _isLoadingCharecters, value); }
        }

        private bool _isLoadingNews;
        public bool IsLoadingNews
        {
            get { return _isLoadingNews; }
            set { SetProperty(ref _isLoadingNews, value); }
        }

        private bool _isLoadingRelated;
        public bool IsLoadingRelated
        {
            get { return _isLoadingRelated; }
            set { SetProperty(ref _isLoadingRelated, value); }
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

        private IList<CharacterEntry> _characters;
        public IList<CharacterEntry> Characters
        {
            get { return _characters; }
            set { SetProperty(ref _characters, value); }
        }

        private IList<MALSubItem> _animeGenres;
        public IList<MALSubItem> AnimeGenres
        {
            get { return _animeGenres; }
            set { SetProperty(ref _animeGenres, value); }
        }

        private IList<AnimeEpisode> _episodes;
        public IList<AnimeEpisode> Episodes
        {
            get { return _episodes; }
            set { SetProperty(ref _episodes, value); }
        }

        private IList<News> _news;
        public IList<News> News
        {
            get { return _news; }
            set { SetProperty(ref _news, value); }
        }

        private Model.RelatedAnime _selectedAnime;
        public Model.RelatedAnime SelectedAnime
        {
            get { return _selectedAnime; }
            set { SetProperty(ref _selectedAnime, value); }
        }

        private CharacterEntry _selectedCharacter;
        public CharacterEntry SelectedCharacter
        {
            get { return _selectedCharacter; }
            set { SetProperty(ref _selectedCharacter, value); }
        }

        private News _selectedNews;
        public News SelectedNews
        {
            get { return _selectedNews; }
            set { SetProperty(ref _selectedNews, value); }
        }

        private IList<GroupedRelatedAnime> _groupedRelatedAnimeList;
        public IList<GroupedRelatedAnime> GroupedRelatedAnimeList
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
            var bdCollection = App.liteDB.GetCollection<FavoritedAnime>();

            var taskResult = Task.Run(async () =>
            {
                if (bdCollection.Exists(p => p.Anime.MalId == AnimeContext.Anime.MalId))
                {
                    AnimeContext.IsFavorited = false;

                    bdCollection.Delete(AnimeContext.Anime.MalId);
                    lang = Lang.Lang.RemovedFromFavorite;
                }
                else
                {
                    AnimeContext.IsFavorited = true;
                    AnimeContext.LastUpdateDate = DateTime.Today;

                    AnimeContext.NextStreamDate = await AnimeContext.NextEpisodeDateAsync();

                    int uniqueId = 0;

                    if (bdCollection.Count() > 0)
                    {
                        uniqueId = bdCollection.Max(p => p.UniqueNotificationID);

                        if (uniqueId == int.MaxValue)
                            uniqueId = 0;
                        else if (uniqueId < int.MaxValue)
                            uniqueId += 1;
                    }

                    AnimeContext.UniqueNotificationID = uniqueId;

                    AnimeContext.CanGenerateNotifications = AnimeContext.Anime.Airing && AnimeContext.NextStreamDate != null;

                    bdCollection.Upsert(AnimeContext.Anime.MalId, AnimeContext);
                    lang = Lang.Lang.AddedToFavorite;
                }

                if (OtherViewModelFunc != null)
                    //atualiza a coleção observável de CatalogueViewModel
                    await OtherViewModelFunc.Invoke();
            });


            await taskResult;
            DependencyService.Get<IToast>().MakeToastMessageShort(lang);

#if DEBUG
            Console.WriteLine("Animes favoritados no momento");
            foreach (var anime in bdCollection.FindAll())
            {
                Console.WriteLine(anime.Anime.Title);
            }
#endif
        }

        public ICommand OpenNewsCommand { get; private set; }
        private async Task OnLinkNews()
        {
            if (IsNotBusy && SelectedNews != null)
            {
                IsBusy = true;
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                await Launcher.TryOpenAsync(SelectedNews.Url);
                IsBusy = false;
            }

            SelectedNews = null;
        }
        public ICommand OpenLinkCommand { get; private set; }
        private async Task OnLink(string link)
        {
            await Launcher.TryOpenAsync(link);
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

        public ICommand OpenAnimeCharacterCommand { get; private set; }
        private async Task OnOpenAnimeCharacter()
        {
            if (IsNotBusy && SelectedCharacter != null)
            {
                IsBusy = true;
                await Task.Delay(TimeSpan.FromMilliseconds(200));
                await NavigationManager.NavigateShellAsync<AnimeCharacterViewModel>(SelectedCharacter.MalId);
                IsBusy = false;
                SelectedCharacter = null;
            }
        }

        public ICommand BackButtonCommand { get; private set; }
        private async Task OnBackButton(BackButtonOriginEnum origin)
        {
            _cancellationToken.Cancel();

            if (origin == BackButtonOriginEnum.NavigationBar)
                await NavigationManager.PopShellPageAsync();
        }

        public ICommand GenreSearchCommand { get; private set; }
        public async Task OnGenreSearch(string genreName)
        {
            if (IsNotBusy)
            {
                IsBusy = true;
                string formatedString = genreName.RemoveOcurrencesFromString(new char[] { '-', ' ' });
                GenreSearch genre = (GenreSearch)Enum.Parse(typeof(GenreSearch), formatedString, true);

                await NavigationManager.NavigateShellAsync<CatalogueViewModel>(genre);

                IsBusy = false;
            }
        }
        #endregion

        #region métodos VM
        private Task AddOrUpdateRecentAnimeAsync(FavoritedAnime recentFavoritedAnime)
        {
            return Task.Run(() =>
            {
                if (_cancellationToken.IsCancellationRequested)
                    _cancellationToken.Token.ThrowIfCancellationRequested();

                var recentCollection = App.liteDB.GetCollection<RecentVisualized>();
                var favoritedSubEntry = recentCollection.FindOne(p => p.FavoritedAnime.Anime.MalId == recentFavoritedAnime.Anime.MalId);

                if (favoritedSubEntry != null)
                    recentCollection.Upsert(recentFavoritedAnime.Anime.MalId, new RecentVisualized(recentFavoritedAnime));
                else
                {
                    if (recentCollection.Count() == 10)
                    {
                        var settings = App.liteDB.GetCollection<SettingsPreferences>().FindById(0);
                        DateTimeOffset mostAntiqueDate = default;

                        var minDate = recentCollection.Min(p => p.Date);
                        var nsfwCollection = recentCollection.Find(p => p.FavoritedAnime.IsNSFW);

                        if (settings.ShowNSFW)
                            mostAntiqueDate = minDate;
                        else
                        {
                            if (nsfwCollection != null && nsfwCollection.Count() > 0)
                                mostAntiqueDate = nsfwCollection.Min(p => p.Date);

                            else if (nsfwCollection == null || nsfwCollection.Count() == 0)
                                mostAntiqueDate = minDate;
                        }

                        RecentVisualized mostAntiqueVisualized = recentCollection.FindOne(p => p.Date == mostAntiqueDate);

                        recentCollection.Delete(mostAntiqueVisualized.FavoritedAnime.Anime.MalId);
                        recentCollection.Upsert(recentFavoritedAnime.Anime.MalId, new RecentVisualized(recentFavoritedAnime));

                    }
                    else if (recentCollection.Count() < 10)
                        recentCollection.Upsert(recentFavoritedAnime.Anime.MalId, new RecentVisualized(recentFavoritedAnime));
                }

            }, _cancellationToken.Token);
        }
        #endregion

        //TODO:descobrir como tirar a sombra/linha do navigation bar para esta página(deixar pro futuro)
        //TODO: trocar de idioma via configurações do android nesta página resulta em uma exception de fragment, provavel de estar relacionado ao tabbedpage
    }
}
