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
using ANT.Droid.Scheduler;

namespace ANT.Droid.Helpers
{
    public static class JobSchedulerHelper
    {
        public static JobInfo.Builder CreateJobBuilderUsingJobId<T>(this Context context, int jobId) where T : JobService
        {
            var javaClass = Java.Lang.Class.FromType(typeof(T));
            var componentName = new ComponentName(context, javaClass);
            return new JobInfo.Builder(jobId, componentName);
        }

        public static void ScheduleJob(Context context, int jobId)
        {
            var job = (JobScheduler)context.GetSystemService(Context.JobSchedulerService);

            if (job.GetPendingJob(jobId) == null)
            {
                long schedulerInterval = Convert.ToInt64(TimeSpan.FromDays(1).TotalMilliseconds);
                long flexInterval = Convert.ToInt64(TimeSpan.FromDays(1).Add(TimeSpan.FromMinutes(20)).TotalMilliseconds);
                long backOffInterval = Convert.ToInt64(TimeSpan.FromMinutes(3).TotalMilliseconds);

                var jobBuilder = context.CreateJobBuilderUsingJobId<NotificationSheduler>(jobId)
                    .SetPeriodic(schedulerInterval, flexInterval)
                    .SetRequiresBatteryNotLow(true)
                    .SetBackoffCriteria(backOffInterval, BackoffPolicy.Exponential)
                    .SetPersisted(true);

                int result = job.Schedule(jobBuilder.Build());

                if (result != JobScheduler.ResultSuccess)
                {
                 //TODO: salvar aqui dados de log para job mal sucedido   
                }
            }
        }
    }
}