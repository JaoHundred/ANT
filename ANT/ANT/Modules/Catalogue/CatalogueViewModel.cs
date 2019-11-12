using System;
using System.Collections.Generic;
using System.Text;
using JikanDotNet;
using JikanDotNet.Helpers;
using Xamarin.Forms;
using ANT._Services;
using System.Linq;
using ANT.UTIL;

namespace ANT.Modules
{
    public class CatalogueViewModel : NotifyProperty
    {
        public CatalogueViewModel()
        {
            OnLoadingCommand = new Command(OnLoad);
        }

        private IList<AnimeSubEntry> _animes;
        public IList<AnimeSubEntry> Animes
        {
            get { return _animes; }
            set { Changed(ref _animes, value); }
        }

        public Command OnLoadingCommand { get; }
        private async void OnLoad()
        {
            AnimeGenre genre = await JikanMALService.GetCatalogueByGenreAsync(GenreSearch.Action);
            Animes = genre.Anime.Take(1000).ToList();
        }
    }
}
