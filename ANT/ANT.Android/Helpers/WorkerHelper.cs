using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.App.Job;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Work;
using ANT.Droid.Helpers;
using ANT.Droid.Scheduler;
using ANT.Interfaces;
using Xamarin.Forms;

[assembly: Dependency(typeof(WorkerHelper))]
namespace ANT.Droid.Helpers
{
    public class WorkerHelper : IWork
    {
        public void CancelAllWorks()
        {
            WorkManager.GetInstance(Xamarin.Essentials.Platform.AppContext).CancelAllWork();
        }

        public void CancelWork(params string[] workIds)
        {
            foreach (var work in workIds)
                WorkManager.GetInstance(Xamarin.Essentials.Platform.AppContext).CancelUniqueWork(work);
        }

        public void CreateOneTimeWorkAndKeep(string workId, TimeSpan triggerAt)
        {
            var constraints = new Constraints();
            constraints.SetRequiresStorageNotLow(false);
            constraints.SetRequiresBatteryNotLow(false);

            var notificationWorker = OneTimeWorkRequest.Builder.From<InitialDelayWorker>()
            .SetConstraints(constraints).SetInitialDelay(triggerAt).Build();

            WorkManager.GetInstance(Xamarin.Essentials.Platform.AppContext)
                .EnqueueUniqueWork(workId, ExistingWorkPolicy.Keep, notificationWorker);
        }

        public void CreatePeriodicWorkAndReplace(string workId, TimeSpan interval)
        {
            var constraints = new Constraints();
            constraints.SetRequiresStorageNotLow(false);
            constraints.SetRequiresBatteryNotLow(false);

            var notificationWorker = PeriodicWorkRequest.Builder.From<NotificationWorker>(interval)
            .SetConstraints(constraints).Build();

            WorkManager.GetInstance(Xamarin.Essentials.Platform.AppContext)
                .EnqueueUniquePeriodicWork(workId, ExistingPeriodicWorkPolicy.Replace, notificationWorker);
        }

        public TimeSpan InitialDelay(TimeSpan triggerAt)
        {
            TimeSpan duration;
            if (new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0) == triggerAt)
                duration = TimeSpan.FromSeconds(1);
            else if (DateTime.Now.TimeOfDay > triggerAt)
                duration = (DateTime.Now.TimeOfDay - triggerAt.Add(TimeSpan.FromDays(1))).Duration();
            else
                duration = (DateTime.Now.TimeOfDay - triggerAt).Duration();
            return duration;
        }
    }
}