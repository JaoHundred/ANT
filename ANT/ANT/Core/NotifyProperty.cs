using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ANT.UTIL
{
    public class NotifyProperty : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        protected void Changed<T>(ref T property, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(property, newValue))
            {
                property = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
