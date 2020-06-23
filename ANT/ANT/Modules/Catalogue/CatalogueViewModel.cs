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
using Android.App;

namespace ANT.Modules
{
    public class CatalogueViewModel : BaseVMExtender, IAsyncInitialization
    {
        //TODO: impedir de abrir a janela de filtros se eu tiver chegado nessa tela a partir da seleção de 1 gênero em AnimeSpecs
        //https://github.com/JaoHundred/ANT/issues/42

        public CatalogueViewModel(CatalogueModeEnum catalogueMode)
        {
            _catalogueMode = catalogueMode;
            InitializeDefaultProperties();

            InitializeTask = LoadAsync(null);
        }

        public CatalogueViewModel(GenreSearch genreEnum)
        {
            _currentGenre = genreEnum;
            //TODO: os filtros não funcionam pelo abertura direto dos gêneros

            InitializeDefaultProperties();

            InitializeTask = LoadAsync(null);
        }

        private void InitializeDefaultProperties()
        {
            _originalCollection = new List<FavoritedAnime>();

            _animeSearchConfig = new AnimeSearchConfig()
            {
                OrderBy = AnimeSearchSortable.Score,
                SortDirection = SortDirection.Descending,
            };

            Animes = new ObservableRangeCollection<FavoritedAnime>();

            SelectionModeCommand = new magno.Command(OnSelectionMode);
            AddToFavoriteCommand = new magno.AsyncCommand(OnAddToFavorite);
            ClearTextCommand = new magno.Command(OnClearText);
            SearchCommand = new magno.AsyncCommand(OnSearch);
            OpenAnimeCommand = new magno.AsyncCommand(OnOpenAnime);
            LoadMoreCommand = new magno.AsyncCommand(OnLoadMore);
            GenreCheckedCommand = new magno.Command<GenreData>(OnGenreCheck);
            ApplyFilterCommand = new magno.AsyncCommand(OnApplyFilter);
            ResetFilterCommand = new magno.AsyncCommand(OnResetFilter);
            BackButtonCommand = new magno.AsyncCommand<CatalogueView>(OnBackButton);
            SelectSortDirectionCommand = new magno.Command<SortDirectionData>(OnChangeSortDirection);
            SelectSortTypeCommand = new magno.Command<OrderData>(OnSelectSortType);
            ChangeSeasonCommand = new magno.AsyncCommand(OnChangeSeason);
        }

        public Task NavigationFrom()
        {
            return Task.Run(() =>
            {
                foreach (var observableAnime in Animes)
                {
                    var favorited = App.liteDB.GetCollection<FavoritedAnime>().FindOne(p => p.Anime.MalId == observableAnime.Anime.MalId);

                    observableAnime.IsFavorited = favorited != null;
                }
            });
        }

        private int _pageCount = 1;
        private readonly CatalogueModeEnum? _catalogueMode;
        private readonly GenreSearch? _currentGenre;
        private AnimeSearchConfig _animeSearchConfig;

        public Task InitializeTask { get; }
        public async Task LoadAsync(object param)
        {
            Loading = true;

            //TODO: deixar assim a parte do "fillgenres" por hora até ver como vai ser a 
            //integração do sistema de escolha do usuário para o que ele não quer exibir
            FilterData = new FilterData
            {
                Genres = ANT.UTIL.AnimeExtension.FillGenres(showNSFWGenres: false),
                SortDirections = UTIL.AnimeExtension.FillSortDirectionData(),
                Orders = new ObservableRangeCollection<OrderData>(UTIL.AnimeExtension.FillOrderData()),
            };

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

            Loading = false;
        }

        #region proriedades

        private bool _loading;
        public bool Loading
        {
            get { return _loading; }
            set { SetProperty(ref _loading, value); }
        }

        public FilterData FilterData { get; set; }

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

        private List<FavoritedAnime> _animesWithSpecifiedFilters;
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

        private SeasonData _seasonData;
        public SeasonData SeasonData
        {
            get { return _seasonData; }
            set { SetProperty(ref _seasonData, value); }
        }

        private bool _isSeasonCatalogue;
        public bool IsSeasonCatalogue
        {
            get { return _isSeasonCatalogue; }
            set { SetProperty(ref _isSeasonCatalogue, value); }
        }

        #endregion

