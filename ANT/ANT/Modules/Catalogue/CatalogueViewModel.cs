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

        private async Task<bool> LoadGlobalCatalogueAsync()
        {
            await App.DelayRequest(4);

            try
            {
                var config = new AnimeSearchConfig
                {
                   OrderBy = AnimeSearchSortable.Title,
                };

                //TODO: o caminho parece ser pelo search anime já que ele me dá acesso a diversos filtros avançados
                //mas na data de hoje 09/04/2020 o método não tem funcionado além da primeira página(ele retorna os mesmos 50 itens da primeira página)
                //foi aberto uma issue para isso em https://github.com/Ervie/jikan.net/issues/12

                //TODO: após a issue ser respondida, testar e verificar se as coisas estão carregando conforme devem(itens distintos de cada página)

                AnimeSearchResult anime = await App.Jikan.SearchAnime("", _pageCount++, config);
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
                await NavigationManager.NavigatePopUpAsync<ProgressPopupViewModel>(items);
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
            //TODO: preparar a busca para o modo global, vai ser necessário retornar qualquer anime dentro da substring que o usuário inseriu na pesquisa
            //checar antes se o anime já está carregado na lista, se estiver não precisa fazer requisição para o jikan, do contrário fazer a requisição e mostrar
            // nos resultados, nesse caso retornar para a situação original antes da busca(o anime que veio por requisição e está agora nos resultados da busca deve
            // sair também da propriedade observável de animes

            if (SearchQuery?.Length > 0)
                RemainingAnimeCount = -1;

            else if (SearchQuery?.Length == 0)
                RemainingAnimeCount = 0;

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
