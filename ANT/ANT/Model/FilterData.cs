using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;

namespace ANT.Model
{
    public class FilterData : ObservableObject
    {
        public IList<GenreData> Genres { get; set; }

        //TODO: quando tiver mais opções de filtros, acrescentar aqui
    }
}