        #region métodos da VM
        private async Task LoadSeasonCatalogueAsync()
        {
            IsSeasonCatalogue = true;
            //remove EndDate, essa informação não existe nos animes da temporada
            FilterData.Orders.RemoveAt(FilterData.Orders.Count - 1);

            await App.DelayRequest();
            var results = await App.Jikan.GetSeason();
            
            results.RequestCached = true;

            var favoritedEntries = results.SeasonEntries.ConvertAnimesToFavorited();
            _originalCollection = favoritedEntries.OrderByDescending(p => p.Anime.Score).ToList();
            Animes.AddRange(_originalCollection);

            await App.DelayRequest();
            var seasonArchive = await App.Jikan.GetSeasonArchive();
            int minYear = seasonArchive.Archives.Min(p => p.Year);
            int maxYear = seasonArchive.Archives.Max(p => p.Year);

            Seasons name = (JikanDotNet.Seasons)Enum.Parse(typeof(JikanDotNet.Seasons), results.SeasonName);
            SeasonData = new SeasonData(results.SeasonYear, results.SeasonName, minYear, maxYear);
        }

        private async Task<bool> LoadGlobalCatalogueAsync()
        {
            await App.DelayRequest(4);

            try
            {
                //TODO: https://github.com/JaoHundred/ANT/issues/32

                AnimeSearchResult anime = await App.Jikan.SearchAnime(_animeSearchConfig, _pageCount++);

                Console.WriteLine($"Page count {_pageCount}");

                if (anime?.Results != null)
                {
                    anime.RequestCached = true;

                    IList<FavoritedAnime> animes = anime.Results.ConvertAnimeSearchEntryToAnimeSubEntry().ConvertAnimesToFavorited();

                    _originalCollection.AddRange(animes);
                    Animes.AddRange(animes);

                    return false;
                }
            }
            catch { }

            return true;
        }

        private async Task<bool> LoadByGenreAsync()
        {
            await App.DelayRequest(4);

            //TODO: entradas por aqui de gêneros como super power são convertidas para supernatural(o valor não muda, mas o MAL me manda os gêneros errados pra esse tipo)
            //ficar de olho e ver se é apenas mais um problema no MAL
            AnimeGenre animeGenre = await App.Jikan.GetAnimeGenre(_currentGenre.Value, _pageCount++);

            if (animeGenre != null)
            {
                animeGenre.RequestCached = true;

                IList<FavoritedAnime> favoritedSubEntries = animeGenre.Anime.ConvertAnimesToFavorited();

                _originalCollection.AddRange(favoritedSubEntries);
                Animes.AddRange(favoritedSubEntries);

                return false;
            }
            else
            {
                RemainingAnimeCount = -1;
                return true;
            }
        }

        private void ClearTextQuery() => SearchQuery = string.Empty;
        #endregion

        #region commands

        public ICommand BackButtonCommand { get; private set; }
        private async Task OnBackButton(CatalogueView catalogueView)
        {
            MessagingCenter.Unsubscribe<CatalogueViewModel>(catalogueView, "CloseFilterView");
            await NavigationManager.PopShellPageAsync();
        }

