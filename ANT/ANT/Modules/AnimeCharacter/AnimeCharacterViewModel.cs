using System;
using System.Collections.Generic;
using System.Text;
using JikanDotNet;
using MvvmHelpers;
using magno = MvvmHelpers.Commands;
using ANT.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Input;
using Xamarin.Essentials;
using ANT.Core;
using ANT.Model;
using Xamarin.Forms;

namespace ANT.Modules
{
    public class AnimeCharacterViewModel : BaseViewModel, IAsyncInitialization
    {
        public AnimeCharacterViewModel(long characterId)
        {
            InitializeTask = LoadAsync(characterId);

            FavoriteCommand = new magno.AsyncCommand(OnFavorite);
            OpenLinkCommand = new magno.AsyncCommand<string>(OpenLink);
            SelectedAnimeCommand = new magno.AsyncCommand<MALImageSubItem>(OnSelectedAnime);
            SelectedVoiceActorCommand = new magno.AsyncCommand(OnSelectedVoiceActor);
        }

        public Task InitializeTask { get; }

        public async Task LoadAsync(object param)
        {
            var characterId = (long)param;

            IsLoading = true;
            CanEnable = !IsLoading;

            try
            {
                FavoritedAnimeCharacter animeCharacter = App.FavoritedAnimeCharacters.FirstOrDefault(p => p.Character.MalId == characterId);

                if (animeCharacter == null)
                {
                    await App.DelayRequest();
                    Character character = await App.Jikan.GetCharacter(characterId);

                    await App.DelayRequest();
                    var characterPictures = await App.Jikan.GetCharacterPictures(characterId);

                    animeCharacter = new FavoritedAnimeCharacter(character, characterPictures.Pictures.ToList());
                }

                CharacterContext = animeCharacter;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                IsLoading = false;
                CanEnable = !IsLoading;
            }
        }

        #region propriedades
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        private bool _canEnable;
        public bool CanEnable
        {
            get { return _canEnable; }
            set { SetProperty(ref _canEnable, value); }
        }

        private FavoritedAnimeCharacter _characterContext;
        public FavoritedAnimeCharacter CharacterContext
        {
            get { return _characterContext; }
            set { SetProperty(ref _characterContext, value); }
        }

        private VoiceActorEntry _selectedVoiceActor;
        public VoiceActorEntry SelectedVoiceActor
        {
            get { return _selectedVoiceActor; }
            set { SetProperty(ref _selectedVoiceActor, value); }
        }
        #endregion

        #region commands
        public ICommand FavoriteCommand { get; private set; }
        public async Task OnFavorite()
        {
            if (CharacterContext.IsFavorited)
            {
                CharacterContext.IsFavorited = false;
                App.FavoritedAnimeCharacters.Remove(CharacterContext);

                DependencyService.Get<IToast>().MakeToastMessageLong(Lang.Lang.RemovedFromFavorite);
            }
            else
            {
                CharacterContext.IsFavorited = true;
                App.FavoritedAnimeCharacters.Add(CharacterContext);

                DependencyService.Get<IToast>().MakeToastMessageLong(Lang.Lang.AddedToFavorite);
            }

            await JsonStorage.SaveDataAsync(App.FavoritedAnimeCharacters, StorageConsts.LocalAppDataFolder, StorageConsts.FavoritedAnimesCharacterFileName);
        }

        public ICommand SelectedAnimeCommand { get; private set; }
        private async Task OnSelectedAnime(MALImageSubItem item)
        {
            if (IsNotBusy)
            {
                IsBusy = true;
                NavigationManager.RemoveAllPagesExceptRootAndHierarquicalRoot();
                await NavigationManager.NavigateShellAsync<AnimeSpecsViewModel>(item.MalId);
                IsBusy = false;
            }
        }

        public ICommand SelectedVoiceActorCommand { get; set; }
        private async Task OnSelectedVoiceActor()
        {
            if (IsNotBusy && SelectedVoiceActor != null)
            {
                IsBusy = true;
                await NavigationManager.NavigateShellAsync<VoiceActorViewModel>(SelectedVoiceActor.MalId);
                IsBusy = false;
                SelectedVoiceActor = null;
            }
        }

        public ICommand OpenLinkCommand { get; private set; }
        private async Task OpenLink(string link)
        {
            await Launcher.TryOpenAsync(link);
        }

        #endregion
    }
}
