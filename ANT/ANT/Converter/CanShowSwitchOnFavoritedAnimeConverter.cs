using ANT.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace ANT.Converter
{
    public class CanShowSwitchOnFavoritedAnimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FavoritedAnime favoritedAnime)
                return favoritedAnime.NextStreamDate != null && favoritedAnime.Anime.Airing;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
