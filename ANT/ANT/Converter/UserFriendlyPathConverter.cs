using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;
using System.Linq;

namespace ANT.Converter
{
    public class UserFriendlyPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && Device.RuntimePlatform == Device.Android)
            {
                var paths = str.Split('/').ToList();

                paths.RemoveAt(0); //remove o espaço em branco

                var builder = new StringBuilder();

                foreach (var path in paths)
                {
                    string pathLower = path.ToLower();

                    if (pathLower != "storage" && pathLower != "0" && pathLower != "emulated")
                    {
                        if (path.Contains('/'))
                            builder.Append(path);
                        else
                            builder.Append($"/{path}");
                    }
                }

               return builder.ToString();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
