using System;
using System.Collections.Generic;
using System.IO;
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

[assembly: Xamarin.Forms.Dependency(typeof(GetAndroidFolderService))]
namespace ANT.Droid.Services
{
    public class GetAndroidFolderService : IGetFolder
    {
        public string GetApplicationDocumentsFolder()
        {
            string fullDirectory = Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
            return fullDirectory;
        }
    }
}