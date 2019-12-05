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
using System.Windows.Input;
using ANT.UTIL;

namespace ANT.Modules
{
    public class CatalogueViewModel : BaseVMSelectionModeExtender, IAsyncInitialization
    {
        public CatalogueViewModel()
        {
            _jikan = new Jikan(useHttps: true);
            InitializeTask = LoadAync();

            Animes = new ObservableRangeCollection<AnimeSubEntry>();
            _mainPageAndroid = DependencyService.Get<IMainPageAndroid>();
            _mainPageAndroid.OnBackPress(this);
        }

        private IJikan _jikan;
        private IMainPageAndroid _mainPageAndroid;

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

        

        private AnimeSubEntry _selectedItem;
        public AnimeSubEntry SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
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
                //TODO: temporário criar meios de filtros especializados no futuro
                _originalCollection = results.SeasonEntries.Where(
                    anime => anime.R18 == false &&
                    anime.HasAllSpecifiedGenres(GenreSearch.Ecchi) == false
                    ).ToList();

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


        public ICommand SelectionModeCommand => new Command(() =>
        {
            if (SelectionMode == SelectionMode.Multiple)
                SingleSelectionMode();
            else
            {
                MultiSelectionMode();
                SelectedItems = null;
            }
        });

        public ICommand AddToFavoriteCommand => new Command(() =>
        {

            if (SelectedItems == null || SelectedItems.Count == 0)
                return;

            var items = SelectedItems.Cast<AnimeSubEntry>().ToList();

            //TODO: pensar em um sistema de save para guardar favoritos do usuário
        });

        public ICommand RefreshCommand => new Command(async () =>
        {
            IsLoadingOrRefreshing = IsLoading || IsRefreshing;
            await LoadCatalogueAsync();
            IsRefreshing = false;
            IsLoadingOrRefreshing = IsLoading || IsRefreshing;
        });

        public ICommand ClearTextCommand => new Command(() =>
        {
            ClearTextQuery();
            SearchCommand.Execute(null);
        });

        public ICommand SearchCommand => new Command(async () =>
        {
            IList<AnimeSubEntry> resultList = await Task.Run(() =>
           {
               return _originalCollection.Where(anime => anime.Title.ToLowerInvariant().Contains(SearchQuery.ToLowerInvariant())).ToList();
           });

            Animes.ReplaceRange(resultList);
        });

        public ICommand OpenAnimeCommand => new Command(async () =>
        {
            if (!IsMultiSelect && SelectedItem != null)
            {
                await App.Current.MainPage.Navigation.PushAsync(new AnimeSpecsView(SelectedItem));
                SelectedItem = null;
            }
        });

        #endregion
    }
}