        public ICommand LoadMoreCommand { get; private set; }
        private async Task OnLoadMore()
        {
            //TODO: fazer o teste com o semaphore https://docs.microsoft.com/pt-br/dotnet/standard/threading/semaphore-and-semaphoreslim
            //para evitar chamadas paralelas a este método(não parece ser problema de sincronização mas da própria API, testar com semaphore mesmo assim
            //se ainda acontecer testar com remoção de itens duplicados da lista original e observável de animes


            RemainingAnimeCount = -1;
            IsBusy = true;

            Console.WriteLine("Executou OnLoadMore");
            bool hasFinishedLoading = false;

            try
            {
                if (_currentGenre != null)
                    hasFinishedLoading = await LoadByGenreAsync();

                else if (_currentGenre == null && _catalogueMode != null)
                    hasFinishedLoading = await LoadGlobalCatalogueAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro encontrado em {ex.Message}");
            }
            finally
            {
                RemainingAnimeCount = 0;
                IsBusy = false;

                if (hasFinishedLoading)
                    RemainingAnimeCount = -1;

                try
                {
                    var dupeList = Animes.GroupBy(p => p.Anime.MalId).Where(p => p.Count() > 1).Select(p => p.First()).ToList();

                    if (dupeList.Count > 0)
                    {
                        //TODO: duplicados tem aparecido quando o MAL tem tido problemas de servidor, investigar mais a fundo quand o MAL
                        //estiver com problemas mínimos no servidor para descobrir se o problema é do lado deles ou da API jikan(se for do jikan, abrir uma issue)
                        //os duplicados aparecem nos limites das páginas(anime no final da página e no começo)
                        //https://github.com/JaoHundred/ANT/issues/35

                        Console.WriteLine($"Animes duplicados {Environment.NewLine} ");

                        foreach (var item in dupeList.Select((Anime, ID) => new { Anime.Anime.Title, Anime.Anime.MalId }))
                            Console.WriteLine($"Anime : {item.Title} {Environment.NewLine} ID : {item.MalId}");

                        Console.WriteLine("Remoção de duplicados");
                        Animes.RemoveRange(dupeList);

                        foreach (var item in dupeList)
                        {
                            var anime = _originalCollection.FirstOrDefault(p => p.Anime.MalId == item.Anime.MalId);

                            if (anime != null)
                                _originalCollection.Remove(anime);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ocorreu um erro no teste de duplicados {Environment.NewLine} {ex.Message}");
                }

                Console.WriteLine($"{Animes.Count} Animes na lista ");
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
                await NavigationManager.NavigatePopUpAsync<ProgressPopupViewModel>(items, this);
            };

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
            if (SearchQuery?.Length > 0)
                RemainingAnimeCount = -1;

            else if (SearchQuery?.Length == 0 && _catalogueMode == CatalogueModeEnum.Season)
                RemainingAnimeCount = -1;

            else if (SearchQuery?.Length == 0 && _catalogueMode == CatalogueModeEnum.Global)
                RemainingAnimeCount = 0;

            else if (SearchQuery?.Length == 0 && _currentGenre != null)
                RemainingAnimeCount = 0;

            var resultListTask = Task.Run(() =>
           {
               IList<FavoritedAnime> result = null;

               switch (_catalogueMode)
               {
                   case CatalogueModeEnum.Season:

                       if (_animesWithSpecifiedFilters != null)
                           result = _animesWithSpecifiedFilters.Where(anime => anime.Anime.Title.ToLowerInvariant()
                           .Contains(SearchQuery.ToLowerInvariant())).ToList();

                       else
                           result = _originalCollection.Where(anime => anime.Anime.Title.ToLowerInvariant()
                           .Contains(SearchQuery.ToLowerInvariant())).ToList();

                       break;

                   case CatalogueModeEnum.Global:

                       result = _originalCollection.Where(anime => anime.Anime.Title.ToLowerInvariant()
                        .Contains(SearchQuery.ToLowerInvariant())).ToList();

                       break;
               }

               return result;
           });

            Animes.ReplaceRange(await resultListTask);
        }

        bool _canNavigate = true;
        public ICommand OpenAnimeCommand { get; private set; }
        private async Task OnOpenAnime()
        {

            if (!IsMultiSelect && SelectedItem != null && _canNavigate)
            {
                _canNavigate = false;
                await NavigationManager.NavigateShellAsync<AnimeSpecsViewModel>(SelectedItem.Anime.MalId, new Func<Task>(NavigationFrom));
                SelectedItem = null;
                _canNavigate = true;
            }
        }

        public ICommand GenreCheckedCommand { get; private set; }
        private void OnGenreCheck(GenreData genreData)
        {
            genreData.IsChecked = !genreData.IsChecked;
        }

        public ICommand ApplyFilterCommand { get; private set; }
        private async Task OnApplyFilter()
        {
            _animeSearchConfig.Genres = FilterData.Genres.Where(p => p.IsChecked).Select(p => p.Genre).ToList();

            MessagingCenter.Send(this, "CloseFilterView");
            switch (_catalogueMode)
            {
                case CatalogueModeEnum.Season:

                    Loading = true;
                    await Task.Delay(TimeSpan.FromMilliseconds(500)); // usado para impedir que seja visto um leve engasto na filtragem

                    _animesWithSpecifiedFilters = new List<FavoritedAnime>();

                    switch (_animeSearchConfig.OrderBy)
                    {
                        case AnimeSearchSortable.Title:

                            if (_animeSearchConfig.SortDirection == SortDirection.Ascending)
                                _animesWithSpecifiedFilters.AddRange(_originalCollection.OrderBy(p => p.Anime.Title));
                            else
                                _animesWithSpecifiedFilters.AddRange(_originalCollection.OrderByDescending(p => p.Anime.Title));

                            break;
                        case AnimeSearchSortable.StartDate:
                            //TODO: existem alguns animes da temporada que não estão refletindo corretamente a data crescente, investigar e tentar descobrir
                            //o que pode ser
                            if (_animeSearchConfig.SortDirection == SortDirection.Ascending)
                                _animesWithSpecifiedFilters.AddRange(_originalCollection.OrderBy(p => p.Anime.Aired.From));
                            else
                                _animesWithSpecifiedFilters.AddRange(_originalCollection.OrderByDescending(p => p.Anime.Aired.From));

                            break;
                        case AnimeSearchSortable.Score:

                            if (_animeSearchConfig.SortDirection == SortDirection.Ascending)
                                _animesWithSpecifiedFilters.AddRange(_originalCollection.OrderBy(p => p.Anime.Score));
                            else
                                _animesWithSpecifiedFilters.AddRange(_originalCollection.OrderByDescending(p => p.Anime.Score));

                            break;
                    }

                    var animeToRemove = new List<FavoritedAnime>();
                    for (int i = 0; i < _animesWithSpecifiedFilters.Count; i++)
                    {
                        FavoritedAnime favoritedAnime = _animesWithSpecifiedFilters[i];

                        bool hasAllGenres = await favoritedAnime.HasAllSpecifiedGenresAsync(_animeSearchConfig.Genres.ToArray());

                        if (!hasAllGenres)
                            animeToRemove.Add(favoritedAnime);
                    }

                    if (animeToRemove.Count > 0)
                        foreach (var item in animeToRemove)
                            _animesWithSpecifiedFilters.Remove(item);

                    Animes.ReplaceRange(_animesWithSpecifiedFilters);

                    Loading = false;

                    break;
                case CatalogueModeEnum.Global:
                    await ResetAndLoadGlobalAsync();
                    break;
            }
        }

        private async Task ResetAndLoadGlobalAsync()
        {
            _originalCollection.Clear();
            Animes.Clear();
            _pageCount = 1;
            ClearTextQuery();
            RemainingAnimeCount = 0;
            Loading = true;
            await LoadGlobalCatalogueAsync();
            Loading = false;
        }

        public ICommand ResetFilterCommand { get; private set; }
        private async Task OnResetFilter()
        {
            var checkeds = FilterData.Genres.Where(p => p.IsChecked);
            FilterData.SortDirections[0].IsChecked = true;
            FilterData.Orders[0].IsChecked = true;

            foreach (var item in checkeds)
                item.IsChecked = false;

            MessagingCenter.Send(this, "CloseFilterView");
            switch (_catalogueMode)
            {
                case CatalogueModeEnum.Season:

                    Loading = true;
                    _animesWithSpecifiedFilters = null;
                    Animes.ReplaceRange(_originalCollection);
                    Loading = false;

                    break;

                case CatalogueModeEnum.Global:

                    _animeSearchConfig = new AnimeSearchConfig()
                    {
                        OrderBy = AnimeSearchSortable.Score,
                        SortDirection = SortDirection.Descending,
                    };

                    await ResetAndLoadGlobalAsync();

                    break;
            }
        }


        public ICommand SelectSortDirectionCommand { get; private set; }
        private void OnChangeSortDirection(SortDirectionData sortDirectionData)
            => _animeSearchConfig.SortDirection = sortDirectionData.SortDirection;

        public ICommand SelectSortTypeCommand { get; private set; }
        private void OnSelectSortType(OrderData orderData)
        {

            //TODO: startDate e endDate tem tanto opção para ordenar via eles(com os radio button) quanto opção para
            //inserir um intervalo de datas, decidir o que fazer a respeito desses 2 casos
            //TODO: traduzir via converter os nomes dos filtros

            _animeSearchConfig.OrderBy = orderData.OrderBy;
        }

        public ICommand ChangeSeasonCommand { get; private set; }
        private async Task OnChangeSeason()
        {
            Loading = true;
            ClearTextQuery();

            await App.DelayRequest();

            var result = await App.Jikan.GetSeason(SeasonData.SelectedYear.Value, SeasonData.SelectedSeason);
            result.RequestCached = true;

            var favoritedEntries = result.SeasonEntries.ConvertAnimesToFavorited();
            _originalCollection = favoritedEntries.OrderByDescending(p => p.Anime.Score).ToList();
            Animes.ReplaceRange(_originalCollection);

            Loading = false;
        }



        #endregion

        //TODO: o footer ainda não está se comportando conforme deveria

        //TODO: os filtros no catálogo de season devem ser nesse formato abaixo(não fazem requisições via internet para jikan)
        /*
         * .Where(
            anime => anime.R18 == false &&
            anime.HasAllSpecifiedGenres(GenreSearch.Ecchi) == false
            )
        */
    }
}
