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

namespace ANT.Modules
{
    public class VoiceActorViewModel : BaseViewModel, IAsyncInitialization
    {
        public VoiceActorViewModel(long malId)
        {
            InitializeTask = LoadAsync(malId);

            FavoriteCommand = new magno.AsyncCommand(OnFavorite);
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
                await Task.Delay(TimeSpan.FromSeconds(4));
                Person person = await App.Jikan.GetPerson(id);

                await Task.Delay(TimeSpan.FromSeconds(4));
                PersonPictures personPictures = await App.Jikan.GetPersonPictures(id);

                PersonContext = person;
                PersonPictures = personPictures.Pictures;
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
        private Person _personContext;
        public Person PersonContext
        {
            get { return _personContext; }
            set { SetProperty(ref _personContext, value); }
        }

        private ICollection<Picture> _personPictures;
        public ICollection<Picture> PersonPictures
        {
            get { return _personPictures; }
            set { SetProperty(ref _personPictures, value); }
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
        private async Task OnFavorite()
        {
            await Task.Delay(TimeSpan.FromSeconds(4));
            //TODO: salvar o voice actor na lista de favoritos, se já estiver favoritado, desfavoritar
        }

        public ICommand SelectedCharacterCommand { get; private set; }
        private async Task OnSelectedCharacter(MALImageSubItem selectedCharacter)
        {
            if(IsNotBusy)
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
