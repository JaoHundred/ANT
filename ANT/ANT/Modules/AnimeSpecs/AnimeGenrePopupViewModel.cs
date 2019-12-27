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
            bool canNavigate = await NavigationManager.CanShellNavigateAsync<CatalogueView>();

            if (canNavigate)
            {
                string formatedString = await RemoveOcurrencesFromStringAsync(genreName.ToString(), new char[] { '-', ' ' });
                GenreSearch genre = (GenreSearch)Enum.Parse(typeof(GenreSearch), formatedString, true);

                await NavigationManager.PopPopUpPageAsync();
                //NavigationManager.RemovePageFromShellStack<CatalogueViewModel>();//remove a página de catálogo antigo
                await NavigationManager.NavigateShellAsync<CatalogueViewModel>(genre);
            }
        });

        //já que o trim não funcionava, fiz meu próprio formatador de string que remove caracteres escolhidos
        private Task<string> RemoveOcurrencesFromStringAsync(string originalString, params char[] ocurrences)
        {
            return Task.Run(() =>
            {
                var builder = new StringBuilder();
                string stringList = originalString;

                for (int i = 0; i < stringList.Length; i++)
                {
                    char c = stringList[i];

                    if (!ocurrences.Contains(c))
                        builder.Append(c);
                }

                return builder.ToString();
            });
        }
    }
}
