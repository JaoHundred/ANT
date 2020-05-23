﻿using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;
using magno = MvvmHelpers.Commands;
using ANT.Interfaces;
using System.Threading.Tasks;
using ANT.Model;
using System.Linq;
using System.Windows.Input;
using ANT.Core;

namespace ANT.Modules
{
    public class RecentViewModel : BaseViewModel
    {
        public RecentViewModel()
        {
            ClearAllRecentCommand = new magno.AsyncCommand(OnClearAllRecent);
            OpenAnimeCommand = new magno.AsyncCommand(OnOpenAnime);

            Recents = new ObservableRangeCollection<RecentVisualized>();
        }

        public async Task LoadAsync(object param)
        {
            await Task.Run(() =>
            {
                var sortedRecents = App.liteDB.GetCollection<RecentVisualized>().FindAll().OrderByDescending(p => p.Date).ToList();
                Recents.ReplaceRange(sortedRecents);
            });
        }

        #region propriedades vm
        private ObservableRangeCollection<RecentVisualized> _recents;
        public ObservableRangeCollection<RecentVisualized> Recents
        {
            get { return _recents; }
            set { SetProperty(ref _recents, value); }
        }

        private RecentVisualized _selectedRecent;

        public RecentVisualized SelectedRecent
        {
            get { return _selectedRecent; }
            set { SetProperty(ref _selectedRecent, value); }
        }
        #endregion

        #region commands
        public ICommand ClearAllRecentCommand { get; private set; }
        private async Task OnClearAllRecent()
        {
            if (Recents.Count == 0)
                return;

            bool canNavigate = await NavigationManager.CanPopUpNavigateAsync<ChoiceModalViewModel>();

            if (canNavigate)
            {
                var confirmDelegateAction = new Action(() =>
                {
                    App.liteDB.GetCollection<RecentVisualized>().DeleteAll();

                    Recents.Clear();
                });

                await NavigationManager.NavigatePopUpAsync<ChoiceModalViewModel>(Lang.Lang.ClearRecentList, Lang.Lang.ClearCannotBeUndone, confirmDelegateAction);
            }
        }

        public ICommand OpenAnimeCommand { get; private set; }
        private async Task OnOpenAnime()
        {
            if (IsNotBusy && SelectedRecent != null)
            {
                IsBusy = true;
                await NavigationManager.NavigateShellAsync<AnimeSpecsViewModel>(SelectedRecent.FavoritedAnime.Anime.MalId);
                IsBusy = false;
            }

            SelectedRecent = null;
        }
        #endregion
    }
}
