using System;
using System.Collections.Generic;
using System.Text;
using magno = MvvmHelpers.Commands;
using MvvmHelpers;
using ANT.UTIL;
using System.Windows.Input;
using System.Threading.Tasks;
using ANT.Interfaces;
using ANT.Model;
using ANT.Core;
using System.Linq;
using System.Resources;
using Xamarin.Forms;
using JikanDotNet;
using MvvmHelpers.Commands;

namespace ANT.Modules
{
    public class FavoriteAnimeViewModel : BaseVMExtender
    {
        public FavoriteAnimeViewModel()
        {
            SearchCommand = new magno.AsyncCommand(OnSearch);
            ClearTextCommand = new magno.Command(OnClearText);
            DeleteFavoriteCommand = new magno.AsyncCommand(OnDeleteFavoriteCommand);
            ClearAllCommand = new magno.AsyncCommand(OnClearAll);
            SelectionModeCommand = new magno.Command(OnSelectionMode);
            OpenAnimeCommand = new magno.AsyncCommand(OnOpenAnime);
            GenreCheckedCommand = new magno.Command<GenreData>(OnGenreCheck);
            ApplyFilterCommand = new magno.AsyncCommand(OnApplyFilter);
            ResetFilterCommand = new magno.Command(OnResetFilter);
            DayOfWeekCheckedCommand = new magno.Command<DayOfWeekFilterDate>(OnDayOfWeekCheck);
            SwitchCommand = new Xamarin.Forms.Command<FavoritedAnime>(OnSwitch);
            StepperCommand = new Xamarin.Forms.Command<FavoritedAnime>(OnStepper);
            UpdateFavoriteAnimesCommand = new AsyncCommand(OnUpdateAnimes);
        }

        public async Task LoadAsync(object param)
        {
            if (_isUpdatingAnimes)
                return;

            _settingsPreferences = App.liteDB.GetCollection<SettingsPreferences>().FindById(0);

            FilterData = new FilterData
            {
                Genres = ANT.UTIL.AnimeExtension.FillGenres(_settingsPreferences.ShowNSFW),
                DayOfWeekOrder = UTIL.AnimeExtension.FillTodayDayOfWeek(),
            };

            _originalCollection = await ConstructGroupedCollectionAsync();
            GroupedFavoriteByWeekList = new ObservableRangeCollection<GroupedFavoriteAnimeByWeekDay>(_originalCollection);
        }

