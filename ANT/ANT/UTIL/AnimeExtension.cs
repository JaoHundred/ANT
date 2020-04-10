﻿using JikanDotNet;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using ANT.Model;
using System.Threading;
using JikanDotNet.Exceptions;

namespace ANT.UTIL
{
    public static class AnimeExtension
    {
        public static bool HasAllSpecifiedGenres(this AnimeSubEntry anime, params GenreSearch[] genres)
        {
            bool hasAllGenres = false;
            foreach (var genre in genres)
            {
                string genreId = GetDescription(genre);
                hasAllGenres = anime.Genres.Any(p => p.MalId == int.Parse(genreId));

                if (!hasAllGenres)
                    break;
            }
            return hasAllGenres;
        }

        public static string GetDescription(Enum value)
        {
            return
                value
                    .GetType()
                    .GetMember(value.ToString())
                    .FirstOrDefault()
                    ?.GetCustomAttribute<DescriptionAttribute>()
                    ?.Description
                ?? value.ToString();
        }

        /// <summary>
        /// Carrega todos os N episódios de um anime e suas N páginas
        /// implementa task.delay internamente para impedir de afogar o serviço jikan
        /// </summary>
        /// <param name="anime"></param>
        /// <returns></returns>
        public static async Task<IList<AnimeEpisode>> GetAllEpisodesAsync(this Anime anime, CancellationTokenSource cancellationToken = null)
        {
            await App.DelayRequest(2);
            AnimeEpisodes episodes = await App.Jikan.GetAnimeEpisodes(anime.MalId);

            var episodeList = new List<AnimeEpisode>();

            for (int j = 0; j < episodes.EpisodesLastPage; j++)
            {
                await App.DelayRequest(4);

                if (cancellationToken != null && cancellationToken.IsCancellationRequested)
                    cancellationToken.Token.ThrowIfCancellationRequested();

                var epiList = await App.Jikan.GetAnimeEpisodes(anime.MalId, j + 1);

                episodeList.AddRange(epiList.EpisodeCollection);
            }

            return episodeList;
        }

        public static IList<FavoritedAnime> ConvertAnimesToFavorited(this ICollection<AnimeSubEntry> animeSubEntries)
        {
            var animeSubs = animeSubEntries.ToList();
            var favoritedAnimeSubsEntries = new List<FavoritedAnime>();

            for (int i = 0; i < animeSubs.Count; i++)
            {
                var anime = animeSubs[i];
                var animeSub = new FavoritedAnime
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
                    },

                IsR18 = anime.R18,
            };

                if (App.FavoritedAnimes.Exists(p => p.Anime.MalId == anime.MalId))
                    animeSub.IsFavorited = true;

                favoritedAnimeSubsEntries.Add(animeSub);
            }

            return favoritedAnimeSubsEntries;
        }

        public static IList<AnimeSubEntry> ConvertAnimeSearchEntryToAnimeSubEntry(this ICollection<AnimeSearchEntry> animeSearch)
        {
            var searchs = animeSearch.ToList();
            var subs = new List<AnimeSubEntry>();

            foreach (var search in searchs)
            {
                var sub = new AnimeSubEntry
                {
                    MalId = search.MalId,
                    ImageURL = search.ImageURL,
                    Title = search.Title,
                    AiringStart = search.StartDate,
                    Score = search.Score,
                };

                subs.Add(sub);
            }

            return subs;
        }

        public static IList<ANT.Model.RelatedAnime> ConvertMalSubItemToRelatedAnime(this ICollection<MALSubItem> subItems, string groupName)
        {
            var relatedAnimes = new List<ANT.Model.RelatedAnime>();

            foreach (var item in subItems)
            {
                var related = new ANT.Model.RelatedAnime(item);
                related.GroupName = groupName;
                relatedAnimes.Add(related);
            }

            return relatedAnimes;
        }
    }
}
