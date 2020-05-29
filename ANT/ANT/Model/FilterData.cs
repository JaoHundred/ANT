using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;
using JikanDotNet;


namespace ANT.Model
{
    public class FilterData : ObservableObject
    {
        public IList<GenreData> Genres { get; set; }

        public ObservableRangeCollection<OrderData> Orders { get; set; }

        public IList<SortDirectionData> SortDirections { get; set; }

        public IList<DayOfWeekFilterDate> DayOfWeekOrder { get; set; }

        //TODO: quando tiver mais opções de filtros, acrescentar aqui
    }
}
