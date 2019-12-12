﻿using JikanDotNet;
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
    public partial class AnimeGenrePopupView : Rg.Plugins.Popup.Pages.PopupPage
    {
        public AnimeGenrePopupView(IList<MALSubItem> animeGenres)
        {
            InitializeComponent();
            BindingContext = new AnimeGenrePopupViewModel(animeGenres);
        }
    }
}