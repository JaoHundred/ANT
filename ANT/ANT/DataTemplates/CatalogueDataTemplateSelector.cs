using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using ANT.Model;

namespace ANT.DataTemplates
{
    public class CatalogueDataTemplateSelector : DataTemplateSelector
    {

        public DataTemplate FavoritedAnime { get; set; }
        public DataTemplate NotFavoritedAnime { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var anime = item as FavoritedAnimeSubEntry;

            if (anime != null && anime.IsFavorited)
                return FavoritedAnime;

            return NotFavoritedAnime;
        }
    }
}
