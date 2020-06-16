using Android.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace ANT.Interfaces
{
    public interface IWork
    {
        void CreateWorkAndKeep(string workId, TimeSpan executionInterval);
        void CreateWorkAndReplaceExisting(string workId, TimeSpan executionInterval);
        void CancelWork(string workId);
        void CancelAllWorks();
    }
}
