using ANT.Lang;
using JikanDotNet;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace ANT.Model
{
    public class SeasonData : ObservableObject
    {

        public SeasonData(int? year, string season, int minYear, int maxYear)
        {
            Seasons = new List<Seasons>
            {
                JikanDotNet.Seasons.Spring,
                JikanDotNet.Seasons.Summer,
                JikanDotNet.Seasons.Fall,
                JikanDotNet.Seasons.Winter
            };

            Years = new List<int>();
            for (int i = minYear; i <= maxYear + 10; i++)
                Years.Add(i);

            SelectedYear = year;
            SelectedSeason = (JikanDotNet.Seasons)Enum.Parse(typeof(JikanDotNet.Seasons), season);
        }

        public IList<int> Years { get; }
        public IList<Seasons> Seasons { get; }

        private int? _selectedYear;
        public int? SelectedYear
        {
            get { return _selectedYear; }
            set { SetProperty(ref _selectedYear, value); }
        }

        private Seasons _selectedSeason;
        public Seasons SelectedSeason
        {
            get { return _selectedSeason; }
            set { SetProperty(ref _selectedSeason, value); }
        }
    }
}
