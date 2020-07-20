using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Xaml;

namespace ANT.Modules
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            BindingContext = new AppShellViewModel();
        }

        protected async override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == "CurrentItem" && Device.RuntimePlatform == Device.Android)
                await Task.Delay(300);
            base.OnPropertyChanged(propertyName);
        }
    }
}