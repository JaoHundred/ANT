using ANT.Core;
using ANT.Model;
using ANT.UTIL;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using magno = MvvmHelpers.Commands;

namespace ANT.Modules
{
    public class FavoriteVoiceActorViewModel : BaseVMExtender
    {
        public FavoriteVoiceActorViewModel()
        {
            ClearAllRecentCommand = new magno.AsyncCommand(OnClearAllRecent);
            OpenVoiceActorCommand = new magno.AsyncCommand(OnOpenVoiceActor);
            DeleteFavoriteCommand = new magno.AsyncCommand(OnDeleteFavorite);
            ClearTextCommand = new magno.Command(OnClearText);
            SearchCommand = new magno.AsyncCommand(OnSearch);
            SelectionModeCommand = new magno.Command(OnSelectionMode);
            UpdateFavoriteVoiceActorsCommand = new magno.AsyncCommand(OnUpdateFavoriteVoiceActors);

            FavoritedActors = new ObservableRangeCollection<FavoritedVoiceActor>();
        }

        private List<FavoritedVoiceActor> _originalCollection;

        public async Task LoadAsync()
        {
            await Task.Run(() =>
            {
                if (_isUpdatingVoiceActors)
                    return;

                FavoritedActors.ReplaceRange(App.liteDB.GetCollection<FavoritedVoiceActor>().FindAll().ToList());
                _originalCollection = new List<FavoritedVoiceActor>(FavoritedActors);
            });
        }


        #region propriedades
        private ObservableRangeCollection<FavoritedVoiceActor> _favoritedActors;
        public ObservableRangeCollection<FavoritedVoiceActor> FavoritedActors
        {
            get { return _favoritedActors; }
            set { SetProperty(ref _favoritedActors, value); }
        }

        private FavoritedVoiceActor _selectedFavorite;
        public FavoritedVoiceActor SelectedFavorite
        {
            get { return _selectedFavorite; }
            set { SetProperty(ref _selectedFavorite, value); }
        }

        private IList<object> _selectedFavorites;
        public IList<object> SelectedFavorites
        {
            get { return _selectedFavorites; }
            set { SetProperty(ref _selectedFavorites, value); }
        }
        #endregion

        #region comandos
        public ICommand ClearAllRecentCommand { get; private set; }
        private async Task OnClearAllRecent()
        {
            if (FavoritedActors.Count == 0)
                return;

            bool canNavigate = await NavigationManager.CanPopUpNavigateAsync<ChoiceModalViewModel>();

            if (canNavigate)
            {
                var confirmDelegateAction = new Action(() =>
                {
                    App.liteDB.GetCollection<FavoritedVoiceActor>().DeleteAll();
                    FavoritedActors.Clear();
                });

                await NavigationManager.
                    NavigatePopUpAsync<ChoiceModalViewModel>(Lang.Lang.ClearFavoriteList, Lang.Lang.ClearCannotBeUndone, confirmDelegateAction);
            }
        }

        public ICommand OpenVoiceActorCommand { get; private set; }
        private async Task OnOpenVoiceActor()
        {
            if (IsNotBusy && SelectedFavorite != null)
            {
                IsBusy = true;
                await NavigationManager.NavigateShellAsync<VoiceActorViewModel>(SelectedFavorite.VoiceActor.MalId);
                IsBusy = false;
            }

            SelectedFavorite = null;
        }

        public ICommand ClearTextCommand { get; private set; }
        private void OnClearText()
        {
            SearchQuery = string.Empty;
            SearchCommand.Execute(null);
        }

        public ICommand SearchCommand { get; private set; }
        private async Task OnSearch()
        {
            var resultListTask = Task.Run(() =>
            {
                return _originalCollection.Where(actor => actor.VoiceActor.Name.ToLowerInvariant().Contains(SearchQuery.ToLowerInvariant())).ToList();
            });

            var resultList = await resultListTask;
            FavoritedActors.ReplaceRange(resultList);
        }

        public ICommand SelectionModeCommand { get; private set; }
        private void OnSelectionMode()
        {
            if (IsMultiSelect)
                SingleSelectionMode();
            else
                MultiSelectionMode();
        }

        public ICommand DeleteFavoriteCommand { get; private set; }
        private async Task OnDeleteFavorite()
        {
            if (SelectedFavorites?.Count > 0)
            {
                bool canNavigate = await NavigationManager.CanPopUpNavigateAsync<ChoiceModalViewModel>();

                if (canNavigate)
                {
                    var actors = SelectedFavorites.Cast<FavoritedVoiceActor>().ToList();
                    var action = new Action(() =>
                    {
                        var voiceActorCollection = App.liteDB.GetCollection<FavoritedVoiceActor>();
                        foreach (var item in actors)
                            voiceActorCollection.Delete(item.VoiceActor.MalId);

                        FavoritedActors.RemoveRange(actors);
                    });

                    await NavigationManager.NavigatePopUpAsync<ChoiceModalViewModel>(Lang.Lang.ClearFavoriteList, Lang.Lang.ClearCannotBeUndone, action);
                    SelectedFavorites = null;
                }
            }
        }

        private bool _isUpdatingVoiceActors;

        //TODO: testar comando abaixo com a funcionalidade nova de filtrar por isNSFW dos animes
        public ICommand UpdateFavoriteVoiceActorsCommand { get; private set; }
        private async Task OnUpdateFavoriteVoiceActors()
        {
            _isUpdatingVoiceActors = true;
            bool canNavigate = await NavigationManager.CanPopUpNavigateAsync<ChoiceModalViewModel>();

            if (canNavigate)
            {
                var action = new Action(async () =>
                {
                    var favoriteCollection = App.liteDB.GetCollection<FavoritedVoiceActor>().FindAll().ToList();

                    await NavigationManager
                    .NavigatePopUpAsync<ProgressPopupViewModel>(favoriteCollection, this
                    , new Action(async () => { await LoadAsync(); }));
                });

                await NavigationManager.NavigatePopUpAsync<ChoiceModalViewModel>(Lang.Lang.UpdatingVoiceActors, Lang.Lang.UpdatingVoiceActorsMessage, action);
            }
            _isUpdatingVoiceActors = false;
        }
        #endregion
    }
}
