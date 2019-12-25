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
            BindingContext = new CatalogueViewModel();//TODO o culpado por fazer os dois construtores serem chamados via NavigationManager, o shell
            //não usa comandos, logo precisa de um ponto de partida pra conectar a VM com a view, toda vez que a view dessa tela for criada ela vai 
            //rebindar para a vm sem parâmetros, logo vai chamar os 2 construtores(o com e o sem parâmetros), descobrir o que pode ser feito
        }

        private async void FavoriteButton_AnimationOnIsVisible(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsVisible" && FavoriteButton.IsVisible)
                await FavoriteButton.Animate(new ShakeAnimation());
        }
    }
}