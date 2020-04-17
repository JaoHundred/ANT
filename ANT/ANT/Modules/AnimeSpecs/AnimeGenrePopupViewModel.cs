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
using magno = MvvmHelpers.Commands;

namespace ANT.Modules
{
    public class AnimeGenrePopupViewModel : BaseVMExtender, IAsyncInitialization
    {
        public AnimeGenrePopupViewModel(IList<MALSubItem> animeGenres)
        {
            InitializeTask = LoadAsync(animeGenres);

            GenreSearchCommand = new magno.AsyncCommand<string>(OnGenreSearch);
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

        #region comandos
        public ICommand GenreSearchCommand { get; private set; }
        public async Task OnGenreSearch(string genreName)
        {
            if (IsNotBusy)
            {
                IsBusy = true;
                string formatedString = await genreName.RemoveOcurrencesFromStringAsync(new char[] { '-', ' ' });
                GenreSearch genre = (GenreSearch)Enum.Parse(typeof(GenreSearch), formatedString, true);

                await NavigationManager.PopPopUpPageAsync();
                NavigationManager.RemoveAllPagesExceptRootAndHierarquicalRoot();
                await NavigationManager.NavigateShellAsync<CatalogueViewModel>(genre);

                IsBusy = false;
            }
        }
        #endregion
    }
}
