using ANT.Core;
using ANT.Model;
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
    public partial class RecentView : ContentPage
    {
        public RecentView()
        {
            InitializeComponent();
            BindingContext = new RecentViewModel();
        }

        private async void ContentPage_Appearing(object sender, EventArgs e)
        {
            //Necessário para atualizar a lista de recentes toda vez que a página for aberta, já que o shell só carrega uma única vez
            //a ViewModel
            await (BindingContext as RecentViewModel)?.LoadAsync(null);
        }

        //daqui para baixo foi necessário para lidar com 2 bugs do xamarin forms, o bug de não poder usar selectedItem e seus respectivos comandos junto com
        //tappedcommand,o selected item nunca era setado, o outro bug é mais uma falta, não há na data de hoje como passar binding via parâmetro do converter
        // e nem soluções com multibinding
        Frame _lastFrameSelected;
        readonly ResourceDictionary _resourceDic = ThemeManager.GetCurrentTheme();
        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if (_lastFrameSelected != null)
                _lastFrameSelected.BackgroundColor = (Color)_resourceDic["CatalogueAnimeFrameColor"];

            _lastFrameSelected = sender as Frame;

            _lastFrameSelected.BackgroundColor = (Color)_resourceDic["CatalogueAnimeSelectedColor"];

            var visu = _lastFrameSelected.BindingContext as RecentVisualized;
            RecentCollectionView.SelectedItem = visu;

            await Task.Delay(TimeSpan.FromSeconds(1));//necessário para não apagar de imediato o efeito visual de clique
            _lastFrameSelected.BackgroundColor = (Color)_resourceDic["CatalogueAnimeFrameColor"];
        }
    }
}