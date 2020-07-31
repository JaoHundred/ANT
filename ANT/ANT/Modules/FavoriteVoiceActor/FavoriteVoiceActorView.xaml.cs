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
    public partial class FavoriteVoiceActorView : ContentPage
    {
        public FavoriteVoiceActorView()
        {
            InitializeComponent();

            BindingContext = new FavoriteVoiceActorViewModel();
        }

        private async void ContentPage_Appearing(object sender, EventArgs e)
        {
            //Necessário para atualizar a lista de recentes toda vez que a página for aberta, já que o shell só carrega uma única vez
            //a ViewModel
            await(BindingContext as FavoriteVoiceActorViewModel)?.LoadAsync();
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

        private async void ActorsCollectionView_Scrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            await BottomSlide.ScrollHappenedAsync(e.VerticalDelta);
        }
    }
}