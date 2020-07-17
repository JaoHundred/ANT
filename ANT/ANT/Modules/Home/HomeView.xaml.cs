using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ANT.Modules
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomeView : ContentPage
    {
        public HomeView()
        {
            InitializeComponent();
            BindingContext = new HomeViewModel();
        }

        private async void ContentPage_Appearing(object sender, EventArgs e)
        {
            await ((HomeViewModel)BindingContext).LoadAsync();
        }

        private void ContentPage_Disappearing(object sender, EventArgs e)
        {
            ((HomeViewModel)BindingContext).Unload();
        }
    }
}