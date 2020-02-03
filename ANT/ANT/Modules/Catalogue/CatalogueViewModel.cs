﻿using System;
using System.Collections.Generic;
using System.Text;
using JikanDotNet;
using JikanDotNet.Helpers;
using Xamarin.Forms;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using ANT.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
using MvvmHelpers;
using System.Windows.Input;
using ANT.UTIL;
using ANT.Core;
using magno = MvvmHelpers.Commands;
using ANT.Model;

namespace ANT.Modules
{
    public class CatalogueViewModel : BaseVMExtender, IAsyncInitialization
    {
        public CatalogueViewModel()
        {
            InitializeDefaultProperties();

            InitializeTask = LoadAsync(null);
        }

        public CatalogueViewModel(GenreSearch genreEnum)
        {
            _currentGenre = genreEnum;

            InitializeDefaultProperties();

            InitializeTask = LoadAsync(null);
        }

        private void InitializeDefaultProperties()
        {
            _originalCollection = new List<FavoritedAnimeSubEntry>();
            Animes = new ObservableRangeCollection<FavoritedAnimeSubEntry>();

            SelectionModeCommand = new magno.Command(OnSelectionMode);
            AddToFavoriteCommand = new magno.AsyncCommand(OnAddToFavorite);
            RefreshCommand = new magno.AsyncCommand(OnRefresh);
            ClearTextCommand = new magno.Command(OnClearText);
            SearchCommand = new magno.AsyncCommand(OnSearch);
            OpenAnimeCommand = new magno.AsyncCommand(OnOpenAnime);
            LoadMoreCommand = new magno.AsyncCommand(OnLoadMore);
        }

        private IMainPageAndroid _mainPageAndroid;
        private int _pageCount = 1;
        private readonly GenreSearch? _currentGenre;
        private readonly SemaphoreSlim loc = new SemaphoreSlim(1);
        private bool _firstLoad = true;

