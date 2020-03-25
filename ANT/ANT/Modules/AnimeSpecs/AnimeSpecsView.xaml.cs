using ANT.Core;
using JikanDotNet;
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
    public partial class AnimeSpecsView : TabbedPage
    {
        public AnimeSpecsView()
        {
            InitializeComponent();

        }

        private void AnimeSpecsView_Popped(object sender, NavigationEventArgs e)
        {
            if (sender is AnimeSpecsView view && view == this)
            {
                (view.BindingContext as AnimeSpecsViewModel).Dispose();
                ((Xamarin.Forms.NavigationPage)App.Current.MainPage).Popped -= AnimeSpecsView_Popped;
            }
        }
    }
}