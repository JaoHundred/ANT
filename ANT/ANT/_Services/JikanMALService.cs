using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JikanDotNet;
using JikanDotNet.Exceptions;
using JikanDotNet.Helpers;

namespace ANT._Services
{
    public static class JikanMALService
    {
        private static IJikan _jikan = _jikan = new Jikan(useHttps: true);

        //TODO: ler a documentação e entender como se usa o jikan para extrair dados referente a animes, personagens, temporadas e outras coisas, 
        //consultar o onenote para mais informações


        public static Task<AnimeGenre> GetCatalogueByGenreAsync(GenreSearch genre)
        {
            return _jikan.GetAnimeGenre(genre);
        }

        public static Task<AnimeSearchResult> GetAnimesAsync(string name)
        {
            return _jikan.SearchAnime(name);
        }

        public static Task<Season> GetSeasonAsync()
        {
            return _jikan.GetSeason();
        }

        public static Task<CharacterSearchResult> GetCharactersAsync(string name)
        {
            return _jikan.SearchCharacter(name);
        }
    }
}
