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
using ANT.Droid.Services;
using ANT.Interfaces;

[assembly: Xamarin.Forms.Dependency(typeof(ToastService))]
namespace ANT.Droid.Services
{
    public class ToastService : IToast
    {
        public void MakeToastMessageLong(string message)
        {
            Toast.MakeText(Android.App.Application.Context, message, ToastLength.Long).Show();
        }

        public void MakeToastMessageShort(string message)
        {
            Toast.MakeText(Android.App.Application.Context, message, ToastLength.Short).Show();
        }
    }
}