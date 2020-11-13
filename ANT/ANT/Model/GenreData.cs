using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;
using JikanDotNet;
using ANT.Interfaces;

namespace ANT.Model
{
    public class GenreData : ObservableObject, ICheckableObject
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

    public class EqualityGenreData : IEqualityComparer<GenreData>
    {
        public bool Equals(GenreData x, GenreData y)
        {
            return x.Genre == x.Genre;
        }

        public int GetHashCode(GenreData obj)
        {
            int hashCode = -193460375;
            hashCode = hashCode * -1521134295 + obj.IsChecked.GetHashCode();
            hashCode = hashCode * -1521134295 + obj.Genre.GetHashCode();
            return hashCode;
        }
    }
}
