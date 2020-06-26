using Android.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace ANT.Interfaces
{
    public interface IWork
    {
        TimeSpan InitialDelay(TimeSpan triggerAt);
        void CreateOneTimeWorkAndKeep(string workId, TimeSpan triggerAt);
        void CreatePeriodicWorkAndReplace(string workId, TimeSpan interval);
        void CancelWork(params string[] workId);
        void CancelAllWorks();
    }
}
