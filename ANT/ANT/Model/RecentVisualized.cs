using System;
using System.Collections.Generic;
using System.Text;

namespace ANT.Model
{
    public class RecentVisualized
    {

        public RecentVisualized(FavoritedAnimeSubEntry anime)
        {
            Date = DateTimeOffset.Now;
            Anime = anime;
        }

        public DateTimeOffset Date { get; set; }

        public FavoritedAnimeSubEntry Anime { get; set; }
    }
}
