using System;
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
        public CatalogueViewModel(CatalogueModeEnum catalogueMode)
        {
            _catalogueMode = catalogueMode;
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
            _originalCollection = new List<FavoritedAnime>();
            Animes = new ObservableRangeCollection<FavoritedAnime>();

            SelectionModeCommand = new magno.Command(OnSelectionMode);
            AddToFavoriteCommand = new magno.AsyncCommand(OnAddToFavorite);
            ClearTextCommand = new magno.Command(OnClearText);
            SearchCommand = new magno.AsyncCommand(OnSearch);
            OpenAnimeCommand = new magno.AsyncCommand(OnOpenAnime);
            LoadMoreCommand = new magno.AsyncCommand(OnLoadMore);
        }

        public Task NavigationFrom()
        {
            return Task.Run(() =>
            {
                foreach (var observableAnime in Animes)
                {
                    var favorited = App.FavoritedAnimes.FirstOrDefault(p => p.Anime.MalId == observableAnime.Anime.MalId);

                    if (favorited != null)
                        Device.BeginInvokeOnMainThread(() => { observableAnime.IsFavorited = true; });
                    else
                        Device.BeginInvokeOnMainThread(() => { observableAnime.IsFavorited = false; });
                }
            });
        }

        private int _pageCount = 1;
        private readonly CatalogueModeEnum? _catalogueMode;
        private readonly GenreSearch? _currentGenre;

        public Task InitializeTask { get; }
        public async Task LoadAsync(object param)
        {

            IsFirstLoading = true;

            if (SearchQuery?.Length > 0)
                ClearTextQuery();

            if (_currentGenre != null)
            {
                RemainingAnimeCount = 0;
                await LoadByGenreAsync();
            }
            else
            {
                switch (_catalogueMode)
                {
                    case CatalogueModeEnum.Season:
                        RemainingAnimeCount = -1;
                        await LoadSeasonCatalogueAsync();
                        break;

                    case CatalogueModeEnum.Global:
                        RemainingAnimeCount = 0;
                        await LoadGlobalCatalogueAsync();
                        break;
                }
            }

            IsFirstLoading = false;
        }

        #region proriedades

        private bool _isFirstLoading;
        public bool IsFirstLoading
        {
            get { return _isFirstLoading; }
            set { SetProperty(ref _isFirstLoading, value); }
        }

        private FavoritedAnime _selectedItem;
        public FavoritedAnime SelectedItem
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

        private List<FavoritedAnime> _originalCollection;
        private ObservableRangeCollection<FavoritedAnime> _animes;
        public ObservableRangeCollection<FavoritedAnime> Animes
        {
            get { return _animes; }
            set { SetProperty(ref _animes, value); }
        }

        //com o valor zero itens novos são carregados somente quando a collectionview chegar no fim da lista
        //com -1 é parado por completo as chamadas para LoadMore
        private long _remainingAnimeCount;
        public long RemainingAnimeCount
        {
            get { return _remainingAnimeCount; }
            set { SetProperty(ref _remainingAnimeCount, value); }
        }

        #endregion

        #region métodos da VM
        private async Task LoadSeasonCatalogueAsync()
        {
            await App.DelayRequest();
            var results = await App.Jikan.GetSeason();
            results.RequestCached = true;

            var favoritedEntries = results.SeasonEntries.ConvertAnimesToFavorited();
            _originalCollection = favoritedEntries.ToList();
            Animes.AddRange(_originalCollection);
        }


        private async Task LoadGlobalCatalogueAsync()
        {
            if (!IsFirstLoading)
                IsBusy = true;

            await App.DelayRequest(4);

            AnimeTop anime = await App.Jikan.GetAnimeTop(_pageCount++);

            if (anime != null)
            {
                anime.RequestCached = true;

                IList<FavoritedAnime> animes = anime.Top.ConvertTopAnimesToAnimeSubEntry().ConvertAnimesToFavorited();

                _originalCollection.AddRange(animes);
                Animes.AddRange(animes);
            }
            else
                RemainingAnimeCount = -1;

            IsBusy = false;
        }

        private async Task LoadByGenreAsync()
        {
            if (!IsFirstLoading)
                IsBusy = true;

            await App.DelayRequest(4);

            AnimeGenre animeGenre = await App.Jikan.GetAnimeGenre(_currentGenre.Value, _pageCount++);

            if (animeGenre != null)
            {
                animeGenre.RequestCached = true;

                IList<FavoritedAnime> favoritedSubEntries = animeGenre.Anime.ConvertAnimesToFavorited();

                _originalCollection.AddRange(favoritedSubEntries);
                Animes.AddRange(favoritedSubEntries);
            }
            else
                RemainingAnimeCount = -1;

            IsBusy = false;
        }

        private void ClearTextQuery() => SearchQuery = string.Empty;
        #endregion

        #region commands

        public ICommand LoadMoreCommand { get; private set; }
        private async Task OnLoadMore()
        {
            if (SearchQuery?.Length > 0 || IsBusy || _currentGenre == null)
            {
                Console.WriteLine("Não executou o OnLoadMore");
                return;
            }

            IsBusy = true;

            try
            {
                if (_currentGenre != null)
                    await LoadByGenreAsync();

                else if (_currentGenre == null && _catalogueMode != null)
                    await LoadGlobalCatalogueAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro encontrado em {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"{Animes.Count} Animes na lista ");


                //TODO: pegar aqui os possíveis duplicados da lista de animes e exibir quais foram
                //atualmente fazendo os testes com o gênero military que tem 555 animes na data de hoje 02/04/2020
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
                var items = SelectedItems.Cast<FavoritedAnime>().ToList();
                await NavigationManager.NavigatePopUpAsync<ProgressPopupViewModel>(items);
            }

            SingleSelectionMode();
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
            //TODO: preparar a busca para o modo global, vai ser necessário retornar qualquer anime dentro da substring que o usuário inseriu na pesquisa
            //checar antes se o anime já está carregado na lista, se estiver não precisa fazer requisição para o jikan, do contrário fazer a requisição e mostrar
            // nos resultados, nesse caso retornar para a situação original antes da busca(o anime que veio por requisição e está agora nos resultados da busca deve
            // sair também da propriedade observável de animes

            var resultListTask = Task.Run(() =>
           {
               return _originalCollection.Where(anime => anime.Anime.Title.ToLowerInvariant().Contains(SearchQuery.ToLowerInvariant())).ToList();
           });

            var resultList = await resultListTask;
            Animes.ReplaceRange(resultList);
        }

        bool _canNavigate = true;
        public ICommand OpenAnimeCommand { get; private set; }
        public async Task OnOpenAnime()
        {

            if (!IsMultiSelect && SelectedItem != null && _canNavigate)
            {
                _canNavigate = false;
                await NavigationManager.NavigateShellAsync<AnimeSpecsViewModel>(SelectedItem.Anime.MalId, new Func<Task>(NavigationFrom));
                SelectedItem = null;
                _canNavigate = true;
            }
        }

        #endregion

        //TODO: o footer ainda não está se comportando conforme deveria

        //TODO: botão de filtro entre os 3 pontos e o campo de pesquisa(abrir um modal com opções de filtro)
        //TODO: temporário criar meios de filtros especializados no futuro, possivelmente por uma outra view e viewmodel 
        //que seleciona os filtros e repassa para cá
        /*
         * .Where(
            anime => anime.R18 == false &&
            anime.HasAllSpecifiedGenres(GenreSearch.Ecchi) == false
            )
        */
    }
}
