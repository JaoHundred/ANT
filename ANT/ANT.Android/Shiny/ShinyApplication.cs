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
using ANT.ShinyStart;
using sh = Shiny;
using Shiny.Notifications;

namespace ANT.Droid.Shiny
{
#if DEBUG
    [Application(Debuggable = true)]
#else
[Application(Debuggable = false)]
#endif
    class ShinyApplication : sh.ShinyAndroidApplication<StartUp>
    {
        public ShinyApplication(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            Xamarin.Essentials.Platform.Init(this);

            //AndroidOptions.DefaultSmallIconResourceName = "notificationIcon.png";
            //AndroidOptions.DefaultLaunchActivityFlags = AndroidActivityFlags.FromBackground;

            //sh.AndroidShinyHost.Init(this);
        }
    }
}