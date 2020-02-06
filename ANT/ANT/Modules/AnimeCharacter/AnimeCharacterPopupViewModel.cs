using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;
using magno = MvvmHelpers.Commands;
using ANT.Interfaces;
using System.Threading.Tasks;
using JikanDotNet;
using System.Windows.Input;
using ANT.Core;

namespace ANT.Modules
{
    public class AnimeCharacterPopupViewModel : BaseViewModel, IAsyncInitialization
    {
        public AnimeCharacterPopupViewModel(long malId)
        {
            Characters = new ObservableRangeCollection<CharacterEntry>();

            InitializeTask = LoadAsync(malId);

            OpenAnimeCharacterCommand = new magno.AsyncCommand<CharacterEntry>(OnOpenAnimeCharacter);
        }

        public Task InitializeTask { get; }

        public async Task LoadAsync(object param)
        {
            try
            {
                long animeId = (long)param;
                IsLoading = true;
                await Task.Delay(TimeSpan.FromSeconds(4));
                AnimeCharactersStaff animeCharactersStaff = await App.Jikan.GetAnimeCharactersStaff(animeId);
                var characterEntrys = animeCharactersStaff.Characters;

                Characters.AddRange(characterEntrys);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #region propriedades

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }


        private ObservableRangeCollection<CharacterEntry> _characters;
        public ObservableRangeCollection<CharacterEntry> Characters
        {
            get { return _characters; }
            set { SetProperty(ref _characters, value); }
        }
        #endregion

        #region commands
        public ICommand OpenAnimeCharacterCommand { get; private set; }
        private async Task OnOpenAnimeCharacter(CharacterEntry characterEntry)
        {
            bool canNavigate = await NavigationManager.CanShellNavigateAsync<AnimeCharacterViewModel>();

            if (canNavigate)
            {
                await NavigationManager.PopPopUpPageAsync();
                await NavigationManager.NavigateShellAsync<AnimeCharacterViewModel>(characterEntry);
            }
        }
        #endregion
    }
}
