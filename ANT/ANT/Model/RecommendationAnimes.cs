using JikanDotNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace ANT.Model
{
    public class RecommendationAnimes
    {
        public RecommendationAnimes()
        {}

        public int Id { get; set; }
        public IEnumerable<Recommendation> Recommendations { get; set; }
        
        private DateTime _lastRecommendationDate;
        public DateTime LastRecommendationDate
        {
            get { return _lastRecommendationDate; }
            set { _lastRecommendationDate = value.ToLocalTime(); }
        }

        public bool ShowNSFW { get; set; }
    }
}
