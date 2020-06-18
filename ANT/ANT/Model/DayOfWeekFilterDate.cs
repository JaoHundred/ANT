using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace ANT.Model
{
    public class DayOfWeekFilterDate : ObservableObject
    {
        public DayOfWeekFilterDate(TodayDayOfWeek todayDayOfWeek, bool isChecked = false)
        {
            TodayDayOfWeek = todayDayOfWeek;
            IsChecked = isChecked;
        }

        public TodayDayOfWeek TodayDayOfWeek { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetProperty(ref _isChecked, value); }
        }
    }

    public enum TodayDayOfWeek
    {
        Today,
        Unknown,
        FinishedAiring,
        NotStarted,
        Sunday,
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
    };
}
