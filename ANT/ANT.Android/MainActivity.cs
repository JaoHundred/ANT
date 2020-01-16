﻿using System;

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
            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);

            LoadApplication(new App());
        }

        private readonly string _rootRoute = "Home";

        public override async void OnBackPressed()
        {
            //TODO: tentar fazer uma solução com MessagingCenter? se for boa, descobrir como passar a viewmodel atual
            if (Shell.Current.FlyoutIsPresented) // se o hamburger menu está aberto, fecho
            {
                Shell.Current.FlyoutIsPresented = false;
                return;
            }
            else if (!Rg.Plugins.Popup.Popup.SendBackPressed())//se não é um modal, posso voltar, o botão de retorno dos modais são lidados direto dos PopUpPage
            {
                if (_currentVm is BaseVMExtender vm && vm.IsMultiSelect) // se estou com a multi seleção ativa, fecho
                {
                    vm.SingleSelectionMode();
                    return;
                }
                if (_currentVm is BaseVMExtender vmm && vmm.SearchQuery?.Length > 0)//se a barra de pesquisa na navigation tiver preenchida, apague o texto
                {
                    vmm.SearchQuery = string.Empty;
                    return;
                }

                else
                {
                    string route = Shell.Current.CurrentItem.Route;
                    int stackCount = App.Current.MainPage.Navigation.NavigationStack.Count;

                    if (stackCount == 1 && route != _rootRoute)// estou na raiz da pilha e não estou na home
                        await Shell.Current.GoToAsync($"///{_rootRoute}", animate: true);
                    else if (stackCount > 1) // estou em qualquer página hierárquica
                        //TODO:xamarin forms está com bug no retorno da animação
                        //quando corrigirem, usar somente o base.OnBackPressed e fungir os 2 else if "else if("stackCount == 1 && route == _rootRoute ||stackCount > 1)
                        await NavigationManager.PopShellPageAsync(animated: false);

                    //TODO: quando isso é chamado em páginas hierárquicas o retorno da uma leve engasgada
                    //, descobrir o que pode ser, não dá pra usar async await nessa linha
                    else if (stackCount == 1 && route == _rootRoute) // estou na home
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