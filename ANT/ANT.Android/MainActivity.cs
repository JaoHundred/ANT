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
using System.Linq;
using Plugin.LocalNotification;
using Android.Content;

[assembly: Xamarin.Forms.Dependency(typeof(ANT.Droid.MainActivity))]
namespace ANT.Droid
{
    [Activity(Label = "ANT", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);


            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(true);
            NotificationCenter.CreateNotificationChannel(
                        new Plugin.LocalNotification.Platform.Droid.NotificationChannelRequest
                        {
                            Id = Consts.NotificationChannelTodayAnime,
                            Name = "Today Animes",
                            Description = "General",
                        });

            LoadApplication(new App());
            NotificationCenter.NotifyNotificationTapped(Intent);
        }

        private readonly string _rootRoute = "Home";

        private static Page GetCurrentPage()
            => (Shell.Current?.CurrentItem?.CurrentItem as IShellSectionController)?.PresentedPage;

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
                var page = GetCurrentPage();

                if (page?.BindingContext is BaseVMExtender vm && vm.IsMultiSelect) // se estou com a multi seleção ativa, fecho
                {
                    vm.SingleSelectionMode();
                    return;
                }
                if (page?.BindingContext is BaseVMExtender vmm && vmm.SearchQuery?.Length > 0)//se a barra de pesquisa na navigation tiver preenchida, apague o texto
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
                    {
                        Page currentpage = GetCurrentPage();

                        switch (currentpage)
                        {
                            case AnimeSpecsView specsView:
                                ((AnimeSpecsViewModel)specsView.BindingContext).BackButtonCommand.Execute(BackButtonOriginEnum.Hardware);
                                await NavigationManager.PopShellPageAsync();
                                break;

                            case CatalogueView catalogueView:
                                ((CatalogueViewModel)catalogueView.BindingContext).BackButtonCommand.Execute(catalogueView);
                                break;

                            default:
                                await NavigationManager.PopShellPageAsync();
                                break;
                        }
                    }

                    else if (stackCount == 1 && route == _rootRoute) // estou na home
                        base.OnBackPressed();
                }
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnNewIntent(Intent intent)
        {
            NotificationCenter.NotifyNotificationTapped(intent);
            base.OnNewIntent(intent);
        }

    }
}