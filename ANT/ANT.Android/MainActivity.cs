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
using ANT.UTIL;

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

        private readonly string _rootRoute = "Home";

        public override async void OnBackPressed()
        {
            if (Shell.Current.FlyoutIsPresented) // se o hamburger menu está aberto, fecho
            {
                Shell.Current.FlyoutIsPresented = false;
                return;
            }
            else
            {
                if (_currentVm is BaseVMSelectionModeExtender vm && vm.IsMultiSelect) // se estou com a multi seleção ativa, fecho
                {
                    vm.SingleSelectionMode();
                    return;
                }

                else
                {
                    string route = Shell.Current.CurrentItem.Route;
                    int stackCount = App.Current.MainPage.Navigation.NavigationStack.Count;

                    if (stackCount == 1 && route != _rootRoute)// estou na raiz da pilha e não estou na home
                        await Shell.Current.GoToAsync($"///{_rootRoute}");
                    else if (stackCount == 1 && route == _rootRoute || stackCount > 1) // estou na home ou qualquer página hierárquica
                        base.OnBackPressed();

                }
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