using JikanDotNet;
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
    public partial class AnimeSpecsView : ContentPage
    {
        public AnimeSpecsView()
        {
            InitializeComponent();
        }

        public AnimeSpecsView(AnimeSubEntry anime)
        {
            InitializeComponent();
            BindingContext = new AnimeSpecsViewModel(anime);
        }
    }
}