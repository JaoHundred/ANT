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

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if (sender is VisualElement ve)
            {
                await ve.ScaleTo(0.95, 100, easing: Easing.Linear);
                await ve.ScaleTo(1, 100, easing: Easing.Linear);
            }
        }
    }
}