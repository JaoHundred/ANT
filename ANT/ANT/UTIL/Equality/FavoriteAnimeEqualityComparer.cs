using ANT.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ANT.UTIL.Equality
{
    public class FavoriteAnimeEqualityComparer : IEqualityComparer<FavoritedAnime>
    {
        public bool Equals(FavoritedAnime x, FavoritedAnime y)
        {
            return x.Anime.MalId == y.Anime.MalId;
        }

        public int GetHashCode(FavoritedAnime obj)
        {
            return new { obj.Anime.MalId, obj.Anime.Title, obj.Anime.TitleEnglish }.GetHashCode();
        }
    }
}
