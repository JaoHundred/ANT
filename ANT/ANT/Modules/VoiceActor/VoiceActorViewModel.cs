using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;
using ANT.Interfaces;
using System.Threading.Tasks;
using JikanDotNet;
using System.Windows.Input;
using Xamarin.Essentials;
using magno = MvvmHelpers.Commands;
using ANT.Core;
using ANT.Model;
using System.Linq;
using Xamarin.Forms;
using JikanDotNet.Exceptions;

namespace ANT.Modules
{
    public class VoiceActorViewModel : BaseViewModel, IAsyncInitialization
    {
        public VoiceActorViewModel(long malId)
        {
            InitializeTask = LoadAsync(malId);

            FavoriteCommand = new magno.Command(OnFavorite);
            SelectedAnimeCommand = new magno.AsyncCommand<MALImageSubItem>(OnSelectedAnime);
            SelectedCharacterCommand = new magno.AsyncCommand<MALImageSubItem>(OnSelectedCharacter);
            OpenLinkCommand = new magno.AsyncCommand<string>(OpenLink);
        }

        public Task InitializeTask { get; }
        public async Task LoadAsync(object param)
        {
            long id = (long)param;

            IsLoading = true;
            CanEnable = !IsLoading;

            try
            {
                var favoritedVoiceActor = App.liteDB.GetCollection<FavoritedVoiceActor>().FindById(id);

                if (favoritedVoiceActor == null)
                {
                    await App.DelayRequest();
                    Person person = await App.Jikan.GetPerson(id);

                    await App.DelayRequest();
                    PersonPictures personPictures = await App.Jikan.GetPersonPictures(id);

                    favoritedVoiceActor = new FavoritedVoiceActor(person, personPictures.Pictures.ToList());
                }

                PersonContext = favoritedVoiceActor;
            }
            catch(JikanRequestException ex)
            {
                Console.WriteLine($"problema encontrado em : {ex.ResponseCode}");
                DependencyService.Get<IToast>().MakeToastMessageLong(ex.ResponseCode.ToString());

                var error = new ErrorLog()
                {
                    AdditionalInfo = ex.ResponseCode.ToString(),
                    Exception = ex,
                    ExceptionDate = DateTime.Now,
                    ExceptionType = ex.GetType(),
                };

                App.liteErrorLogDB.GetCollection<ErrorLog>().Insert(error);
            }
            catch(OperationCanceledException ex)
            { }
            catch (Exception ex)
            {
                Console.WriteLine($"problema encontrado em : {ex.Message}");
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
        private FavoritedVoiceActor _personContext;
        public FavoritedVoiceActor PersonContext
        {
            get { return _personContext; }
            set { SetProperty(ref _personContext, value); }
        }

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
        #endregion

        #region commands
        public ICommand FavoriteCommand { get; private set; }
        private void OnFavorite()
        {
            if(PersonContext.IsFavorited)
            {
                PersonContext.IsFavorited = false;
                App.liteDB.GetCollection<FavoritedVoiceActor>().Delete(PersonContext.VoiceActor.MalId);
                DependencyService.Get<IToast>().MakeToastMessageLong(Lang.Lang.RemovedFromFavorite);
            }
            else
            {
                PersonContext.IsFavorited = true;
                App.liteDB.GetCollection<FavoritedVoiceActor>().Upsert(PersonContext.VoiceActor.MalId, PersonContext);
                DependencyService.Get<IToast>().MakeToastMessageLong(Lang.Lang.AddedToFavorite);
            }
        }

        public ICommand SelectedCharacterCommand { get; private set; }
        private async Task OnSelectedCharacter(MALImageSubItem selectedCharacter)
        {
            if (IsNotBusy)
            {
                IsBusy = true;
                await NavigationManager.NavigateShellAsync<AnimeCharacterViewModel>(selectedCharacter.MalId);
                IsBusy = false;
            }
        }

        public ICommand SelectedAnimeCommand { get; private set; }
        private async Task OnSelectedAnime(MALImageSubItem selectedAnime)
        {
            if (IsNotBusy)
            {
                IsBusy = true;
                await NavigationManager.NavigateShellAsync<AnimeSpecsViewModel>(selectedAnime.MalId);
                IsBusy = false;
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
