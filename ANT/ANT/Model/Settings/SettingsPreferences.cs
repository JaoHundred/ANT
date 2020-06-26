using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;
using Java.Sql;

namespace ANT.Model
{
    public class SettingsPreferences
    {
        public SettingsPreferences()
        {
            AutomaticTranslate = true;
            NotificationsIsOn = true;
            HourToNotify = TimeSpan.FromHours(12);
            LastNotifyDate = DateTime.Today; // today inicia em 12h do dia
        }

        [BsonId(autoId: true)]
        public int Id { get; set; }

        public bool AutomaticTranslate { get; set; }

        public int SelectedThemeIndex { get; set; }

        public int SelectedLanguageIndex { get; set; }

        public bool NotificationsIsOn { get; set; }

        public TimeSpan HourToNotify { get; set; }

        /// <summary>
        /// propriedade usada para avisar que já foi notificado uma única vez
        /// ao recriar o alarme, setar como falso
        /// </summary>
        private DateTime _astNotifyDate;
        public DateTime LastNotifyDate
        {
            get { return _astNotifyDate; }
            set { _astNotifyDate = value.ToLocalTime(); }
        }
    }
}
