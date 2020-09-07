using Android.Widget;
using ANT.Core;
using ANT.Model;
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
    public partial class FavoriteAnimeView : ContentPage
    {
        public FavoriteAnimeView()
        {
            InitializeComponent();

#if DEBUG
            //NotificationTester.IsVisible = true;
#endif   
            BindingContext = new FavoriteAnimeViewModel();
        }

        private async void ContentPage_Appearing(object sender, EventArgs e)
        {
            //Necessário para atualizar a lista de recentes toda vez que a página for aberta, já que o shell só carrega uma única vez
            //a ViewModel
            MessagingCenter.Subscribe<FavoriteAnimeViewModel>(this, "CloseFilterView", (sender) =>
            {
                FilterSlideControl.IsOpen = false;
            });
            await (BindingContext as FavoriteAnimeViewModel)?.LoadAsync(null);
        }

        private void SearchLabelTapped(object sender, EventArgs e)
        {
            SearchControl.IsVisible = true;
            EntrySearchField.Focus();

            LabelTitle.IsVisible = false;
            UpdateLabel.IsVisible = false;
        }

        private void SearchFieldLostFocus(object sender, FocusEventArgs e)
        {
            SearchControl.IsVisible = false;
            LabelTitle.IsVisible = true;
            UpdateLabel.IsVisible = true;
        }

        private async void DeleteButton_AnimationOnIsVisible(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsVisible" && DeleteButton.IsVisible)
                await DeleteButton.Animate(new ShakeAnimation());
        }

        private void ContentPage_Disappearing(object sender, EventArgs e)
        {
            MessagingCenter.Unsubscribe<FavoriteAnimeViewModel>(this, "CloseFilterView");
            FilterSlideControl.IsOpen = false;
        }

        private void FilterTapped(object sender, EventArgs e)
        {
            FilterSlideControl.IsOpen = true;
        }

        private async void CollectionView_Scrolled(object sender, ItemsViewScrolledEventArgs e) =>
            await bottomSlideMenu.ScrollHappenedAsync(e.VerticalDelta);
    }
}