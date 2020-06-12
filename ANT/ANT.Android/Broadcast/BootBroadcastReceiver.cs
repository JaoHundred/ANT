﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.App.Job;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ANT.Droid.Helpers;
using ANT.Droid.Scheduler;

[assembly: UsesPermission(Manifest.Permission.ReceiveBootCompleted)]
namespace ANT.Droid.Broadcast
{

    [BroadcastReceiver(Enabled = true, Exported = true, DirectBootAware = true)]
    [IntentFilter(new[] { Intent.ActionLockedBootCompleted })]
    class BootBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            OnReceive(context, intent);

            JobSchedulerHelper.ScheduleJob(context, 0);
        }
    }
}