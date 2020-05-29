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
using NotificationManager = ANT.Core.NotificationManager;

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
                CloseSlideMenuTapped(null, null);
            });
            await (BindingContext as FavoriteAnimeViewModel)?.LoadAsync(null);
        }

        private void SearchLabelTapped(object sender, EventArgs e)
        {
            SearchControl.IsVisible = true;
            SearchIconLabel.IsVisible = false;
            EntrySearchField.Focus();

            LabelTitle.IsVisible = false;
        }

        private void SearchFieldLostFocus(object sender, FocusEventArgs e)
        {
            SearchControl.IsVisible = false;
            SearchIconLabel.IsVisible = true;
            LabelTitle.IsVisible = true;
        }

        private async void DeleteButton_AnimationOnIsVisible(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsVisible" && DeleteButton.IsVisible)
                await DeleteButton.Animate(new ShakeAnimation());
        }

        //TODO: O método logo abaixo é para testes, remover quando estiver tudo funcionando
        private void gerador_de_notificacao(object sender, EventArgs e)
        {
            var animes = (BindingContext as FavoriteAnimeViewModel).GroupedFavoriteByWeekList.SelectMany(p => p.Select(q => q));

            foreach (var item in animes)
            {
                NotificationManager.CreateNotificationAsync(item, Consts.NotificationChannelTodayAnime, DateTime.Now.AddMinutes(3));
            }
        }

        private async void ContentPage_Disappearing(object sender, EventArgs e)
        {
            MessagingCenter.Unsubscribe<FavoriteAnimeViewModel>(this, "CloseFilterView");
            CloseSlideMenuTapped(null, null);

            await Task.Run(async () => 
            {
                var animes = (BindingContext as FavoriteAnimeViewModel)._originalCollection.SelectMany(p => p.Select(q => q));
                var favoriteCollection = App.liteDB.GetCollection<FavoritedAnime>();

                foreach (var anime in animes)
                {
                    favoriteCollection.Update(anime.Anime.MalId, anime);

                    if (anime.NextStreamDate == null)
                        continue;

                    if(anime.HasNotificationReady && !anime.CanGenerateNotifications)
                        await NotificationManager.CancelNotificationAsync(anime);
                    else if(!anime.HasNotificationReady && anime.CanGenerateNotifications)
                        await NotificationManager.CreateNotificationAsync(anime, Consts.NotificationChannelTodayAnime);
                }
            });
        }

        private async void CloseSlideMenuTapped(object sender, EventArgs e)
        {
            await SlideMenu.TranslateTo(0, _page.Bounds.Bottom, easing: Easing.Linear);
        }

        private async void FilterTapped(object sender, EventArgs e)
        {
            await SlideMenu.TranslateTo(0, _page.Bounds.Top, easing: Easing.Linear);
        }
    }
}