        private static Task<List<GroupedFavoriteAnimeByWeekDay>> ConstructGroupedCollectionAsync()
        {
            return Task.Run(() =>
            {
                var favoriteCollection = App.liteDB.GetCollection<FavoritedAnime>().FindAll().ToList();

                if(!_settingsPreferences.ShowNSFW)
                {
                    favoriteCollection = favoriteCollection.Where(p => !p.IsNSFW).ToList();
                }

                Lazy<ResourceManager> resMgr = new Lazy<ResourceManager>(
                                    () => new ResourceManager(typeof(Lang.Lang)));

                var group = new List<GroupedFavoriteAnimeByWeekDay>();

                var airingAndHasDayWeekAnimes = favoriteCollection.Where(p => p.NextStreamDate != null && p.Anime.Airing);


                var groupedFavoriteAnimes = airingAndHasDayWeekAnimes?.GroupBy(p => p.NextStreamDate.Value.DayOfWeek).ToList();

                var todayAnimes = airingAndHasDayWeekAnimes?.Where(p => p.NextStreamDate.Value.DayOfWeek == DateTime.Today.DayOfWeek)
                .GroupBy(p => p.NextStreamDate.Value.DayOfWeek);

                var tomorrowAnimes = airingAndHasDayWeekAnimes?.Where(p => p.NextStreamDate.Value.DayOfWeek == DateTime.Today.AddDays(1).DayOfWeek)
                .GroupBy(p => p.NextStreamDate.Value.DayOfWeek);

                var today = todayAnimes.LastOrDefault();
                var tomorrow = tomorrowAnimes.LastOrDefault();

                if (today != null)
                {
                    groupedFavoriteAnimes.RemoveAll(p => p.Key == today.Key);

                    string groupName = resMgr.Value.GetString(DateTime.Today.DayOfWeek.ToString());
                    string todayString = $"{groupName} ({Lang.Lang.TodayAnimes})";

                    group.Add(new GroupedFavoriteAnimeByWeekDay(todayString
                        , today.ToList()));
                }

                if (tomorrow != null)
                {
                    groupedFavoriteAnimes.RemoveAll(p => p.Key == tomorrow.Key);

                    string groupName = resMgr.Value.GetString(DateTime.Today.AddDays(1).DayOfWeek.ToString());
                    string todayString = $"{groupName} ({Lang.Lang.TomorrowAnimes})";

                    group.Add(new GroupedFavoriteAnimeByWeekDay(todayString
                        , tomorrow.ToList()));
                }

                IList<DayOfWeek> daysOfWeek = UTIL.AnimeExtension.FillDayOfWeek();

                int nextDay = (int)DateTime.Today.DayOfWeek;
                foreach (var days in daysOfWeek)
                {
                    nextDay += 1;

                    if (nextDay > 6)
                        nextDay = 0;

                    IGrouping<DayOfWeek, FavoritedAnime> nextGroupDay;

                    nextGroupDay = groupedFavoriteAnimes.FirstOrDefault(p => p.Key == (DayOfWeek)nextDay);

                    if (nextGroupDay == null)
                        continue;

                    group.Add(new GroupedFavoriteAnimeByWeekDay(resMgr.Value.GetString(nextGroupDay.Key.ToString()), nextGroupDay.ToList()));
                }

                favoriteCollection.RemoveAll(p => airingAndHasDayWeekAnimes.Contains(p));

                var notStarted = favoriteCollection.Where(p => UTIL.AnimeExtension.HasNotStartedAiring(p));
                var groupedFavoritedNotStarted = notStarted.GroupBy(p => !p.CanGenerateNotifications);

                if (groupedFavoritedNotStarted != null)
                    foreach (var item in groupedFavoritedNotStarted)
                        group.Add(new GroupedFavoriteAnimeByWeekDay(Lang.Lang.NotStarted, item.ToList()));

                favoriteCollection.RemoveAll(p => notStarted.Contains(p));

                var finished = favoriteCollection.Where(p => UTIL.AnimeExtension.HasFinishedAiring(p));
                var groupedFavoritedFinished = finished.GroupBy(p => !p.CanGenerateNotifications);

                if (groupedFavoritedFinished != null)
                    foreach (var item in groupedFavoritedFinished)
                        group.Add(new GroupedFavoriteAnimeByWeekDay(Lang.Lang.FinishedAiring, item.ToList()));

                favoriteCollection.RemoveAll(p => finished.Contains(p));

                var groupedFavoritedUnknown = favoriteCollection.Where(p =>
               UTIL.AnimeExtension.IsUnknownAiring(p))
                .GroupBy(p => !p.CanGenerateNotifications);

                if (groupedFavoritedUnknown != null)
                    foreach (var item in groupedFavoritedUnknown)
                        group.Add(new GroupedFavoriteAnimeByWeekDay(Lang.Lang.UnknownDate, item.ToList()));

#if DEBUG
                var groupedList = group.SelectMany(p => p.Select(q => q)).ToList();

                var excludedIDs = new HashSet<long>(groupedList.Select(p => p.Anime.MalId));
                var missedItens = favoriteCollection.Where(p => !excludedIDs.Contains(p.Anime.MalId)).ToList();

                Console.WriteLine($"Itens perdidos na filtragem {missedItens.Count}");
#endif

                return group;
            });
        }

        public IList<GroupedFavoriteAnimeByWeekDay> _originalCollection;
        private static SettingsPreferences _settingsPreferences;
        #region Propriedades
        private ObservableRangeCollection<GroupedFavoriteAnimeByWeekDay> _groupedFavoriteByWeekList;
        public ObservableRangeCollection<GroupedFavoriteAnimeByWeekDay> GroupedFavoriteByWeekList
        {
            get { return _groupedFavoriteByWeekList; }
            set { SetProperty(ref _groupedFavoriteByWeekList, value); }
        }

        private FavoritedAnime _selectedItem;
        public FavoritedAnime SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        private IList<object> _selectedItems;
        public IList<object> SelectedItems
        {
            get { return _selectedItems; }
            set { SetProperty(ref _selectedItems, value); }
        }

        private FilterData _filterData;
        public FilterData FilterData
        {
            get { return _filterData; }
            set { SetProperty(ref _filterData, value); }
        }

        #endregion


