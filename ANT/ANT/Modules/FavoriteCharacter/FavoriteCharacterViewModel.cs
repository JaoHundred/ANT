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

namespace ANT.Modules
{
    public class FavoriteCharacterViewModel : BaseViewModel
    {
        public FavoriteCharacterViewModel()
        {
            ClearAllRecentCommand = new magno.AsyncCommand(OnClearAllRecent);

            FavoritedCharacters = new ObservableRangeCollection<FavoritedAnimeCharacter>();
        }

        public async Task LoadAsync(object param)
        {
            await Task.Run(() =>
            {
                FavoritedCharacters.ReplaceRange(App.FavoritedAnimeCharacters);
            });
        }

        #region propriedades
        private ObservableRangeCollection<FavoritedAnimeCharacter> _favoritedCharacters;
        public ObservableRangeCollection<FavoritedAnimeCharacter> FavoritedCharacters
        {
            get { return _favoritedCharacters; }
            set { SetProperty(ref _favoritedCharacters, value); }
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
                var confirmDelegateAction = new Action(async () =>
                {
                    await Task.Run(() =>
                    {
                        App.FavoritedAnimeCharacters.Clear();
                        JsonStorage.SaveDataAsync(App.FavoritedAnimeCharacters, StorageConsts.LocalAppDataFolder, StorageConsts.RecentAnimesFileName);
                    });

                    FavoritedCharacters.Clear();
                });

                await NavigationManager.
                    NavigatePopUpAsync<ChoiceModalViewModel>(Lang.Lang.ClearFavoriteList, Lang.Lang.ClearCannotBeUndone, confirmDelegateAction);
            }
        } 
        #endregion

        //TODO: criar comando para deletar 1 por 1(pensar em como fazer isso, multiseleção ou seleção mais lixeira
        //TODO: personalizar o template dos personagens
        //TODO: comando de navegar para a página do personagem
    }
}
