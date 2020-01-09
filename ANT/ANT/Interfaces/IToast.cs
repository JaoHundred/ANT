using System;
using System.Collections.Generic;
using System.Text;

namespace ANT.Interfaces
{
    public interface IToast
    {
        void MakeToastMessageLong(string message);
        void MakeToastMessageShort(string message);
    }
}
