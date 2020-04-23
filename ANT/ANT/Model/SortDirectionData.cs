using System;
using System.Collections.Generic;
using System.Text;
using JikanDotNet;

namespace ANT.Model
{
    public class SortDirectionData
    {
        public SortDirectionData(SortDirection sortDirection, bool isChecked = false)
        {
            SortDirection = sortDirection;
            IsChecked = isChecked;
        }

        public SortDirection SortDirection { get; set; }

        public bool IsChecked { get; set; }

    }
}
