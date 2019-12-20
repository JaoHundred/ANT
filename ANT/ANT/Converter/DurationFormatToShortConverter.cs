using ANT.Core;
using ANT.Lang;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace ANT.Converter
{
    public class DurationFormatToShortConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string duration && duration != Consts.Unknown)
            {
                var split = duration.Split(' ');

                return $"{split[0]} {split[1]}";
            }
            return Consts.NA;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
