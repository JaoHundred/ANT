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
            Toast toast = Toast.MakeText(Android.App.Application.Context, message, ToastLength.Long);

            SetFontSize(toast, 20);

            toast.Show();
        }

        private static void SetFontSize(Toast toast, int fontSize)
        {
            ViewGroup toastView = (ViewGroup)toast.View;

            if (toastView.ChildCount > 0 && toastView.GetChildAt(0) is TextView)
            {
                var textView = (TextView)toastView.GetChildAt(0);
                textView.SetTextSize(Android.Util.ComplexUnitType.Sp, fontSize);
            }
        }

        public void MakeToastMessageShort(string message)
        {
            Toast toast = Toast.MakeText(Android.App.Application.Context, message, ToastLength.Short);

            SetFontSize(toast, 20);

            toast.Show();
        }
    }
}