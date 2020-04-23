using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;
using System.Resources;

namespace ANT.Converter
{
    public class TranslateEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Enum bla = (Enum)value;

            if(bla != null)
            {
                Lazy<ResourceManager> ResMgr = new Lazy<ResourceManager>(
                    () => new ResourceManager(typeof(Lang.Lang)));
                
                string translated = ResMgr.Value.GetString(bla.ToString());

                return translated;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
