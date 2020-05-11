﻿using System;
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

namespace ANT.Modules
{
    public class FavoriteAnimeViewModel : BaseVMExtender
    {
        public FavoriteAnimeViewModel()
        {
            SearchCommand = new magno.AsyncCommand(OnSearch);
            ClearTextCommand = new magno.AsyncCommand(OnClearText);
            DeleteFavoriteCommand = new magno.AsyncCommand(OnDeleteFavoriteCommand);
            ClearAllCommand = new magno.AsyncCommand(OnClearAll);
            SelectionModeCommand = new magno.Command(OnSelectionMode);
            OpenAnimeCommand = new magno.AsyncCommand(OnOpenAnime);
        }

        public async Task LoadAsync(object param)
        {
            GroupedFavoriteByWeekList = new ObservableRangeCollection<GroupedFavoriteAnimeByWeekDay>(await ConstructGroupedCollectionAsync());
        }

        private static Task<List<GroupedFavoriteAnimeByWeekDay>> ConstructGroupedCollectionAsync()
        {
            return Task.Run(() =>
            {
                Lazy<ResourceManager> resMgr = new Lazy<ResourceManager>(
                                    () => new ResourceManager(typeof(Lang.Lang)));

                var group = new List<GroupedFavoriteAnimeByWeekDay>();

                var favorited = App.FavoritedAnimes.Where(p => p.NextStreamDate != null);
                var groupedFavoritedNullDate = App.FavoritedAnimes.Where(p => p.NextStreamDate == null).GroupBy(p => p.NextStreamDate);

                var groupedFavoriteAnimes = favorited?.GroupBy(p => p.NextStreamDate.Value.DayOfWeek).ToList();
                var todayAnimes = favorited?.Where(p => p.NextStreamDate.Value.DayOfWeek == DateTime.Today.DayOfWeek)
                .GroupBy(p => p.NextStreamDate.Value.DayOfWeek);

                var today = todayAnimes.LastOrDefault();

                if (today != null)
                {
                    groupedFavoriteAnimes.RemoveAll(p => p.Key == today.Key);

                    string groupName = resMgr.Value.GetString(DateTime.Today.DayOfWeek.ToString());
                    string todayString = $"{groupName} ({Lang.Lang.TodayAnimes})";

                    group.Add(new GroupedFavoriteAnimeByWeekDay(todayString
                        , today.ToList()));
                }

                IList<DayOfWeek> daysOfWeek = AnimeExtension.FillDayOfWeek();

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

                if (groupedFavoritedNullDate != null)
                    foreach (var item in groupedFavoritedNullDate)
                        group.Add(new GroupedFavoriteAnimeByWeekDay(Lang.Lang.UnknownDate, item.ToList()));

                return group;
            });
        }

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

        #endregion


        #region Commands
        public ICommand SearchCommand { get; private set; }
        private async Task OnSearch()
        {
            await App.DelayRequest();
        }

        public ICommand ClearTextCommand { get; private set; }

        private async Task OnClearText()
        {
            await App.DelayRequest();
        }

        public ICommand DeleteFavoriteCommand { get; private set; }

        private async Task OnDeleteFavoriteCommand()
        {
            var items = SelectedItems.Cast<FavoritedAnime>();

            foreach (var item in items)
            {
                App.FavoritedAnimes.Remove(item);
                await NotificationManager.CancelNotificationAsync(item);
            }

            var constructTask = ConstructGroupedCollectionAsync();
            var jsonStorageTask = JsonStorage.SaveDataAsync(App.FavoritedAnimes, StorageConsts.LocalAppDataFolder, StorageConsts.FavoritedAnimesFileName);

            GroupedFavoriteByWeekList = new ObservableRangeCollection<GroupedFavoriteAnimeByWeekDay>(await constructTask);
            await jsonStorageTask;
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
                    await Task.Run(async () =>
                    {
                        foreach (var item in App.FavoritedAnimes)
                            await NotificationManager.CancelNotificationAsync(item);
                        
                        App.FavoritedAnimes.Clear();
                        await JsonStorage.SaveDataAsync(App.FavoritedAnimes, StorageConsts.LocalAppDataFolder
                            , StorageConsts.FavoritedAnimesFileName);

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
        #endregion


        //TODO: implementar esta VM e sua View(implementar o template da coleção, e mecanismo de switch para ativar ou desativar as notificações
        //animes que não estão exibindo ou não tem data da semana não vão ter essa opção do switch
    }
}
