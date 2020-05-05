using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ANT.Model
{
    public class GroupedFavoriteAnimeByWeekDay : List<FavoritedAnime>
    {
        public GroupedFavoriteAnimeByWeekDay(string groupName, IList<FavoritedAnime> collection) : base(collection)
        {
            GroupName = groupName;
        }
        
        public string GroupName { get; set; }

    }
}
