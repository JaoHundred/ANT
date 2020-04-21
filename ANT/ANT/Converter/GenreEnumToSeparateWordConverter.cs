using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;
using JikanDotNet;
using System.Threading.Tasks;

namespace ANT.Converter
{
    public class GenreEnumToSeparateWordConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GenreSearch? genre = (GenreSearch)value;

            if (genre != null)
            {
                string word = genre.ToString();
                var builder = new StringBuilder();
                builder.Append(word[0]);

                for (int i = 1; i < word.Length; i++)
                {
                    char c = word[i];

                    if (char.IsUpper(c))
                        builder.Append(' ');

                    builder.Append(c);
                }

                return builder.ToString();
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
