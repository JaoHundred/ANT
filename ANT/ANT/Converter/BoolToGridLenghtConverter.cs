﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace ANT.Converter
{
    public class BoolToGridLenghtConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSeasonCatalogue && isSeasonCatalogue)
            {
                string par = parameter.ToString();
                return new GridLength(int.Parse(par), GridUnitType.Star);
            }

            return new GridLength(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
