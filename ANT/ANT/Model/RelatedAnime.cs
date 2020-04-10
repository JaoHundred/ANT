using JikanDotNet;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace ANT.Model
{
    public class RelatedAnime : ObservableObject
    {
        public RelatedAnime()
        {

        }

        public RelatedAnime(MALSubItem anime)
        {
            Anime = new Anime
            {
                MalId = anime.MalId,
                Title = anime.Name,
            };
        }

        public Anime Anime { get; set; }

        private string _imageURL;
        public string ImageURL
        {
            get { return _imageURL; }
            set
            {
                ImgIsLoading = true;
                SetProperty(ref _imageURL, value);
                ImgIsLoading = false;
            }
        }

        private bool _imgIsLoading = true;
        [Newtonsoft.Json.JsonIgnore]
        public bool ImgIsLoading
        {
            get { return _imgIsLoading; }
            set { SetProperty(ref _imgIsLoading, value); }
        }
        public string GroupName { get; set; }
    }
}
