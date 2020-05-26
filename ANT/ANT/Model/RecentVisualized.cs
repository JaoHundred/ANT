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

        private DateTimeOffset _date;
        public DateTimeOffset Date { get => _date; set => _date = value.LocalDateTime; }

        public FavoritedAnime FavoritedAnime { get; set; }
    }
}