        public Task InitializeTask { get; }
        public async Task LoadAsync(object param)
        {
            IsLoadingOrRefreshing = IsLoading = true;

            await LoadCatalogueAsync();

            IsLoadingOrRefreshing = IsLoading = false;

            _mainPageAndroid = DependencyService.Get<IMainPageAndroid>();
            _mainPageAndroid.OnBackPress(this);
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
        private FavoritedAnimeSubEntry _selectedItem;
        public FavoritedAnimeSubEntry SelectedItem
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

        private List<FavoritedAnimeSubEntry> _originalCollection;
        private ObservableRangeCollection<FavoritedAnimeSubEntry> _animes;
        public ObservableRangeCollection<FavoritedAnimeSubEntry> Animes
        {
            get { return _animes; }
            set { SetProperty(ref _animes, value); }
        }

        private long _remainingAnimeCount;

        public long RemainingAnimeCount
        {
            get { return _remainingAnimeCount; }
            set { SetProperty(ref _remainingAnimeCount, value); }
        }

        #endregion

        #region métodos da VM
        private async Task LoadCatalogueAsync()
        {
            if (SearchQuery?.Length > 0)
                ClearTextQuery();

            try
            {
                if (_currentGenre != null)
                {
                    IsBusy = false;
                    await OnLoadMore();
                }
                else
                {
                    var results = await App.Jikan.GetSeason();
                    results.RequestCached = true;
                    //TODO: temporário criar meios de filtros especializados no futuro, possivelmente por uma outra view e viewmodel 
                    //que seleciona os filtros e repassa para cá
                    /*
                     * .Where(
                        anime => anime.R18 == false &&
                        anime.HasAllSpecifiedGenres(GenreSearch.Ecchi) == false
                        )
                    */


                    var favoritedEntries = results.SeasonEntries.ConvertAnimesToFavoritedSubEntry();
                    _originalCollection = favoritedEntries.ToList();
                    Animes.ReplaceRange(_originalCollection);
                }

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

        public ICommand LoadMoreCommand { get; private set; }
        private async Task OnLoadMore()
        {


            if (_currentGenre == null || SearchQuery?.Length > 0 || RemainingAnimeCount < 0 || IsBusy)
                return;

            // semáforo, usado para permitir que somente um apanhado de thread/task passe por vez
            //parece ser um lock melhorado
            await loc.WaitAsync();

            if (!_firstLoad)
                IsLoadingOrRefreshing = IsBusy = true;

            try
            {
                //TODO: conflitos no footer da collectionview, ele nunca esconde o activityindicator
                //https://github.com/xamarin/Xamarin.Forms/issues/8700
                // solução momentânea foi simular um footer de overlay com activity indicator, quando estiver corrigido, usar o footer

                await Task.Delay(TimeSpan.FromSeconds(4));

                AnimeGenre animeGenre = await App.Jikan.GetAnimeGenre(_currentGenre.Value, _pageCount);

                if (animeGenre != null)
                {
                    animeGenre.RequestCached = true;

                    if (RemainingAnimeCount == 0)
                        RemainingAnimeCount = animeGenre.TotalCount;

                    var favoritedSubEntries = animeGenre.Anime.ConvertAnimesToFavoritedSubEntry();

                    if (RemainingAnimeCount <= animeGenre.TotalCount)
                        RemainingAnimeCount -= favoritedSubEntries.Count;

                    if (_pageCount == 1)
                    {
                        _originalCollection = favoritedSubEntries.ToList();
                        Animes.ReplaceRange(_originalCollection);
                    }

                    else if (_pageCount > 1)
                    {
                        _originalCollection.AddRange(favoritedSubEntries);
                        Animes.AddRange(favoritedSubEntries);
                    }

                    if (RemainingAnimeCount == 0) // usado para desativar a chamada da collectionview para este método
                    {
                        RemainingAnimeCount = -1;
                        return;
                    }

                    _pageCount++;
                }

                await Task.Delay(TimeSpan.FromSeconds(4));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Problema encontrado: {ex.Message}, valor errado em : {RemainingAnimeCount}");
            }
            finally
            {
                Console.WriteLine($"Animes restantes: {RemainingAnimeCount}");
                Console.WriteLine($"Animes da categoria {_currentGenre.Value} adicionados na lista : {Animes.Count}");
                IsLoadingOrRefreshing = IsBusy = false;
                _firstLoad = false;
                loc.Release();
            }
        }

        public ICommand SelectionModeCommand { get; private set; }
        private void OnSelectionMode()
        {
            if (SelectionMode == SelectionMode.Multiple)
                SingleSelectionMode();
            else
            {
                MultiSelectionMode();
                SelectedItems = null;
            }
        }

        public ICommand AddToFavoriteCommand { get; private set; }
        private async Task OnAddToFavorite()
        {
            if (SelectedItems == null || SelectedItems.Count == 0)
                return;

            bool canNavigate = await NavigationManager.CanPopUpNavigateAsync<ProgressPopupViewModel>();

            if (canNavigate)
            {
                var items = SelectedItems.Cast<FavoritedAnimeSubEntry>().ToList();
                await NavigationManager.NavigatePopUpAsync<ProgressPopupViewModel>(items);
            }

            SingleSelectionMode();
        }

        public ICommand RefreshCommand { get; private set; }
        private async Task OnRefresh()
        {
            IsLoadingOrRefreshing = true;
            await LoadCatalogueAsync();
            IsLoadingOrRefreshing = IsRefreshing = false;
        }

        public ICommand ClearTextCommand { get; private set; }
        private void OnClearText()
        {
            ClearTextQuery();
            SearchCommand.Execute(null);
        }

        public ICommand SearchCommand { get; private set; }
        private async Task OnSearch()
        {
            var resultListTask = Task.Run(() =>
           {
               return _originalCollection.Where(anime => anime.FavoritedAnime.Title.ToLowerInvariant().Contains(SearchQuery.ToLowerInvariant())).ToList();
           });

            var resultList = await resultListTask;
            Animes.ReplaceRange(resultList);
        }

        public ICommand OpenAnimeCommand { get; private set; }
        public async Task OnOpenAnime()
        {
            bool canNavigate = await NavigationManager.CanShellNavigateAsync<AnimeSpecsViewModel>(() =>
              {
                  SelectedItem = null;
              });


            if (!IsMultiSelect && SelectedItem != null && canNavigate)
            {
                await NavigationManager.NavigateShellAsync<AnimeSpecsViewModel>(SelectedItem);
                SelectedItem = null;
            }
        }

        #endregion

        //TODO: botão de filtro entre os 3 pontos e o campo de pesquisa(abrir um modal com opções de filtro)
    }
}
