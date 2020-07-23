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
using JikanDotNet.Exceptions;

namespace ANT.Modules
{
    public class AnimeCharacterViewModel : BaseViewModel, IAsyncInitialization
    {
        public AnimeCharacterViewModel(long characterId)
        {
            InitializeTask = LoadAsync(characterId);

            FavoriteCommand = new magno.Command(OnFavorite);
            OpenLinkCommand = new magno.AsyncCommand<string>(OpenLink);
            SelectedItemCommand = new magno.AsyncCommand(OnSelectedItem);
        }

        public Task InitializeTask { get; }

        public async Task LoadAsync(object param)
        {
            var characterId = (long)param;

            IsLoading = true;
            CanEnable = !IsLoading;

            try
            {
                FavoritedAnimeCharacter animeCharacter = App.liteDB.GetCollection<FavoritedAnimeCharacter>().Find(p => p.Character.MalId == characterId).FirstOrDefault();

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
            catch (JikanRequestException ex)
            {
                DependencyService.Get<IToast>().MakeToastMessageLong(ex.ResponseCode.ToString());

                var error = new ErrorLog()
                {
                    Exception = ex,
                    ExceptionDate = DateTime.Now,
                    ExceptionType = ex.GetType(),
                    AdditionalInfo = ex.ResponseCode.ToString(),
                };

                App.liteErrorLogDB.GetCollection<ErrorLog>().Insert(error);


            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine($"Tasks canceladas {Environment.NewLine} " +
                    $"{ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);

                DependencyService.Get<IToast>().MakeToastMessageLong(Lang.Lang.Error);

                var error = new ErrorLog()
                {
                    Exception = ex,
                    ExceptionDate = DateTime.Now,
                    ExceptionType = ex.GetType(),
                };

                App.liteErrorLogDB.GetCollection<ErrorLog>().Insert(error);
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

        private object _selectedItem;
        public object SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }
        #endregion

        #region commands
        public ICommand FavoriteCommand { get; private set; }
        public void OnFavorite()
        {
            if (CharacterContext.IsFavorited)
            {
                CharacterContext.IsFavorited = false;
                App.liteDB.GetCollection<FavoritedAnimeCharacter>().Delete(CharacterContext.Character.MalId);

                DependencyService.Get<IToast>().MakeToastMessageLong(Lang.Lang.RemovedFromFavorite);
            }
            else
            {
                CharacterContext.IsFavorited = true;
                App.liteDB.GetCollection<FavoritedAnimeCharacter>().Upsert(CharacterContext.Character.MalId, CharacterContext);

                DependencyService.Get<IToast>().MakeToastMessageLong(Lang.Lang.AddedToFavorite);
            }
        }

        public ICommand SelectedItemCommand { get; set; }
        private async Task OnSelectedItem()
        {
            if (IsNotBusy && SelectedItem != null)
            {

                IsBusy = true;
                if (SelectedItem is VoiceActorEntry voiceActorEntry)
                    await NavigationManager.NavigateShellAsync<VoiceActorViewModel>(voiceActorEntry.MalId);

                else if (SelectedItem is MALImageSubItem mALImageSubItem)
                    await NavigationManager.NavigateShellAsync<AnimeSpecsViewModel>(mALImageSubItem.MalId);

                IsBusy = false;
                SelectedItem = null;
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
