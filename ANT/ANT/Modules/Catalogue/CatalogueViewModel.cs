using System;
using System.Collections.Generic;
using System.Text;
using JikanDotNet;
using JikanDotNet.Helpers;
using Xamarin.Forms;
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
        private IJikan _jikan;

        public CatalogueViewModel()
        {
            _jikan = new Jikan(useHttps: true);
            InitializeTask = LoadAync();
        }

        //TODO: usar data trigger(?) para verificar se a imagem terminou de carregar, se não, exibir um activity indicator
        //TODO: tentar ver outro tamanho para o "ícone" de X na pesquisa, testar entre 21 e 24 no tamanho da fonte
        public Task InitializeTask { get; }
        public async Task LoadAync()
        {
            IsLoading = true;
            IsLoadingOrRefreshing = IsLoading || IsRefreshing;
            await LoadCatalogueAsync();
            IsLoading = false;
            IsLoadingOrRefreshing = IsLoading || IsRefreshing;
        }

        #region proriedades

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { Changed(ref _isLoading, value); }
        }

        private bool _isLoadingOrRefreshing;
        public bool IsLoadingOrRefreshing
        {
            get { return _isLoadingOrRefreshing; }
            set { Changed(ref _isLoadingOrRefreshing, value); }
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

        private IList<AnimeSubEntry> _originalCollection;
        private IList<AnimeSubEntry> _animes;
        public IList<AnimeSubEntry> Animes
        {
            get { return _animes; }
            set { Changed(ref _animes, value); }
        }

        private string _searchQuery;
        public string SearchQuery
        {
            get { return _searchQuery; }
            set { Changed(ref _searchQuery, value); }
        }

        #endregion

        #region métodos da VM
        private async Task LoadCatalogueAsync()
        {
            if (SearchQuery?.Length > 0)
                ClearTextQuery();

            var results = await _jikan.GetSeason();
            results.RequestCached = true;
            Animes = _originalCollection = results.SeasonEntries.ToList();
        }

        private void ClearTextQuery()
        {
            SearchQuery = string.Empty;
        }
        #endregion

        #region commands

        public Command<IList<object>> SelectItemsCommand => new Command<IList<object>>
            ((IList<object> selectedItems) =>
        {
            var items = selectedItems.Cast<AnimeSubEntry>();

            //TODO: ver como adicionar menus via o toolbar para esta tela, opções de modo de seleção, adicionar aos favoritos e deletar dos favoritos se já existir
        });

        public Command RefreshCommand => new Command(async () =>
        {
            IsLoadingOrRefreshing = IsLoading || IsRefreshing;
            await LoadCatalogueAsync();
            IsRefreshing = false;
            IsLoadingOrRefreshing = IsLoading || IsRefreshing;
        });

        public Command ClearTextCommand => new Command(() =>
        {
            ClearTextQuery();
            SearchCommand.Execute(null);
        });

        public Command SearchCommand => new Command(() =>
        {
            Animes = _originalCollection.Where(p => p.Title.ToLowerInvariant().StartsWith(SearchQuery.ToLowerInvariant())).ToList();
        });

        #endregion
    }
}
