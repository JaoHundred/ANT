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
            SelectionModeCommand = new magno.Command(OnSelectionModeChanged);
        }

        //TODO: implementar esta VM e sua View(implementar o template da coleção, e mecanismo de switch para ativar ou desativar as notificações
        //animes que não estão exibindo ou não tem data da semana não vão ter essa opção do switch
        public async Task LoadAsync(object param)
        {
            var groupTask = Task.Run(() =>
            {
                Lazy<ResourceManager> resMgr = new Lazy<ResourceManager>(
                    () => new ResourceManager(typeof(Lang.Lang)));

                var group = new List<GroupedFavoriteAnimeByWeekDay>();

                var favorited = App.FavoritedAnimes.Where(p => p.NextStreamDate != null);
                var favoritedNullDate = App.FavoritedAnimes.Where(p => p.NextStreamDate == null).GroupBy(p => p.NextStreamDate);

                var sortedDaysOfWeek = favorited?.OrderBy(p => p.NextStreamDate.Value.DayOfWeek).GroupBy(p => p.NextStreamDate.Value.DayOfWeek).ToList();
                var todayAnimes = favorited?.Where(p => p.NextStreamDate.Value.DayOfWeek == DateTime.Today.DayOfWeek)
                .GroupBy(p => p.NextStreamDate.Value.DayOfWeek);

                if (todayAnimes != null)
                {
                    sortedDaysOfWeek.RemoveAll(p => p.Key == DateTime.Today.DayOfWeek);
                    foreach (var item in todayAnimes)
                        group.Add(new GroupedFavoriteAnimeByWeekDay(resMgr.Value.GetString(DateTime.Today.DayOfWeek.ToString())
                        , item.ToList()));
                }
               
                foreach (var anime in sortedDaysOfWeek)
                    group.Add(new GroupedFavoriteAnimeByWeekDay(resMgr.Value.GetString(anime.Key.ToString()), anime.ToList()));
                
                if (favoritedNullDate != null)
                    foreach (var item in favoritedNullDate)
                        group.Add(new GroupedFavoriteAnimeByWeekDay(Lang.Lang.UnknownDate, item.ToList()));

                return group;
            });


            GroupedFavoriteByWeekList = new ObservableRangeCollection<GroupedFavoriteAnimeByWeekDay>(await groupTask);
        }

        #region Propriedades
        private ObservableRangeCollection<GroupedFavoriteAnimeByWeekDay> _groupedFavoriteByWeekList;
        public ObservableRangeCollection<GroupedFavoriteAnimeByWeekDay> GroupedFavoriteByWeekList
        {
            get { return _groupedFavoriteByWeekList; }
            set { SetProperty(ref _groupedFavoriteByWeekList, value); }
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
            await App.DelayRequest();
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

        public ICommand SelectionModeCommand { get; set; }
        private void OnSelectionModeChanged()
        {
            IsMultiSelect = !IsMultiSelect;
        }
        #endregion

    }
}