        #region Commands
        public ICommand SearchCommand { get; private set; }
        private async Task OnSearch()
        {
            var resultListTask = Task.Run(() =>
            {
                IList<GroupedFavoriteAnimeByWeekDay> result = null;

                result = _originalCollection.Select(animeGroup
                    => new GroupedFavoriteAnimeByWeekDay(animeGroup.GroupName,
                    animeGroup.Where(anime => anime.Anime.Title.ToLowerInvariant().Contains(SearchQuery.ToLowerInvariant()))
                    .ToList()))
                .Where(animeGroup => animeGroup.Count > 0)
                .ToList();

                return result;
            });

            GroupedFavoriteByWeekList.ReplaceRange(await resultListTask);
        }

        public ICommand ClearTextCommand { get; private set; }

        private void OnClearText()
        {
            SearchQuery = string.Empty;
            SearchCommand.Execute(null);
        }

        public ICommand DeleteFavoriteCommand { get; private set; }

        private async Task OnDeleteFavoriteCommand()
        {
            var items = SelectedItems.Cast<FavoritedAnime>();
            var favoriteCollection = App.liteDB.GetCollection<FavoritedAnime>();
            foreach (var item in items)
                favoriteCollection.Delete(item.Anime.MalId);

            var constructTask = ConstructGroupedCollectionAsync();

            GroupedFavoriteByWeekList = new ObservableRangeCollection<GroupedFavoriteAnimeByWeekDay>(await constructTask);
        }

        public ICommand ClearAllCommand { get; private set; }
        private async Task OnClearAll()
        {
            if (GroupedFavoriteByWeekList.Count == 0)
                return;

            bool canNavigate = await NavigationManager.CanPopUpNavigateAsync<ChoiceModalViewModel>();

            if (canNavigate)
            {
                var confirmDelegateAction = new Action(async () =>
                {
                    await Task.Run(() =>
                    {
                        var favoriteCollection = App.liteDB.GetCollection<FavoritedAnime>();
                        var animesToCancelNotification = favoriteCollection.FindAll().ToList();
                        favoriteCollection.DeleteAll();
                    });

                    GroupedFavoriteByWeekList.Clear();
                });

                await NavigationManager.
                    NavigatePopUpAsync<ChoiceModalViewModel>(Lang.Lang.ClearFavoriteList, Lang.Lang.ClearCannotBeUndone, confirmDelegateAction);
            }
        }

        public ICommand SelectionModeCommand { get; private set; }
        private void OnSelectionMode()
        {
            if (SelectionMode == SelectionMode.Multiple)
                SingleSelectionMode();
            else
            {
                MultiSelectionMode();
                SelectedItems = null;
            }
        }

        bool _canNavigate = true;
        public ICommand OpenAnimeCommand { get; private set; }
        private async Task OnOpenAnime()
        {

            if (!IsMultiSelect && SelectedItem != null && _canNavigate)
            {
                _canNavigate = false;
                await NavigationManager.NavigateShellAsync<AnimeSpecsViewModel>(SelectedItem.Anime.MalId);
                SelectedItem = null;
                _canNavigate = true;
            }
        }

        public ICommand GenreCheckedCommand { get; private set; }
        private void OnGenreCheck(GenreData genreData)
        {
            genreData.IsChecked = !genreData.IsChecked;
        }

        public ICommand DayOfWeekCheckedCommand { get; private set; }
        private void OnDayOfWeekCheck(DayOfWeekFilterDate dayOfWeekFilterDate)
        {
            dayOfWeekFilterDate.IsChecked = !dayOfWeekFilterDate.IsChecked;
        }

