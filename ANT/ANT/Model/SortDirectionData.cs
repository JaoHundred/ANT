﻿using System;
using System.Collections.Generic;
using System.Text;
using ANT.Interfaces;
using JikanDotNet;
using MvvmHelpers;

namespace ANT.Model
{
    public class SortDirectionData : ObservableObject, ICheckableObject
    {
        public SortDirectionData(SortDirection sortDirection, bool isChecked = false)
        {
            SortDirection = sortDirection;
            IsChecked = isChecked;
        }

        public SortDirection SortDirection { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetProperty(ref _isChecked, value); }
        }
    }
}
