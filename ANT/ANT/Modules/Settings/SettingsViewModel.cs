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
using magno = MvvmHelpers.Commands;

namespace ANT.Modules
{
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel()
        {

            //TODO: as funcionalidades de troca de idioma dentro dos settings não funcionam, deixar desativado isso por tempo indeterminado

            Settings = App.liteDB.GetCollection<SettingsPreferences>().FindById(0);

            SelectedThemeIndex = Settings.SelectedThemeIndex;
            CurrentVersion = Xamarin.Essentials.VersionTracking.CurrentVersion;
            //SelectedLangIndex = App.SettingsPreferences.SelectedLanguageIndex;
            //IsAutomaticTranslate = App.SettingsPreferences.AutomaticTranslate;

            SwitchNotificationCommand = new Command(OnSwitchNotification);
            SwitchNSFWCommand = new Command(OnSwitchNSFW);
            ClearDatabaseCommand = new magno.AsyncCommand(OnClearDatabase);
            OverviewCommand = new magno.AsyncCommand(OnOverview);
            PatchNotesCommand = new magno.AsyncCommand(OnPatchNotes);
            LicensesCommand = new magno.AsyncCommand(OnLicenses);
        }

        public Task InitializeTask { get; }

        public async Task LoadAsync(object param)
        {
            await Task.Run(() =>
            {
                DatabaseInfo = new DatabaseInfo<ErrorLog>(App.liteErrorLogDB);
            });
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

        private DatabaseInfo<ErrorLog> _databaseInfo;
        public DatabaseInfo<ErrorLog> DatabaseInfo
        {
            get { return _databaseInfo; }
            set { SetProperty(ref _databaseInfo, value); }
        }

        private string _currentVersion;
        public string CurrentVersion
        {
            get { return _currentVersion; }
            set { SetProperty(ref _currentVersion, value); }
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

        #region commands


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
            if (Settings != null)
            {
                App.liteDB.GetCollection<SettingsPreferences>().Upsert(0, Settings);
            }
        }

        public ICommand ClearDatabaseCommand { get; private set; }
        private async Task OnClearDatabase()
        {
            //TODO: ficar de olho em uma atualização para isso, rebuild sempre lança exceção de timeout com ou sem uma task

            bool canNavigate = await NavigationManager.CanPopUpNavigateAsync<ChoiceModalViewModel>();

            if (canNavigate)
            {
                var action = new Action(() =>
                {
                    if (App.liteErrorLogDB == DatabaseInfo.LiteDatabase)
                        DatabaseInfo.LiteDatabase.GetCollection<ErrorLog>().DeleteAll();

                    DatabaseInfo.DatabaseSize = DatabaseInfo.GetDatabaseSize();
                    DatabaseInfo.CollectionDataCount = DatabaseInfo.GetDatabaseCollectionDataCount();
                });

                await NavigationManager.NavigatePopUpAsync<ChoiceModalViewModel>(
                    Lang.Lang.ClearDatabase,
                    string.Format(Lang.Lang.DropDatabase, DatabaseInfo.GetDatabaseName()), action);
            }
        }

        public ICommand OverviewCommand { get; private set; }
        private async Task OnOverview()
        {
            await Launcher.TryOpenAsync(UsefulLinksConsts.Overview);
        }

        public ICommand PatchNotesCommand { get; private set; }
        private async Task OnPatchNotes()
        {
            await Launcher.TryOpenAsync(UsefulLinksConsts.Releases);
        }

        public ICommand LicensesCommand { get; private set; }
        private async Task OnLicenses()
        {
            await Launcher.TryOpenAsync(UsefulLinksConsts.Licenses);
        }

        #endregion
    }
}
