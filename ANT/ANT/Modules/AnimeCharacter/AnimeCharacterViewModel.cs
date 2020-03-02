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

namespace ANT.Modules
{
    public class AnimeCharacterViewModel : BaseViewModel, IAsyncInitialization
    {
        public AnimeCharacterViewModel(long characterId)
        {
            InitializeTask = LoadAsync(characterId);

            FavoriteCommand = new magno.AsyncCommand(OnFavorite);
            OpenImageCommand = new magno.AsyncCommand<Picture>(OnOpenImage);
            SelectedAnimeCommand = new magno.AsyncCommand<MALImageSubItem>(OnSelectedAnime);
            SelectedSeiyuuCommand = new magno.AsyncCommand<VoiceActorEntry>(OnSelectedSeiyuu);
        }

        public Task InitializeTask { get; }

        public async Task LoadAsync(object param)
        {
            var characterId = (long)param;

            IsLoading = true;
            CanEnable = !IsLoading;
            
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(4));
                Character character = await App.Jikan.GetCharacter(characterId);

                await Task.Delay(TimeSpan.FromSeconds(4));
                var characterPictures = await App.Jikan.GetCharacterPictures(characterId);

                CharacterContext = character;
                CharacterPictures = characterPictures.Pictures.ToList();
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

        private Character _characterContext;
        public Character CharacterContext
        {
            get { return _characterContext; }
            set { SetProperty(ref _characterContext, value); }
        }

        private IList<Picture> _characterPictures;
        public IList<Picture> CharacterPictures
        {
            get { return _characterPictures; }
            set { SetProperty(ref _characterPictures, value); }
        }
        #endregion

        #region commands
        public ICommand FavoriteCommand { get; private set; }
        public async Task OnFavorite()
        {
            await Task.Delay(TimeSpan.FromSeconds(4));
            //TODO: salvar o personagem na lista de favoritos, se já estiver favoritado, desfavoritar
        }

        public ICommand OpenImageCommand { get; private set; }
        private async Task OnOpenImage(Picture picture)
        {
            await Launcher.TryOpenAsync(picture.Large);
        }

        public ICommand SelectedAnimeCommand { get; private set; }
        private async Task OnSelectedAnime(MALImageSubItem item)
        {
            if(IsNotBusy)
            {
                IsBusy = true;
                NavigationManager.RemoveAllPagesExceptRootAndHierarquicalRoot();
                await NavigationManager.NavigateShellAsync<AnimeSpecsViewModel>(item.MalId);
                IsBusy = false;
            }
        }

        public ICommand SelectedSeiyuuCommand { get; set; }
        private async Task OnSelectedSeiyuu(VoiceActorEntry voiceActorEntry)
        {
            //TODO: passar o MalId para o navigation da tela dos seiyuu
        }

        #endregion


        //TODO: implementar comando de abrir página para voice actor
        // na tela dos atores usar a combinação de GetPerson e https://github.com/Ervie/jikan.net/wiki/Person e https://github.com/Ervie/jikan.net/wiki/VoiceActingRole
        // para pegar as informações referentes a animes e personagens que essa pessoa trabalhou

        //TODO: https://github.com/JaoHundred/ANT/issues/20
        //implementar a parte dos Voice Actors na tela
    }
}
