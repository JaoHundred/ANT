using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;
using magno = MvvmHelpers.Commands;
using ANT.Model;
using ANT.Interfaces;
using System.Threading.Tasks;
using System.Windows.Input;
using ANT.Core;
using ANT.UTIL;
using System.Linq;
using Xamarin.Forms;
using LiteDB;

namespace ANT.Modules
{
    public class FavoriteCharacterViewModel : BaseVMExtender
    {
        public FavoriteCharacterViewModel()
        {
            ClearAllRecentCommand = new magno.AsyncCommand(OnClearAllRecent);
            OpenCharacterCommand = new magno.AsyncCommand(OnOpenCharacter);
            DeleteFavoriteCommand = new magno.AsyncCommand(OnDeleteFavorite);
            ClearTextCommand = new magno.Command(OnClearText);
            SearchCommand = new magno.AsyncCommand(OnSearch);
            SelectionModeCommand = new magno.Command(OnSelectionMode);
            UpdateFavoriteCharactersCommand = new magno.AsyncCommand(OnUpdateFavoriteCharacters);

            FavoritedCharacters = new ObservableRangeCollection<FavoritedAnimeCharacter>();
        }

        private List<FavoritedAnimeCharacter> _originalCollection;

        public async Task LoadAsync(object param)
        {
            await Task.Run(() =>
            {
                if (_isUpdatingAnimes)
                    return;

                FavoritedCharacters.ReplaceRange(App.liteDB.GetCollection<FavoritedAnimeCharacter>().FindAll().ToList());
                _originalCollection = new List<FavoritedAnimeCharacter>(FavoritedCharacters);
            });
        }

        #region propriedades
        private ObservableRangeCollection<FavoritedAnimeCharacter> _favoritedCharacters;
        public ObservableRangeCollection<FavoritedAnimeCharacter> FavoritedCharacters
        {
            get { return _favoritedCharacters; }
            set { SetProperty(ref _favoritedCharacters, value); }
        }

        private FavoritedAnimeCharacter _selectedFavorite;
        public FavoritedAnimeCharacter SelectedFavorite
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
            if (FavoritedCharacters.Count == 0)
                return;

            bool canNavigate = await NavigationManager.CanPopUpNavigateAsync<ChoiceModalViewModel>();

            if (canNavigate)
            {
                var confirmDelegateAction = new Action(() =>
                {
                    App.liteDB.GetCollection<FavoritedAnimeCharacter>().DeleteAll();
                    FavoritedCharacters.Clear();
                });

                await NavigationManager.
                    NavigatePopUpAsync<ChoiceModalViewModel>(Lang.Lang.ClearFavoriteList, Lang.Lang.ClearCannotBeUndone, confirmDelegateAction);
            }
        }

        public ICommand OpenCharacterCommand { get; private set; }
        private async Task OnOpenCharacter()
        {
            if (IsNotBusy && SelectedFavorite != null)
            {
                IsBusy = true;
                await NavigationManager.NavigateShellAsync<AnimeCharacterViewModel>(SelectedFavorite.Character.MalId);
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
                return _originalCollection.Where(character => character.Character.Name.ToLowerInvariant().Contains(SearchQuery.ToLowerInvariant())).ToList();
            });

            var resultList = await resultListTask;
            FavoritedCharacters.ReplaceRange(resultList);
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
                    var characters = SelectedFavorites.Cast<FavoritedAnimeCharacter>().ToList();
                    var action = new Action(() =>
                   {
                       var characterCollection = App.liteDB.GetCollection<FavoritedAnimeCharacter>();
                       foreach (var item in characters)
                           characterCollection.Delete(item.Character.MalId);

                       FavoritedCharacters.RemoveRange(characters);
                   });

                    await NavigationManager.NavigatePopUpAsync<ChoiceModalViewModel>(Lang.Lang.ClearFavoriteList, Lang.Lang.ClearCannotBeUndone, action);
                    SelectedFavorites = null;
                }
            }
        }

        private bool _isUpdatingAnimes;

        //TODO: testar comando abaixo com a funcionalidade nova de filtrar por isNSFW dos animes
        public ICommand UpdateFavoriteCharactersCommand { get; private set; }
        private async Task OnUpdateFavoriteCharacters()
        {
            _isUpdatingAnimes = true;
            bool canNavigate = await NavigationManager.CanPopUpNavigateAsync<ChoiceModalViewModel>();

            if (canNavigate)
            {
                var action = new Action(async () =>
                {
                    var favoriteCollection = App.liteDB.GetCollection<FavoritedAnimeCharacter>().FindAll().ToList();

                    await NavigationManager
                    .NavigatePopUpAsync<ProgressPopupViewModel>(favoriteCollection, this
                    , new Action(async () => { await LoadAsync(null); }));
                });

                await NavigationManager.NavigatePopUpAsync<ChoiceModalViewModel>(Lang.Lang.UpdatingCharacters, Lang.Lang.UpdatingCharactersMessage, action);
            }
            _isUpdatingAnimes = false;
        }
        #endregion
    }
}
