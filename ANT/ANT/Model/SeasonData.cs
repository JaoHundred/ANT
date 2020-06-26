using ANT.Lang;
using JikanDotNet;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;

namespace ANT.Model
{
    public class SeasonData : ObservableObject
    {

        public SeasonData(int? year, string season, int minYear, int maxYear)
        {
            Seasons = new List<string>
            {
                Lang.Lang.Spring,
                Lang.Lang.Summer,
                Lang.Lang.Fall,
                Lang.Lang.Winter
            };

            SeasonKeys = new List<Seasons>
            {
                JikanDotNet.Seasons.Spring,
                JikanDotNet.Seasons.Summer,
                JikanDotNet.Seasons.Fall,
                JikanDotNet.Seasons.Winter
            };

            Years = new List<int>();
            for (int i = minYear; i <= maxYear + 10; i++)
                Years.Add(i);

            //(JikanDotNet.Seasons)Enum.Parse(typeof(JikanDotNet.Seasons), season)
            SelectedYear = year;
            SelectedSeason = Seasons.First(p => p.ToLower() == season.ToLower());
        }

        public IList<int> Years { get; }
        public IList<string> Seasons { get; }
        public IList<Seasons> SeasonKeys { get; }

        private int? _selectedYear;
        public int? SelectedYear
        {
            get { return _selectedYear; }
            set { SetProperty(ref _selectedYear, value); }
        }

        private string _selectedSeason;
        public string SelectedSeason
        {
            get { return _selectedSeason; }
            set { SetProperty(ref _selectedSeason, value); }
        }
    }
}
