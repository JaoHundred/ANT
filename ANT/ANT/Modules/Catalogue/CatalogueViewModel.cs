using System;
using System.Collections.Generic;
using System.Text;
using JikanDotNet;
using JikanDotNet.Helpers;
using Xamarin.Forms;
using ANT._Services;
using System.Linq;
using ANT.UTIL;
using System.Threading.Tasks;
using ANT.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ANT.Modules
{
    public class CatalogueViewModel : NotifyProperty, IAsyncInitialization
    {
        public CatalogueViewModel()
        {
            InitializeTask = LoadAync();

            SelectItemsCommand = new Command<IList<object>>(OnSelectItems);
            RefreshCommand = new Command(OnRefreshCatalogue);
        }

        public Task InitializeTask { get; }
        public async Task LoadAync()
        {
            IsLoading = true;
            await LoadCatalogueAsync();
            IsLoading = false;
        }

        #region proriedades

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { Changed(ref _isLoading, value); }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set { Changed(ref _isRefreshing, value); }
        }

        private Xamarin.Forms.SelectionMode _selectionMode;
        public Xamarin.Forms.SelectionMode SelectionMode
        {
            get { return _selectionMode; }
            set { Changed(ref _selectionMode, value); }
        }

        private IList<AnimeSubEntry> _animes;
        public IList<AnimeSubEntry> Animes
        {
            get { return _animes; }
            set { Changed(ref _animes, value); }
        }

        #endregion

        #region métodos da VM
        private async Task LoadCatalogueAsync()
        {
            AnimeGenre genre = await JikanMALService.GetCatalogueByGenreAsync(GenreSearch.SciFi);
            Animes = genre.Anime.Take(300).ToList();
        } 
        #endregion

        #region commands

        public Command<IList<object>> SelectItemsCommand { get; }
        private void OnSelectItems(IList<object> selectedItems)
        {
            var items = selectedItems.Cast<AnimeSubEntry>();

            //TODO: não tem como personalizar o toolbar(cor, outros controles além de texto, esconder)
            //TODO: ver como adicionar menus via o toolbar para esta tela, opções de modo de seleção, adicionar aos favoritos e deletar dos favoritos se já existir
        }

        public Command RefreshCommand { get; }
        private async void OnRefreshCatalogue(object obj)
        {
            await LoadCatalogueAsync();
            IsRefreshing = false;
        }

        #endregion
    }
}
