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
    public partial class CatalogueView : ContentPage
    {
        public CatalogueView()
        {
            InitializeComponent();
            BindingContext = new CatalogueViewModel();
        }
    }
}