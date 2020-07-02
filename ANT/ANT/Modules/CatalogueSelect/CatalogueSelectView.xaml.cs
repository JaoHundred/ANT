using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamanimation.Behaviors;

namespace ANT.Modules
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CatalogueSelectView : ContentPage
    {
        public CatalogueSelectView()
        {
            InitializeComponent();
            BindingContext = new CatalogueSelectViewModel();
        }
    }
}