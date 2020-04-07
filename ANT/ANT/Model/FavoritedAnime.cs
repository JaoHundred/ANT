﻿using System;
using System.Collections.Generic;
using System.Text;
using JikanDotNet;
using MvvmHelpers;
using Newtonsoft.Json;

namespace ANT.Model
{
    public class FavoritedAnime : ObservableObject
    {
        //TODO: provável de precisar implementar propertychanged em todas as propriedades dessa classe, motivo: atualizar uma tela já aberta com dados novos
        //AnimeSpecsView
        public FavoritedAnime(Anime anime, IList<AnimeEpisode> episodes)
        {
            Anime = anime;
            Episodes = episodes;
        }

        public FavoritedAnime(AnimeSubEntry anime)
        {
            Anime = new Anime
            {
                MalId = anime.MalId,
                Title = anime.Title,
                ImageURL = anime.ImageURL,
                Genres = anime.Genres,
                LinkCanonical = anime.URL,
                Score = anime.Score,
                Aired = new TimePeriod() 
                {
                    From = anime.AiringStart,
                },
            };

            IsR18 = anime.R18;
        }

        public FavoritedAnime()
        { }

        private bool _isFavorited;
        public bool IsFavorited
        {
            get { return _isFavorited; }
            set { SetProperty(ref _isFavorited, value); }
        }

        public Anime Anime { get; set; }

        public bool? IsR18 { get; set; }

        public IList<AnimeEpisode> Episodes { get; set; }

        public IList<RelatedAnime> RelatedAnimes { get; set; }
    }
}
