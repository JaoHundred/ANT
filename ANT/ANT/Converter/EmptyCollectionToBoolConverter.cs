using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;
namespace ANT.Converter
{
    public class EmptyCollectionToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //TODO: verificar aqui e linha 92 de catalogueview, tem alguma coisa errada, só é chamado uma vez
            //TODO: ver cor de "desligado" para quando o botão estiver desativado
            if (value is IList<object> amount)
                return amount.Count > 0;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
