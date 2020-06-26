using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ANT.Core;
using ANT.Droid.Helpers;
using ANT.Model;

//[assembly: UsesPermission(Manifest.Permission.SetAlarm)]
namespace ANT.Droid.Broadcast
{
    [BroadcastReceiver(Enabled = true)]
    public class FavoriteAnimesAlarm : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            //TODO: se o work manager funcionar, apagar o conteúdo desse método
            //AlarmManagerHelper.OnAlarm();
        }
    }
}