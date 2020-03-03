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

namespace ANT.Modules
{
    public class VoiceActorViewModel : BaseViewModel, IAsyncInitialization
    {
        public VoiceActorViewModel(long malId)
        {
            InitializeTask = LoadAsync(malId);

            OpenImageCommand = new magno.AsyncCommand<Picture>(OnOpenImage);
            FavoriteCommand = new magno.AsyncCommand(OnFavorite);
        }

        //TODO: a view não consegue ser construída e gera uma exceção que não pode ser tratada ou vista, comentar a tela toda de VoiceActorView
        // para descobrir o que é(o código dela foi copiado e ajustado de AnimeCharacterView)

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

                //TODO: construir o data template da collectionview para carregar papeis de dublador,
                //usar formato semelhante ao que está no MAL https://github.com/JaoHundred/ANT/issues/27
                //o template deve ter comandos para navegar para o anime ou para o personagem(ambos devem estar em um mesmo template de anime)
                //pensar o que fazer se existir mais de um personagem dublado por um mesmo ator em um mesmo anime(vai dar uma situação de 1 template
                //para 1 registro de anime e 1 ou mais personagens deste anime) e ver como a api do jikan lida com isso
                //aparentemente ele repete o registro do anime, voltando assim para 1 template que segura somente 1 anime e 1 personagem
                //ver o exemplo em https://myanimelist.net/people/2/Tomokazu_Sugita no final da lista
                //nos animes com letra Z

                //TODO: implementar comando de abrir página para voice actor
                // na tela dos atores usar a combinação de GetPerson e https://github.com/Ervie/jikan.net/wiki/Person e https://github.com/Ervie/jikan.net/wiki/VoiceActingRole
                // para pegar as informações referentes a animes e personagens que essa pessoa trabalhou
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
        public ICommand OpenImageCommand { get; private set; }
        private async Task OnOpenImage(Picture picture)
        {
            await Launcher.TryOpenAsync(picture.Large);
        }

        public ICommand FavoriteCommand { get; private set; }
        private async Task OnFavorite()
        {
            await Task.Delay(TimeSpan.FromSeconds(4));
            //TODO: salvar o voice actor na lista de favoritos, se já estiver favoritado, desfavoritar
        } 
        #endregion

       
    }
}
