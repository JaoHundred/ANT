using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;
using JikanDotNet;
using ANT.Interfaces;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using ANT.UTIL;
using ANT.Core;
using System.Linq;

namespace ANT.Modules
{
    public class AnimeGenrePopupViewModel : BaseVMExtender, IAsyncInitialization
    {
        public AnimeGenrePopupViewModel(IList<MALSubItem> animeGenres)
        {
            InitializeTask = LoadAsync(animeGenres);
        }

        public Task InitializeTask { get; }
        public Task LoadAsync(object param)
        {
            return Task.Run(() => { AnimeGenres = (IList<MALSubItem>)param; });
        }


        private IList<MALSubItem> _animeGenres;
        public IList<MALSubItem> AnimeGenres
        {
            get { return _animeGenres; }
            set { SetProperty(ref _animeGenres, value); }
        }

        public ICommand GenreSearchCommand => new Command<object>(async (object genreName) =>
        {
            //TODO: carregar aqui o CatalogueView passando como parâmetro o ID do gênero clicado(a nova coleção carregada em CatalogueView deve ter animes com o gênero clicado no popup)

            bool canNavigate = await NavigationManager.CanShellNavigateAsync<CatalogueView>();


            if (canNavigate)
            {

                GenreSearch genre = (GenreSearch)Enum.Parse(typeof(GenreSearch), genreName.ToString(), true);

                //TODO: trocar método de navegação abaixo por um que limpe a pilha de navegação(semelhante ao gotoAsync com route)
                await NavigationManager.NavigateShellAsync<CatalogueViewModel>(genre);
            }
        });


    }
}
