using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamanimation;
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

            MessagingCenter.Subscribe<CatalogueViewModel>(this, "CloseFilterView", (sender) =>
           {
               FilderSlide.IsOpen = false;
           });
        }

        private async void FavoriteButton_AnimationOnIsVisible(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsVisible" && FavoriteButton.IsVisible)
                await FavoriteButton.Animate(new ShakeAnimation());
        }

        private void SearchLabelTapped(object sender, EventArgs e)
        {
            EntrySearchField.Focus();
        }

        private void FilterTapped(object sender, EventArgs e)
        {
            FilderSlide.IsOpen = true;
        }

        private async void CatalogueCollection_Scrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            await bottomSlideMenu.ScrollHappenedAsync(e.VerticalDelta);
        }
    }
}