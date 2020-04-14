using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;
using JikanDotNet;

namespace ANT.Model
{
    public class GenreData : ObservableObject
    {
        public GenreData(GenreSearch genre)
        {
            Genre = genre;
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetProperty(ref _isChecked, value); }
        }

        public GenreSearch Genre { get; set; }
    }
}
