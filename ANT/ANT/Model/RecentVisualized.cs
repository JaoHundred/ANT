using System;
using System.Collections.Generic;
using System.Text;

namespace ANT.Model
{
    public class RecentVisualized
    {

        public RecentVisualized(FavoritedAnime anime)
        {
            Date = DateTimeOffset.Now;
            FavoritedAnime = anime;
        }

        public RecentVisualized()
        {}

        public DateTimeOffset Date { get; set; }

        public FavoritedAnime FavoritedAnime { get; set; }
    }
}
