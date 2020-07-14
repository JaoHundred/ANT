using JikanDotNet;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace ANT.Model
{
    public class FavoritedVoiceActor : ObservableObject
    {
        public FavoritedVoiceActor()
        {}

        public FavoritedVoiceActor(Person voiceActor, IList<Picture> voiceActorPictures)
        {
            VoiceActor = voiceActor;
            VoiceActorPictures = voiceActorPictures;
        }

        public Person VoiceActor { get; set; }

        private bool _isFavorited;
        public bool IsFavorited
        {
            get { return _isFavorited; }
            set { SetProperty(ref _isFavorited, value); }
        }

        public IList<Picture> VoiceActorPictures { get; set; }
    }
}
