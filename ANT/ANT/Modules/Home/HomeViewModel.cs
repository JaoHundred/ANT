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
                await App.DelayRequest(2);
                var loadRecommendationsTask = Task.Run(async () =>
                {
                    var animeCollection = settings.ShowNSFW
                         ? App.liteDB.GetCollection<FavoritedAnime>().FindAll().ToList()
                         : App.liteDB.GetCollection<FavoritedAnime>().Find(p => !p.IsNSFW).ToList();

                    int collectionCount = animeCollection.Count;

                    if (collectionCount == 0)
                    {
                        return;
                    }

                    var rand = new Random();

                    int indexPick = rand.Next(collectionCount);

                    FavoritedAnime animeAsRecommend = animeCollection[indexPick];

                    if (_cancellationTokenSource.IsCancellationRequested)
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    Recommendations recommendations = await App.Jikan.GetAnimeRecommendations(animeAsRecommend.Anime.MalId);

                    var recommendationAnimes = recommendations.RecommendationCollection
                    .Where(recommendation => !animeCollection.Exists(p => recommendation.MalId == p.Anime.MalId))
                    .ToList();

                    if (recommendationAnimes.Count == 0)
                    {
                        return;
                    }

                    var recomendationsList = new HashSet<Recommendation>();


                    for (int i = 0; i < 5; i++)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(50));
                        indexPick = rand.Next(recommendationAnimes.Count);

                        recomendationsList.Add(recommendationAnimes[indexPick]);
                    }

                    Recommendations = recomendationsList.ToList();
                    HasRecommendations = true;

                }, _cancellationTokenSource.Token);

                await App.DelayRequest(2);
                var loadTodayAnimesTask = Task.Run(async () =>
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    Schedule schedule = await App.Jikan.GetSchedule(DateTime.Today.DayOfWeek.ConvertDayOfWeekToScheduleDay());

                    TodayAnimes = await schedule.GetCurrentScheduleDay().ConvertCatalogueAnimesToFavoritedAsync(settings.ShowNSFW);

                }, _cancellationTokenSource.Token);

                await Task.WhenAny(loadRecommendationsTask, loadTodayAnimesTask);
            }
            catch (JikanDotNet.Exceptions.JikanRequestException ex)
            {
                DependencyService.Get<IToast>().MakeToastMessageLong($"Erros code {ex.ResponseCode}");
            }
            catch (OperationCanceledException ex)
            { }
            catch (ObjectDisposedException ex)
            { }
            catch (Exception ex)
            {
                DependencyService.Get<IToast>().MakeToastMessageLong(ex.Message);
            }

        }

        #region properties
        private IList<Recommendation> _recomendations;
        public IList<Recommendation> Recommendations
        {
            get { return _recomendations; }
            set { SetProperty(ref _recomendations, value); }
        }

        private IList<FavoritedAnime> _todayAnimes;
        public IList<FavoritedAnime> TodayAnimes
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

    }
}
