using System;
using System.Collections.Generic;
using System.Text;

namespace ANT.Core
{
    public interface INativeChangeThemeService
    {
        void onThemeChanged(ThemeManager.Themes theme);
    }
}
