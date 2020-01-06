using JikanDotNet;
using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;
using ANT.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Essentials;
using ANT.Core;
using ANT.UTIL;
using magno = MvvmHelpers.Commands;

namespace ANT.Modules
{
    public class AnimeSpecsViewModel : BaseVMExtender, IAsyncInitialization
    {
        public AnimeSpecsViewModel(long malID)
        {
            InitializeTask = LoadAsync(malID);

            FavoriteCommand = new magno.Command(OnFavorite);
            OpenImageInBrowserCommand = new magno.AsyncCommand(OnOpenImageInBrowser);
            CheckAnimeGenresCommand = new magno.AsyncCommand(OnCheckAnimeGenres);
            CheckAnimeCharactersCommand = new magno.AsyncCommand(OnCheckAnimeCharacters);
            OpenAnimeInBrowserCommand = new magno.AsyncCommand(OnOpenAnimeInBrowser);
            DiscussionsCommand = new magno.AsyncCommand<string>(OnDiscussions);
        }

        public Task InitializeTask { get; }
        public async Task LoadAsync(object param)
        {
            long id = (long)param;

            IsLoading = true;
            CanEnable = false;
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                AnimeEpisodes episodes = await App.Jikan.GetAnimeEpisodes(id);
                episodes.RequestCached = true;
                Episodes = episodes.EpisodeCollection.ToList();

                await Task.Delay(TimeSpan.FromSeconds(4));

                Anime anime = await App.Jikan.GetAnime(id);
                anime.RequestCached = true;
                AnimeContext = anime;

                IsLoading = false;
                CanEnable = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Problema encontrado em :{ex.Message}");
            }
        }

        #region properties

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        private Anime _animeContext;
        public Anime AnimeContext
        {
            get { return _animeContext; }
            set { SetProperty(ref _animeContext, value); }
        }

        private bool _canEnable;
        public bool CanEnable
        {
            get { return _canEnable; }
            set { SetProperty(ref _canEnable, value); }
        }


        private IList<AnimeEpisode> _episodes;
        public IList<AnimeEpisode> Episodes
        {
            get { return _episodes; }
            set { SetProperty(ref _episodes, value); }
        }
        #endregion

        #region commands

        public ICommand FavoriteCommand { get; private set; }
        private void OnFavorite ()
        {
            //TODO:implementar classe modelo e serviço de favoritar, ela deve ter animecontext e toda a parte referente a episódios, quando criar a classe
            //chamar o serviço de salvar em json, se já estiver favoritado e o usuário clicar novamente, o save é apagado e o botão volta a ficar na cor padrão
            //devo precisar de um datatemplate selector pra lidar com a aparência do botão de favoritar quando o show estiver favoritado ou não

        }

        public ICommand OpenImageInBrowserCommand { get; private set; }
        private async Task OnOpenImageInBrowser()
        {
            await Launcher.TryOpenAsync(AnimeContext.ImageURL);
        }


        public ICommand CheckAnimeGenresCommand { get; private set; }
        private async Task OnCheckAnimeGenres ()
        {
            bool canNavigate = await NavigationManager.CanPopUpNavigateAsync<AnimeGenrePopupView>();

            if (canNavigate)
                await NavigationManager.NavigatePopUpAsync<AnimeGenrePopupViewModel>(AnimeContext.Genres.ToList());
        }

        public ICommand CheckAnimeCharactersCommand { get; private set; }
        private async Task OnCheckAnimeCharacters ()
        {
           //TODO: implementar, quando tiver a view para os personagens, linkar aqui

        }

        public ICommand OpenAnimeInBrowserCommand { get; private set; }
        private async Task OnOpenAnimeInBrowser()
        {
            await Launcher.TryOpenAsync(AnimeContext.LinkCanonical);
        }

        public ICommand DiscussionsCommand { get; private set; }
        private async Task OnDiscussions(string forumLink)
        {
            await Launcher.TryOpenAsync(forumLink);
        }

        #endregion

        //TODO:descobrir como tirar a sombra/linha do navigation bar para esta página(deixar pro futuro)
        //TODO: estilizar o template dos episódios, ver quais dados eu posso exibir a cerca dos episódios e ver o que mostrar
        //TODO: trocar de idioma via configurações do android nesta página resulta em uma exception de fragment, provavel de estar relacionado ao tabbedpage
    }
}
