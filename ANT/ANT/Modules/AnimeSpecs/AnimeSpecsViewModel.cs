using JikanDotNet;
using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;

namespace ANT.Modules
{
    public class AnimeSpecsViewModel : BaseViewModel
    {
        public AnimeSpecsViewModel(AnimeSubEntry anime)
        {
            AnimeContext = anime;
        }

        private AnimeSubEntry _animeContext;
        public AnimeSubEntry AnimeContext
        {
            get { return _animeContext; }
            set { SetProperty(ref _animeContext, value); }
        }

    }
}
