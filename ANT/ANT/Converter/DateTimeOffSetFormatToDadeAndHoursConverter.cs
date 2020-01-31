using ANT.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace ANT.Converter
{
    public class DateTimeOffSetFormatToDadeAndHoursConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTimeOffset dateOff)
            {
                //TODO: existe um bug no cultureinfo https://github.com/xamarin/Xamarin.Forms/issues/4282
                switch (culture.Name)
                {
                    default://en-US
                        return dateOff.ToString("MM/dd/yyyy h:mm:ss tt");

                    case CultureConsts.BRCulture:
                        return dateOff.ToString("dd/MM/yyyy H:mm:ss");
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
