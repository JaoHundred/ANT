using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ANT.Model
{
    public class GroupedRelatedAnime : List<RelatedAnime>
    {
        public GroupedRelatedAnime(string groupName, IList<RelatedAnime> relatedAnimes) : base(relatedAnimes)
        {
            GroupName = groupName;
        }
        
        public string GroupName { get; set; }

    }
}
