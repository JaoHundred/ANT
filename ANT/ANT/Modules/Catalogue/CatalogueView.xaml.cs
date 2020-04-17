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
               CloseSlideMenuTapped(null, null);
           });
        }

        private async void FavoriteButton_AnimationOnIsVisible(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsVisible" && FavoriteButton.IsVisible)
                await FavoriteButton.Animate(new ShakeAnimation());
        }

        private void SearchLabelTapped(object sender, EventArgs e)
        {
            SearchControl.IsVisible = true;
            SearchIconLabel.IsVisible = false;
            FilterControl.IsVisible = false;
            EntrySearchField.Focus();

            LabelTitle.IsVisible = false;
        }

        private void SearchFieldLostFocus(object sender, FocusEventArgs e)
        {
            SearchControl.IsVisible = false;
            SearchIconLabel.IsVisible = true;
            FilterControl.IsVisible = true;
            LabelTitle.IsVisible = true;
        }

        private async void CloseSlideMenuTapped(object sender, EventArgs e)
        {
            await SlideMenu.TranslateTo(0, _Page.Bounds.Bottom, easing: Easing.Linear);
        }

        private async void FilterTapped(object sender, EventArgs e)
        {
            await SlideMenu.TranslateTo(0, _Page.Bounds.Top, easing: Easing.Linear);
        }
    }
}