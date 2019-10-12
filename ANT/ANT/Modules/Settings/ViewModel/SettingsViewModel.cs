using ANT.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ANT.Modules
{
    class SettingsViewModel
    {
        public SettingsViewModel()
        {

        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;

                switch (_selectedIndex)
                {
                    case 0://light
                        ThemeManager.ChangeTheme(ThemeManager.Themes.Light);
                        break;
                    case 1://dark
                        ThemeManager.ChangeTheme(ThemeManager.Themes.Dark);
                        break;
                }
            }
        }

    }
}
