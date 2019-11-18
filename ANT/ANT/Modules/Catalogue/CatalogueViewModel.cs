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

namespace ANT.Modules
{
    public class CatalogueViewModel : NotifyProperty, IAsyncInitialization
    {
        public CatalogueViewModel()
        {
            //TODO: trocar as fontes segoemdl2 para material(baixar em algum canto a fonte material e ver como se usa)
            InitializeTask = LoadAync();
        }


        #region proriedades

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { Changed(ref _isLoading, value); }
        }

        private IList<AnimeSubEntry> _animes;
        public IList<AnimeSubEntry> Animes
        {
            get { return _animes; }
            set { Changed(ref _animes, value); }
        } 
        #endregion

        public Task InitializeTask { get; }
        public async Task LoadAync()
        {
            IsLoading = true;
            AnimeGenre genre = await JikanMALService.GetCatalogueByGenreAsync(GenreSearch.SciFi);
            Animes = genre.Anime.Take(300).ToList();
            IsLoading = false;
        }
    }
}
