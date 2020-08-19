using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;

namespace ANT.Droid
{
    [Activity(Theme = "@style/MainTheme.Splash", Label = "ANT", Icon = "@mipmap/ic_launcher", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashActivity : AppCompatActivity
    {
        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
        }

        protected override void OnResume()
        {
            base.OnResume();
            Task startupWork = new Task(() => { Startup(); });
            startupWork.Start();
        }

        private void Startup()
        {
            //disable activity slide-in animation

            //load async data, if has any

            StartActivity(typeof(MainActivity));
            OverridePendingTransition(0, 0);
            Finish();
        }
    }
}