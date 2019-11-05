using ANT.Core;
using ANT.UTIL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ANT.Modules
{
    public class SettingsViewModel : NotifyProperty
    {
        public SettingsViewModel()
        {
            SelectedThemeIndex = ThemeManager._currentThemeIndex;
            SelectedLangIndex = CultureManager._currentCultureIndex;


            object val;
            App.Current.Properties.TryGetValue(AppPropertiesConsts.CultureKey, out val);

            IsAutomaticTranslate = val is bool va ? va : true;
        }

        private int _selectedThemeIndex;
        public int SelectedThemeIndex
        {
            get => _selectedThemeIndex;
            set
            {
                if (_selectedThemeIndex == value)
                    return;

                _selectedThemeIndex = value;

                switch (_selectedThemeIndex)
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


        private int _selectedLangIndex;
        public int SelectedLangIndex
        {
            get => _selectedLangIndex;
            set
            {
                if (_selectedLangIndex == value)
                    return;

                Changed(ref _selectedLangIndex, value);

                switch (_selectedLangIndex)
                {
                    case 0://english
                        CultureManager.SetCultureAsync(CultureManager.Culture.English);
                        break;
                    case 1://portuguese
                        CultureManager.SetCultureAsync(CultureManager.Culture.Portuguese);
                        break;
                }
            }
        }

        private bool _isAutomaticTranslate;

        public bool IsAutomaticTranslate
        {
            get => _isAutomaticTranslate;
            set
            {
                if (_isAutomaticTranslate == value)
                    return;

                Changed(ref _isAutomaticTranslate, value);

                App.Current.Properties.AddOrUpdate(AppPropertiesConsts.CultureKey, _isAutomaticTranslate);
            }
        }
    }
}
