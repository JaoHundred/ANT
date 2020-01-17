using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Pages;

namespace ANT.Modules
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProgressPopupView : PopupPage
    {
        public ProgressPopupView()
        {
            InitializeComponent();
        }

        protected override bool OnBackButtonPressed()
        {
            //true faz o popup não fechar com o backbutton, antigamente isso era lidado no mainactivity sobrescrevendo o evento de back
            //mas como tem como sobreescrever aqui, não vai precisar ser tratado as PopupPage por lá
            //se por padrão eu quero que o popup seja fechado se eu apertar back, não precisa sobreescrever esse método
            return true;
        }

        protected override bool OnBackgroundClicked()
        {
            return false;//false faz o popup não fechar ao apertar fora do popup
        }

        private async void ProgressBarControl_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //TODO: descobrir um meio de chamar o progressto via ViewModel
            if (e.PropertyName == "Progress")
                await ProgressBarControl.ProgressTo(ProgressBarControl.Progress, 500, Easing.SinInOut);
        }
    }
}