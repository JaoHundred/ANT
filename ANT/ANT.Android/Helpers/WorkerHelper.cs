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

        public void CancelWork(string workId)
        {
            WorkManager.GetInstance(Xamarin.Essentials.Platform.AppContext).CancelAllWorkByTag(workId);
        }

        public void CreatePeriodicWorkAndKeep(string workId, TimeSpan executionInterval)
        {
            PeriodicWorkRequest notificationWorker = CreateConstraints(executionInterval);

            WorkManager.GetInstance(Xamarin.Essentials.Platform.AppContext)
                .EnqueueUniquePeriodicWork(workId, ExistingPeriodicWorkPolicy.Keep, notificationWorker);
        }

        private static PeriodicWorkRequest CreateConstraints(TimeSpan executionInterval)
        {
            var constraints = new Constraints();
            constraints.SetRequiresStorageNotLow(false);
            constraints.SetRequiresBatteryNotLow(false);

            var notificationWorker = PeriodicWorkRequest.Builder.From<NotificationWorker>(executionInterval)
            .SetConstraints(constraints).Build();
            return notificationWorker;
        }

        public void CreatePeriodicWorkAndReplaceExisting(string workId, TimeSpan executionInterval)
        {
            PeriodicWorkRequest notificationWorker = CreateConstraints(executionInterval);

            WorkManager.GetInstance(Xamarin.Essentials.Platform.AppContext)
                .EnqueueUniquePeriodicWork(workId, ExistingPeriodicWorkPolicy.Replace, notificationWorker);
        }
    }
}