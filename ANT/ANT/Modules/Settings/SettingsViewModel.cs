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
using Shiny;
using Shiny.Notifications;
using Shiny.Jobs;
using ANT.UTIL;

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
            DataDirectory = DependencyService.Get<IGetFolder>().GetApplicationDocumentsFolder();
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

        private string _dataDirectory;
        public string DataDirectory
        {
            get { return _dataDirectory; }
            set { SetProperty(ref _dataDirectory, value); }
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

                    Task.Run(async() => 
                    {
                        await App.CancelJobAsync(WorkManagerConsts.AnimesNotificationWorkId);
                        await App.RunJobAsync(typeof(NotificationJob), WorkManagerConsts.AnimesNotificationWorkId);
                    });

                    App.liteDB.GetCollection<SettingsPreferences>().Upsert(0, Settings);

                    SetProperty(ref _timeToNotify, value);
                }
            }
        }

        #region commands
        public ICommand SwitchNotificationCommand { get; private set; }
        private async void OnSwitchNotification()
        {
            if (Settings != null)
            {
                if (Settings.NotificationsIsOn)
                    await App.RunJobAsync(typeof(NotificationJob), WorkManagerConsts.AnimesNotificationWorkId);
                else
                    await App.CancelJobAsync(WorkManagerConsts.AnimesNotificationWorkId);

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
            await LauncherHelper.OpenLinkAsync(UsefulLinksConsts.Overview);
        }

        public ICommand PatchNotesCommand { get; private set; }
        private async Task OnPatchNotes()
        {
            await LauncherHelper.OpenLinkAsync(UsefulLinksConsts.Releases);
        }

        public ICommand LicensesCommand { get; private set; }
        private async Task OnLicenses()
        {
            await LauncherHelper.OpenLinkAsync(UsefulLinksConsts.Licenses);
        }

        #endregion
    }
}
