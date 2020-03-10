using JikanDotNet;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace ANT.Model
{
    public class FavoritedAnimeCharacter : ObservableObject
    {
        public FavoritedAnimeCharacter(Character character, IList<Picture> characterPictures)
        {
            Character = character;
            CharacterPictures = characterPictures;
        }

        private bool _isFavorited;
        public bool IsFavorited
        {
            get { return _isFavorited; }
            set { SetProperty(ref _isFavorited, value); }
        }

        public Character Character { get; set; }
        public IList<Picture> CharacterPictures { get; set; }

    }
}
