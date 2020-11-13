using System;
using System.Collections.Generic;
using System.Text;
using ANT.Interfaces;
using JikanDotNet;
using MvvmHelpers;

namespace ANT.Model
{
    public class OrderData : ObservableObject, ICheckableObject
    {

        public OrderData(AnimeSearchSortable animeSearchSortable, bool isChecked = false)
        {
            OrderBy = animeSearchSortable;
            IsChecked = isChecked;
        }

        public AnimeSearchSortable OrderBy { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetProperty(ref _isChecked, value); }
        }

    }
}
