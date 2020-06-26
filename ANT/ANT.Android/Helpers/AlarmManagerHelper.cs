using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ANT.Core;
using ANT.Droid.Broadcast;
using ANT.Droid.Helpers;
using ANT.Interfaces;
using ANT.Model;
using Java.Sql;

[assembly: Xamarin.Forms.Dependency(typeof(AlarmManagerHelper))]
namespace ANT.Droid.Helpers
{
    public class AlarmManagerHelper : IAlarm
    {
        /// <summary>
        /// checa internamente se o alarme existe, se não existir cria um
        /// </summary>
        public void StartAlarmRTCWakeUp(TimeSpan hourToTrigger, int alarmCode, TimeSpan interval)
        {
            Context androidContext = Xamarin.Essentials.Platform.AppContext;
            var alarmIntent = new Intent(androidContext, typeof(FavoriteAnimesAlarm));

            AlarmManager alarmManager = (AlarmManager)androidContext.GetSystemService(Android.Content.Context.AlarmService);

            var pendingIntent = PendingIntent.GetBroadcast(androidContext, alarmCode, alarmIntent, PendingIntentFlags.UpdateCurrent);

            long triggerAtMilis = Convert.ToInt64(hourToTrigger.TotalMilliseconds);
            long intervalMilis = Convert.ToInt64(interval.TotalMilliseconds);

            alarmManager.SetRepeating(AlarmType.RtcWakeup, triggerAtMilis, intervalMilis, pendingIntent);
        }

        /// <summary>
        /// checa internamente se o alarme existe, se já existir cancela ele
        /// </summary>
        /// <param name="alarmCode"></param>
        public void CancelAlarm(int alarmCode)
        {
            Context androidContext = Xamarin.Essentials.Platform.AppContext;
            var alarmIntent = new Intent(androidContext, typeof(FavoriteAnimesAlarm));

            AlarmManager alarmManager = (AlarmManager)androidContext.GetSystemService(Android.Content.Context.AlarmService);
            PendingIntent pendingIntent = GetExistingAlarm(alarmCode, androidContext, alarmIntent);

            alarmManager.Cancel(pendingIntent);
            pendingIntent.Cancel();
        }

        private static PendingIntent GetExistingAlarm(int alarmCode, Context androidContext, Intent alarmIntent)
        {
            return PendingIntent.GetBroadcast(androidContext, alarmCode, alarmIntent, PendingIntentFlags.NoCreate);
        }

        public static void OnAlarm()
        {
            try
            {
                if (App.liteDB == null)
                    App.StartLiteDB();

                var settings = App.liteDB.GetCollection<SettingsPreferences>().FindById(0);

                //TODO testar condição abaixo
                //se o horário que marquei para notificar é menor que o horário de agora
                //assume que o alarme já disparou
                if (settings.HourToNotify < DateTime.Now.TimeOfDay)
                {
#if DEBUG
                    Console.WriteLine("Alarme já disparou hoje {0}", settings.LastNotifyDate);
#endif
                    return;
                }

                var animes = App.liteDB.GetCollection<FavoritedAnime>()
                    .Find(p => p.CanGenerateNotifications && p.NextStreamDate != null && p.Anime.Airing);
                DateTime now = DateTime.Now;

                foreach (var anime in animes)
                {
                    DateTime nextStream = anime.NextStreamDate.Value;

                    if (nextStream.DayOfWeek == now.DayOfWeek)
                        ANT.Core.NotificationManager.CreateNotificationAsync(anime, Consts.NotificationChannelTodayAnime, DateTime.Now);
                }

#if DEBUG
                Console.WriteLine("Alarme FavoriteAnimesAlarm chamado em {0}", DateTime.Now);
#endif
            }
            catch (Exception ex)
            {
                //TODO:salvar aqui os dados do que deu errado
            }
        }
    }
}