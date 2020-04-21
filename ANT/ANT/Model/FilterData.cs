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

        public IList<OrderData> Orders { get; set; }

        public IList<SortDirection> SortDirections { get; set; }

        //TODO: quando tiver mais opções de filtros, acrescentar aqui
    }
}