        public ICommand ApplyFilterCommand { get; private set; }
        private async Task OnApplyFilter()
        {
            var checkedGenres = FilterData.Genres.Where(p => p.IsChecked).Select(p => p.Genre).ToList();
            var checkedDaysOfWeek = FilterData.DayOfWeekOrder.Where(p => p.IsChecked).ToList();

            MessagingCenter.Send(this, "CloseFilterView");

            await Task.Delay(TimeSpan.FromMilliseconds(500)); // usado para impedir que seja visto um leve engasto na filtragem

            var animeToRemove = new List<FavoritedAnime>();
            // meio encontrado de não ter a originalCollection filtrada, não está bom, mas é a correção por hora
            _originalCollection = await ConstructGroupedCollectionAsync();
            var groupAnimes = new List<GroupedFavoriteAnimeByWeekDay>(_originalCollection);

            //TODO:contar a quantidade de animes cadastrados e ver se todos estão aparecendo apenas uma única vez

            //TODO:olhar aqui em conjunto com o HasAnyDayOfWeek
            for (int i = 0; i < groupAnimes.Count; i++)
            {
                GroupedFavoriteAnimeByWeekDay favoritedAnimeGroup = groupAnimes[i];

                for (int j = 0; j < favoritedAnimeGroup.Count; j++)
                {
                    FavoritedAnime favoritedAnime = favoritedAnimeGroup[j];

                    var hasAllGenresTask = favoritedAnime.HasAllSpecifiedGenresAsync(checkedGenres.ToArray());
                    var hasAnyDaysOfWeekTask = favoritedAnime.HasAnyDayOfWeekAsync(checkedDaysOfWeek.ToArray());

                    if (!await hasAllGenresTask || !await hasAnyDaysOfWeekTask)
                        animeToRemove.Add(favoritedAnime);
                }
            }

            if (animeToRemove.Count > 0)
                foreach (var item in animeToRemove)
                {
                    foreach (var group in groupAnimes)
                    {
                        if (group.Contains(item))
                            group.Remove(item);
                    }
                }

            var groupsWithAnimes = groupAnimes.Where(p => p.Count > 0);
            GroupedFavoriteByWeekList.ReplaceRange(groupsWithAnimes);
        }

        public ICommand SwitchCommand { get; private set; }
        private void OnSwitch(FavoritedAnime favoritedAnime)
        {
            var favoriteds = App.liteDB.GetCollection<FavoritedAnime>();
            FavoritedAnime favorited = favoriteds.FindById(favoritedAnime.Anime.MalId);

            if (favorited == null)
                return;

            if (favorited.CanGenerateNotifications != favoritedAnime.CanGenerateNotifications)
                favoriteds.Update(favoritedAnime.Anime.MalId, favoritedAnime);
        }

        public ICommand StepperCommand { get; private set; }
        private void OnStepper(FavoritedAnime favoritedAnime)
        {
            var favoriteds = App.liteDB.GetCollection<FavoritedAnime>();
            FavoritedAnime favorited = favoriteds.FindById(favoritedAnime.Anime.MalId);

            if (favorited == null)
                return;

            if (favorited.LastEpisodeSeen != favoritedAnime.LastEpisodeSeen)
                favoriteds.Update(favoritedAnime.Anime.MalId, favoritedAnime);
        }

        public ICommand ResetFilterCommand { get; private set; }
        private async void OnResetFilter()
        {
            var checkedGenres = FilterData.Genres.Where(p => p.IsChecked);
            var checkedDays = FilterData.DayOfWeekOrder.Where(p => p.IsChecked);

            foreach (var item in checkedGenres)
                item.IsChecked = false;
            foreach (var item in checkedDays)
                item.IsChecked = false;

            _originalCollection = await ConstructGroupedCollectionAsync();
            GroupedFavoriteByWeekList.ReplaceRange(_originalCollection);
            MessagingCenter.Send(this, "CloseFilterView");

            //TODO: mostrar o padrão de filtro quando o sistema de filtro for resetado
        }

        private bool _isUpdatingAnimes;

        //TODO: testar comando abaixo com a funcionalidade nova de filtrar por isNSFW dos animes
        public ICommand UpdateFavoriteAnimesCommand { get; private set; }
        private async Task OnUpdateAnimes()
        {
            _isUpdatingAnimes = true;
            bool canNavigate = await NavigationManager.CanPopUpNavigateAsync<ChoiceModalViewModel>();

            if (canNavigate)
            {
                var action = new Action(async () =>
                {
                    var favoriteCollection = App.liteDB.GetCollection<FavoritedAnime>().FindAll().ToList();

                    if (!_settingsPreferences.ShowNSFW)
                    {
                        favoriteCollection = favoriteCollection.Where(p => !p.IsNSFW).ToList();
                    }

                    await NavigationManager
                    .NavigatePopUpAsync<ProgressPopupViewModel>(favoriteCollection, this
                    , new Action(async () => { await LoadAsync(null); }));
                });

                await NavigationManager.NavigatePopUpAsync<ChoiceModalViewModel>(Lang.Lang.UpdatingAnimes, Lang.Lang.UpdatingAnimesMessage, action);
            }
            _isUpdatingAnimes = false;
        }


        #endregion


        //TODO: implementar filtros https://github.com/JaoHundred/ANT/issues/50
        //usar como base o filtro que existe em catalogueview 
    }
}
