using System;
using System.Collections.Generic;
using System.Text;
using JikanDotNet;
using JikanDotNet.Helpers;
using Xamarin.Forms;
using System.Linq;
using System.Threading.Tasks;
using ANT.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
using MvvmHelpers;

namespace ANT.Modules
{
    public class CatalogueViewModel : BaseViewModel, IAsyncInitialization
    {
        private IJikan _jikan;

        public CatalogueViewModel()
        {
            _jikan = new Jikan(useHttps: true);
            InitializeTask = LoadAync();

            Animes = new ObservableRangeCollection<AnimeSubEntry>();
        }

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
            set { SetProperty(ref _isLoading, value); }
        }

        private bool _isLoadingOrRefreshing;
        public bool IsLoadingOrRefreshing
        {
            get { return _isLoadingOrRefreshing; }
            set { SetProperty(ref _isLoadingOrRefreshing, value); }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set { SetProperty(ref _isRefreshing, value); }
        }

        private bool _isMultiSelect;
        public bool IsMultiSelect
        {
            get { return _isMultiSelect; }
            set { SetProperty(ref _isMultiSelect, value); }
        }

        private Xamarin.Forms.SelectionMode _selectionMode;
        public Xamarin.Forms.SelectionMode SelectionMode
        {
            get { return _selectionMode; }
            set { SetProperty(ref _selectionMode, value); }
        }

        private IList<object> _selectedItems;
        public IList<object> SelectedItems
        {
            get { return _selectedItems; }
            set { SetProperty(ref _selectedItems, value); }
        }

        private IList<AnimeSubEntry> _originalCollection;
        private ObservableRangeCollection<AnimeSubEntry> _animes;
        public ObservableRangeCollection<AnimeSubEntry> Animes
        {
            get { return _animes; }
            set { SetProperty(ref _animes, value); }
        }

        private string _searchQuery;
        public string SearchQuery
        {
            get { return _searchQuery; }
            set { SetProperty(ref _searchQuery, value); }
        }

        #endregion

        #region métodos da VM
        private async Task LoadCatalogueAsync()
        {
            if (SearchQuery?.Length > 0)
                ClearTextQuery();

            try
            {
                var results = await _jikan.GetSeason();
                results.RequestCached = true;
                _originalCollection = results.SeasonEntries.ToList();
                Animes.ReplaceRange(_originalCollection);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //TODO:capturar aqui possíveis erros de conexão
            }
        }

        private void ClearTextQuery() => SearchQuery = string.Empty;
        #endregion

        #region commands


        public Command SelectionModeCommand => new Command(() =>
        {
            SelectionMode = SelectionMode != SelectionMode.Multiple ? SelectionMode.Multiple : SelectionMode.None;
            IsMultiSelect = SelectionMode == SelectionMode.Multiple;

            if (SelectionMode != SelectionMode.Multiple)
                SelectedItems = null;
        });

        public Command AddToFavoriteCommand => new Command(() =>
        {
            //TODO: mudar o texto do toolbaritem entre "multi seleção" ou " multi seleção desligada"(pensar se esse é um nome bom)
            //TODO: pensar o que fazer com o botão de favoritos se o usuário estiver no fim da lista E com um registro de anime logo atrás do botão

            if (SelectedItems?.Count == 0)
                return;

            foreach (var item in SelectedItems)
            {

            }

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

        public Command SearchCommand => new Command(async () =>
        {
            IList<AnimeSubEntry> resultList = await Task.Run(() =>
           {
               return _originalCollection.Where(anime => anime.Title.ToLowerInvariant().Contains(SearchQuery.ToLowerInvariant())).ToList();
           });

            Animes.ReplaceRange(resultList);
        });

        #endregion
    }
}
