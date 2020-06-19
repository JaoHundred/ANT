using ANT.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using MvvmHelpers;
using ANT.Model;
using System.Windows.Input;
using ANT.Interfaces;

namespace ANT.Modules
{
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel()
        {

            //TODO: as funcionalidades de troca de idioma dentro dos settings não funcionam, deixar desativado isso por tempo indeterminado

            Settings = App.liteDB.GetCollection<SettingsPreferences>().FindById(0);

            SelectedThemeIndex = Settings.SelectedThemeIndex;
            //SelectedLangIndex = App.SettingsPreferences.SelectedLanguageIndex;
            //IsAutomaticTranslate = App.SettingsPreferences.AutomaticTranslate;

            SwitchNotificationCommand = new Command(OnSwitchNotification);
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

        public SettingsPreferences Settings { get; set; }

        //private int _selectedLangIndex;
        //public int SelectedLangIndex
        //{
        //    get => _selectedLangIndex;
        //    set
        //    {
        //        if (_selectedLangIndex == value)
        //            return;

        //        SetProperty(ref _selectedLangIndex, value);

        //        //switch (_selectedLangIndex)
        //        //{
        //        //    case 0://english
        //        //        CultureManager.SetCultureAsync(CultureManager.Culture.English);
        //        //        break;
        //        //    case 1://portuguese
        //        //        CultureManager.SetCultureAsync(CultureManager.Culture.Portuguese);
        //        //        break;
        //        //}
        //    }
        //}

        //private bool _isAutomaticTranslate;

        //public bool IsAutomaticTranslate
        //{
        //    get => _isAutomaticTranslate;
        //    set
        //    {
        //        if (_isAutomaticTranslate == value)
        //            return;

        //        SetProperty(ref _isAutomaticTranslate, value);

        //        //App.SettingsPreferences.AutomaticTranslate = _isAutomaticTranslate;
        //        //JsonStorage.SaveSettingsAsync(App.SettingsPreferences, StorageConsts.LocalAppDataFolder, StorageConsts.SettingsFileName);
        //    }
        //}


        public ICommand SwitchNotificationCommand { get; private set; }
        private void OnSwitchNotification()
        {
            var settings = App.liteDB.GetCollection<SettingsPreferences>().FindById(0);

            if(settings != null && settings.NotificationsIsOn != Settings.NotificationsIsOn)
            {
                if (Settings.NotificationsIsOn)
                    DependencyService.Get<IWork>().CreatePeriodicWorkAndReplaceExisting("0", TimeSpan.FromDays(1));
                else
                    DependencyService.Get<IWork>().CancelWork("0");

                App.liteDB.GetCollection<SettingsPreferences>().Upsert(0, Settings);
            }
        }
    }
}
