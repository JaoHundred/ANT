using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ANT.Droid.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ResolutionGroupName("ANT.Effects")]
[assembly: ExportEffect(typeof(StepperColorEffect), nameof(StepperColorEffect))]
namespace ANT.Droid.Effects
{

    public class StepperColorEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
                //TODO: procurar depois o uso disso que não é deprecated
            if (Element is Stepper element && Control is LinearLayout control)
            {
                //control.GetChildAt(0).Background.SetColorFilter(
                //    new BlendModeColorFilter(ANT.Effects.StepperColorEffect.GetColor(element).ToAndroid(), BlendMode.Multiply));
                //control.GetChildAt(1).Background.SetColorFilter(
                //    new BlendModeColorFilter(ANT.Effects.StepperColorEffect.GetColor(element).ToAndroid(), BlendMode.Multiply));

                control.GetChildAt(0).Background.SetColorFilter(ANT.Effects.StepperColorEffect.GetColor(element).ToAndroid(), PorterDuff.Mode.Multiply);
                control.GetChildAt(1).Background.SetColorFilter(ANT.Effects.StepperColorEffect.GetColor(element).ToAndroid(), PorterDuff.Mode.Multiply);
            }
        }

        protected override void OnDetached()
        {
            if (Element is Stepper element && Control is LinearLayout control)
            {
                //control.GetChildAt(0).Background.SetColorFilter(
                //    new BlendModeColorFilter(Xamarin.Forms.Color.Gray.ToAndroid(), BlendMode.Multiply));
                //control.GetChildAt(1).Background.SetColorFilter(
                //    new BlendModeColorFilter(Xamarin.Forms.Color.Gray.ToAndroid(), BlendMode.Multiply));

                control.GetChildAt(0).Background.SetColorFilter(Xamarin.Forms.Color.Gray.ToAndroid(), PorterDuff.Mode.Multiply);
                control.GetChildAt(1).Background.SetColorFilter(Xamarin.Forms.Color.Gray.ToAndroid(), PorterDuff.Mode.Multiply);
            }
        }
    }
}