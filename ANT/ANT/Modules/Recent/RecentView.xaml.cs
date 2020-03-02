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

        
    }
}