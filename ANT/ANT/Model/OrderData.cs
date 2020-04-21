using System;
using System.Collections.Generic;
using System.Text;
using JikanDotNet;

namespace ANT.Model
{
    public class OrderData
    {

        public OrderData(AnimeSearchSortable animeSearchSortable, bool isChecked = false)
        {
            OrderBy = animeSearchSortable;
            IsChecked = isChecked;
        }

        public AnimeSearchSortable OrderBy { get; set; }

        public bool IsChecked { get; set; }
    }
}
