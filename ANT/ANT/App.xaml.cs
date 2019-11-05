﻿using ANT.Core;
using ANT.Modules;
using System;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ANT
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }

        protected async override void OnStart()
        {
            // Handle when your app starts
            //await ThemeManager.LoadThemeAsync();
            //await CultureManager.LoadCultureAsync();

            //Debug.WriteLine(App.Current.Properties.Keys);
            //App.Current.Properties.Clear();
            //await App.Current.SavePropertiesAsync();
            //Debug.WriteLine(App.Current.Properties.Keys);
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
