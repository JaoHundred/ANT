using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Text;
using ANT.Interfaces;
using System.Threading.Tasks;
using JikanDotNet;
using System.Linq;
using System.Threading;
using ANT.Model;
using Xamarin.Forms;
using System.Windows.Input;
using magno = MvvmHelpers.Commands;
using ANT.Core;
using ANT.UTIL;
using LiteDB;

namespace ANT.Modules
{
    public class HomeViewModel : BaseViewModel
    {
        public HomeViewModel()
        {
            SelectedItemCommand = new magno.AsyncCommand(OnSelectedItem);
        }

        public Task InitializeTask { get; }
        private CancellationTokenSource _cancellationTokenSource;

        public void Unload()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        public async Task LoadAsync()
        {
            while (App.liteDB == null)
                await Task.Delay(TimeSpan.FromMilliseconds(100));

            _cancellationTokenSource = new CancellationTokenSource();

            var settings = App.liteDB.GetCollection<SettingsPreferences>().FindById(0);

            try
            {
                var loadTodayAnimesTask = Task.Run(async () =>
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    var todayAnimeCollection = App.liteDB.GetCollection<TodayAnimes>();
                    var todayAnimes = todayAnimeCollection.FindById(0);

                    if (todayAnimes?.DayOfWeek != DateTime.Today.DayOfWeek || todayAnimes?.ShowNSFW != settings.ShowNSFW)
                    {
                        IsLoadingTodayAnimes = true;

                        await App.DelayRequest(2);
                        Schedule schedule = await App.Jikan.GetSchedule(DateTime.Today.DayOfWeek.ConvertDayOfWeekToScheduleDay());

                        var animes = await schedule.GetCurrentScheduleDay().ConvertCatalogueAnimesToFavoritedAsync(settings.ShowNSFW);

                        var tdayAnimes = new TodayAnimes
                        {
                            DayOfWeek = DateTime.Today.DayOfWeek,
                            FavoritedAnimes = animes,
                            ShowNSFW = settings.ShowNSFW,
                        };

                        TodayAnimes = tdayAnimes;
                        todayAnimeCollection.Upsert(0, tdayAnimes);

                        IsLoadingTodayAnimes = false;
                    }
                    else if (TodayAnimes == null || todayAnimes?.DayOfWeek != DateTime.Today.DayOfWeek)
                        TodayAnimes = todayAnimes;

                }, _cancellationTokenSource.Token);

                var loadRecommendationsTask = Task.Run(async () =>
                {
                    var recommendationAnimesCollection = App.liteDB.GetCollection<RecommendationAnimes>();
                    var recommendationsCache = recommendationAnimesCollection.FindById(0);

                    var animeCollection = settings.ShowNSFW
                       ? App.liteDB.GetCollection<FavoritedAnime>().FindAll().ToList()
                       : App.liteDB.GetCollection<FavoritedAnime>().Find(p => !p.IsNSFW).ToList();

                    int collectionCount = animeCollection.Count;

                    if (collectionCount == 0)
                        return;

                    if (recommendationsCache == null
                    || recommendationsCache != null && DateTime.Now.Subtract(recommendationsCache.LastRecommendationDate).TotalHours >= 1
                    || settings.ShowNSFW != recommendationsCache.ShowNSFW)
                    {
                        var rand = new Random();

                        int indexPick = rand.Next(collectionCount);

                        FavoritedAnime animeAsRecommend = animeCollection[indexPick];

                        if (_cancellationTokenSource.IsCancellationRequested)
                            _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                        await App.DelayRequest(2);
                        Recommendations recommendations = await App.Jikan.GetAnimeRecommendations(animeAsRecommend.Anime.MalId);

                        var recommendationAnimes = recommendations.RecommendationCollection
                        .Where(recommendation => !animeCollection.Exists(favoritedAnime => recommendation.MalId == favoritedAnime.Anime.MalId))
                        .ToList();

                        if (recommendationAnimes.Count == 0)
                        {
                            if (recommendationsCache != null)
                                LoadRecommendationFromCache(settings, recommendationAnimesCollection, recommendationsCache);

                            return;
                        }

                        var recomendationsList = new HashSet<Recommendation>();

                        for (int i = 0; i < 5; i++)
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(50));
                            indexPick = rand.Next(recommendationAnimes.Count);

                            recomendationsList.Add(recommendationAnimes[indexPick]);
                        }

                        var recommendationAnimesCache = new RecommendationAnimes
                        {
                            Recommendations = recomendationsList
                            .Where(recommendation => !App.liteDB.GetCollection<FavoritedAnime>()
                            .Exists(favoritedAnime => recommendation.MalId == favoritedAnime.Anime.MalId))
                            .ToList(),
                            LastRecommendationDate = DateTime.Now,
                            ShowNSFW = settings.ShowNSFW,
                        };

                        RecommendationAnimes = recommendationAnimesCache;
                        HasRecommendations = RecommendationAnimes.Recommendations.Count() != 0;
                        recommendationAnimesCollection.Upsert(0, recommendationAnimesCache);

                    }

                    else if (DateTime.Now.Subtract(recommendationsCache.LastRecommendationDate).TotalHours < 1)
                        LoadRecommendationFromCache(settings, recommendationAnimesCollection, recommendationsCache);

                }, _cancellationTokenSource.Token);

