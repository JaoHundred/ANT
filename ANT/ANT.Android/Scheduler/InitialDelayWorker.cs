using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Work;
using ANT.Droid.Helpers;
using ANT.Core;

namespace ANT.Droid.Scheduler
{
    public class InitialDelayWorker : Worker
    {
        public InitialDelayWorker(Context context, WorkerParameters workerParams) : base(context, workerParams)
        {
        }

        public override Result DoWork()
        {

            new WorkerHelper().CreatePeriodicWorkAndReplace(WorkManagerConsts.AnimesNotificationWorkId, TimeSpan.FromDays(1));

            return new Result.Success();
        }
    }
}