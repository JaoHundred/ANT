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
using AndroidX;
using AndroidX.Work;
using ANT.Model;
using ANT.Core;

[assembly: UsesPermission(Manifest.Permission.ReceiveBootCompleted)]
namespace ANT.Droid.Broadcast
{

    [BroadcastReceiver(Enabled = true, Exported = true, DirectBootAware = true)]
    [IntentFilter(new[] { Intent.ActionLockedBootCompleted })]
    class BootBroadcastReceiver : BroadcastReceiver
    {
        //TODO:aparentemente o workmanager já escuta o bootreceiver segundo 
        //https://stackoverflow.com/questions/53043183/how-to-register-a-periodic-work-request-with-workmanger-system-wide-once-i-e-a
        // na resposta correta o IgorGanapolsky respondeu que ele já escuta o boot_completed
        //testar desativar essa classe e seus apetrechos de atributos para ver se o boot ainda acontece
        //se acontecer, deletar essa arquivo/classe

        public override void OnReceive(Context context, Intent intent)
        {
            OnReceive(context, intent);

            //if (App.liteDB == null)
            //    App.StartLiteDB();

            //var bd = ANT.App.liteDB.GetCollection<SettingsPreferences>();
            //var settings = bd.FindById(0);

            //if (settings == null)
            //{
            //    settings = new SettingsPreferences();
            //    ANT.App.liteDB.GetCollection<SettingsPreferences>().Upsert(0, settings);
            //}
        }
    }
}