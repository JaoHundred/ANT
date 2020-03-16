using JikanDotNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace ANT.Model
{
    public class RelatedAnimes
    {
        public RelatedAnimes()
        {
            Parents = new List<MALImageSubItem>();
            Prequels = new List<MALImageSubItem>();
            Sequels = new List<MALImageSubItem>();
            SpinOffs = new List<MALImageSubItem>();
        }

        public List<MALImageSubItem> Parents { get; set; }
        public List<MALImageSubItem> Prequels { get; set; }
        public List<MALImageSubItem> Sequels { get; set; }
        public List<MALImageSubItem> SpinOffs { get; set; }
    }
}
