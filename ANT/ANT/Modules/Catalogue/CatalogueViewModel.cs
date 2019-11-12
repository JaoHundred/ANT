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
            //TODO: atualmente o comando está bindado para um botão, remover o botão e deixar carregar naturalmente quando entrar na view
            //pesquisar como fazer isso, já que o eventtocommand do behavior não funcionou, ver como fazer isso para a contentpage
            //TODO: trocar as fontes segoemdl2 para material(baixar em algum canto a fonte material e ver como se usa)
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
            AnimeGenre genre = await JikanMALService.GetCatalogueByGenreAsync(GenreSearch.SciFi);
            Animes = genre.Anime.Take(300).ToList();
        }
    }
}
