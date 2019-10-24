using ANT.Core;
using ANT.UTIL;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ANT.Modules
{
    public class SettingsViewModel
    {
        public SettingsViewModel()
        {
            SelectedIndex = ThemeManager._currentThemeIndex;
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex == value)
                    return;

                _selectedIndex = value;

                switch (_selectedIndex)
                {
                    case 0://light
                        ThemeManager.ChangeThemeAsync(ThemeManager.Themes.Light);
                        break;
                    case 1://dark
                        ThemeManager.ChangeThemeAsync(ThemeManager.Themes.Dark);
                        break;
                }
            }
        }

    }
}