                await Task.WhenAny(loadRecommendationsTask, loadTodayAnimesTask);
            }
            catch (JikanDotNet.Exceptions.JikanRequestException ex)
            {
                ex.SaveExceptionData();
            }
            catch (OperationCanceledException ex)
            { }
            catch (ObjectDisposedException ex)
            { }
            catch (Exception ex)
            {
                ex.SaveExceptionData();
            }
            finally
            {
                IsLoadingTodayAnimes = false;
            }

        }

        #region properties
        private RecommendationAnimes _recomendationAnimes;
        public RecommendationAnimes RecommendationAnimes
        {
            get { return _recomendationAnimes; }
            set { SetProperty(ref _recomendationAnimes, value); }
        }

        private TodayAnimes _todayAnimes;
        public TodayAnimes TodayAnimes
        {
            get { return _todayAnimes; }
            set { SetProperty(ref _todayAnimes, value); }
        }

        private object _selectedItem;
        public object SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        private bool _isLoadingTodayAnimes;
        public bool IsLoadingTodayAnimes
        {
            get { return _isLoadingTodayAnimes; }
            set { SetProperty(ref _isLoadingTodayAnimes, value); }
        }

        private bool _hasRecommendations;
        public bool HasRecommendations
        {
            get { return _hasRecommendations; }
            set { SetProperty(ref _hasRecommendations, value); }
        }

        #endregion

        #region commands
        public ICommand SelectedItemCommand { get; private set; }
        private async Task OnSelectedItem()
        {
            if (IsNotBusy && SelectedItem != null)
            {
                IsBusy = true;

                if (SelectedItem is Recommendation recommendation)
                    await NavigationManager.NavigateShellAsync<AnimeSpecsViewModel>(recommendation.MalId);
                else if (SelectedItem is FavoritedAnime anime)
                    await NavigationManager.NavigateShellAsync<AnimeSpecsViewModel>(anime.Anime.MalId);

                SelectedItem = null;
                IsBusy = false;
            }
        }

        #endregion

        #region ViewModel Methods
        private void LoadRecommendationFromCache(SettingsPreferences settings
            , ILiteCollection<RecommendationAnimes> recommendationAnimesCollection, RecommendationAnimes recommendationsCache)
        {

            recommendationsCache.Recommendations = recommendationsCache.Recommendations
                        .Where(recommendation => !App.liteDB.GetCollection<FavoritedAnime>()
                        .Exists(favoritedAnime => recommendation.MalId == favoritedAnime.Anime.MalId))
                        .ToList();

            RecommendationAnimes = recommendationsCache;

            HasRecommendations = RecommendationAnimes.Recommendations.Count() != 0;

            recommendationAnimesCollection.Upsert(0, recommendationsCache);
        }
        #endregion
    }
}
