using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ANT.Lang
{
    public interface ILocalize
    {
        CultureInfo GetCurrentCultureInfo();
        void SetLocale(CultureInfo cultureInfo);
    }
}
