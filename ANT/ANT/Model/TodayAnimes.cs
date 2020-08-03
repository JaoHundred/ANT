using System;
using System.Collections.Generic;
using System.Text;

namespace ANT.Model
{
    public class TodayAnimes
    {
        public TodayAnimes()
        { }
       
        public int Id { get; set; }
        public IEnumerable<FavoritedAnime> FavoritedAnimes { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public bool ShowNSFW { get; set; }
    }
}
