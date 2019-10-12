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
using ANT.Core;
using ANT.Droid.NativeService;
using Xamarin.Forms;

[assembly: Dependency(typeof(NativeChangeThemeService))]
namespace ANT.Droid.NativeService
{
    public class NativeChangeThemeService : INativeChangeThemeService
    {
        public void onThemeChanged(ThemeManager.Themes theme)
        {
            MainActivity activity = MainActivity.Instance;
            var intent = MainActivity.Instance.Intent;
            activity.Finish();
            activity.StartActivity(intent);
        }
    }
}