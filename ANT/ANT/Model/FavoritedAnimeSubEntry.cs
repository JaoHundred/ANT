using System;
using System.Collections.Generic;
using System.Text;
using JikanDotNet;
using MvvmHelpers;

namespace ANT.Model
{
    public class FavoritedAnimeSubEntry : ObservableObject
    {
        public FavoritedAnimeSubEntry(AnimeSubEntry animeSubEntry)
        {
            FavoritedAnime = animeSubEntry;
        }

        public AnimeSubEntry FavoritedAnime { get; set; }

        private bool _isFavorited;
        public bool IsFavorited
        {
            get { return _isFavorited; }
            set { SetProperty(ref _isFavorited, value); }
        }
    }
}
