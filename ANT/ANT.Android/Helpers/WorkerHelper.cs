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
using ANT.Droid.Scheduler;

namespace ANT.Droid.Helpers
{
    public static class WorkerHelper
    {
        public static void WorkSheduler(Context context, int jobId, TimeSpan periodicInterval, ExistingPeriodicWorkPolicy periodicWorkPolicy)
        {
            var constraints = new Constraints();
            constraints.SetRequiresBatteryNotLow(true);
            constraints.SetRequiresStorageNotLow(true);

            var notificationWorker = PeriodicWorkRequest.Builder.From<NotificationWorker>(periodicInterval)
            .SetConstraints(constraints).Build();

            WorkManager.GetInstance(context).EnqueueUniquePeriodicWork(jobId.ToString(), periodicWorkPolicy, notificationWorker);
        }
    }
}