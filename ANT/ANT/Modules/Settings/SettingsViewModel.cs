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
            SwitchNSFWCommand = new Command(OnSwitchNSFW);
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

        private TimeSpan _timeToNotify;

        public TimeSpan TimeToNotify
        {
            get { return _timeToNotify = Settings.HourToNotify; }
            set
            {
                if (Settings.HourToNotify != value)
                {
                    Settings.HourToNotify = value;

                    var worksIds = new[]
                    {
                        WorkManagerConsts.AnimesNotificationWorkId,
                        WorkManagerConsts.ReschedulerWorkId,
                    };

                    DependencyService.Get<IWork>().CancelWork(WorkManagerConsts.AnimesNotificationWorkId);

                    //DependencyService.Get<IAlarm>()
                    //     .StartAlarmRTCWakeUp(Settings.HourToNotify, int.Parse(WorkManagerConsts.AnimesNotificationWorkId), TimeSpan.FromDays(1));
                    var hourToNotify = DependencyService.Get<IWork>().InitialDelay(Settings.HourToNotify);
                    DependencyService.Get<IWork>().CreateOneTimeWorkAndKeep(WorkManagerConsts.ReschedulerWorkId, hourToNotify);

                    App.liteDB.GetCollection<SettingsPreferences>().Upsert(0, Settings);

                    SetProperty(ref _timeToNotify, value);
                }
            }
        }



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
            if (Settings != null)
            {
                if (Settings.NotificationsIsOn)
                {
                    var hourToNotify = DependencyService.Get<IWork>().InitialDelay(Settings.HourToNotify);
                    DependencyService.Get<IWork>().CreateOneTimeWorkAndKeep(WorkManagerConsts.ReschedulerWorkId, hourToNotify);
                    //DependencyService.Get<IAlarm>()
                    //    .StartAlarmRTCWakeUp(Settings.HourToNotify, int.Parse(WorkManagerConsts.AnimesNotificationWorkId), TimeSpan.FromDays(1));
                }
                else
                {
                    var worksIds = new[]
                    {
                        WorkManagerConsts.AnimesNotificationWorkId,
                        WorkManagerConsts.ReschedulerWorkId,
                    };

                    DependencyService.Get<IWork>().CancelWork(worksIds);
                    //DependencyService.Get<IAlarm>().CancelAlarm(int.Parse(WorkManagerConsts.AnimesNotificationWorkId));
                }

                App.liteDB.GetCollection<SettingsPreferences>().Upsert(0, Settings);
            }
        }

        public ICommand SwitchNSFWCommand { get; private set; }
        private void OnSwitchNSFW()
        {
            if(Settings!= null)
            {
                App.liteDB.GetCollection<SettingsPreferences>().Upsert(0, Settings);
            }
        }
    }
}
