using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Forms;
using ANT.Core;
using Android.Util;
using ANT.Modules;
using ANT.Interfaces;
using MvvmHelpers;

[assembly: Xamarin.Forms.Dependency(typeof(ANT.Droid.MainActivity))]
namespace ANT.Droid
{
    [Activity(Label = "ANT", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IMainPageAndroid
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        public override void OnBackPressed()
        {
            if (_currentVm is CatalogueViewModel ctl && ctl.IsMultiSelect)
                ctl.SelectionModeCommand.Execute(null);
            else
            {
                //TODO: atualmente, pressionar o botão de voltar faz o app fechar e retornar para a página raiz do shell, descobrir se isso é normal ou se é um bug
                base.OnBackPressed();
            }
        }

       

        private static BaseViewModel _currentVm;
        public void OnBackPress(BaseViewModel vm)
            => _currentVm = vm;

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


    }
}