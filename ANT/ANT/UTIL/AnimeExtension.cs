using JikanDotNet;
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
        public static Task<bool> HasAllSpecifiedGenresAsync(this FavoritedAnime favoriteAnime, params GenreSearch[] genres)
        {
            return Task.Run(() =>
            {
                List<bool> hasAllGenres = new List<bool>();

                foreach (var genre in genres)
                {
                    bool result = favoriteAnime.Anime.Genres.Any(p => p.Name.ToLowerInvariant().RemoveOcurrencesFromString(new[] { '-', ' ' })
                     == genre.ToString().ToLowerInvariant().RemoveOcurrencesFromString(new[] { '-', ' ' }));

                    hasAllGenres.Add(result);
                }

                return !hasAllGenres.Any(p => p == false);
            });
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

                if (App.liteDB.GetCollection<FavoritedAnime>().Exists(p => p.Anime.MalId == anime.MalId))
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

        public static IList<GenreData> FillGenres(bool showNSFWGenres = true)
        {
            var genres = new List<GenreData>()
            {
                new GenreData(GenreSearch.Action),
                new GenreData(GenreSearch.Adventure),
                new GenreData(GenreSearch.Cars),
                new GenreData(GenreSearch.Comedy),
                new GenreData(GenreSearch.Dementia),
                new GenreData(GenreSearch.Demons),
                new GenreData(GenreSearch.Drama),
                new GenreData(GenreSearch.Ecchi),
                new GenreData(GenreSearch.Fantasy),
                new GenreData(GenreSearch.Game),
                new GenreData(GenreSearch.Harem),
                new GenreData(GenreSearch.Hentai),
                new GenreData(GenreSearch.Historical),
                new GenreData(GenreSearch.Horror),
                new GenreData(GenreSearch.Josei),
                new GenreData(GenreSearch.Kids),
                new GenreData(GenreSearch.Magic),
                new GenreData(GenreSearch.MartialArts),
                new GenreData(GenreSearch.Mecha),
                new GenreData(GenreSearch.Military),
                new GenreData(GenreSearch.Music),
                new GenreData(GenreSearch.Mystery),
                new GenreData(GenreSearch.Parody),
                new GenreData(GenreSearch.Police),
                new GenreData(GenreSearch.Psychological),
                new GenreData(GenreSearch.Romance),
                new GenreData(GenreSearch.Samurai),
                new GenreData(GenreSearch.School),
                new GenreData(GenreSearch.SciFi),
                new GenreData(GenreSearch.Seinen),
                new GenreData(GenreSearch.Shoujo),
                new GenreData(GenreSearch.ShoujoAi),
                new GenreData(GenreSearch.Shounen),
                new GenreData(GenreSearch.ShounenAi),
                new GenreData(GenreSearch.SliceOfLife),
                new GenreData(GenreSearch.Space),
                new GenreData(GenreSearch.Sports),
                new GenreData(GenreSearch.Supernatural),
                new GenreData(GenreSearch.SuperPower),
                new GenreData(GenreSearch.Thriller),
                new GenreData(GenreSearch.Vampire),
                new GenreData(GenreSearch.Yaoi),
                new GenreData(GenreSearch.Yuri),
            };

            if (!showNSFWGenres)
            {
                var toRemove = new List<GenreData>
                {
                    new GenreData(GenreSearch.Ecchi),
                    new GenreData(GenreSearch.Harem),
                    new GenreData(GenreSearch.Hentai),
                    new GenreData(GenreSearch.Yaoi),
                    new GenreData(GenreSearch.Yuri),
                };

                genres = genres.Except(toRemove, new EqualityGenreData()).ToList();
            }

            return genres;
        }

        public static IList<OrderData> FillOrderData()
        {
            return new List<OrderData>
            {
               new OrderData( AnimeSearchSortable.Score, true),
               new OrderData( AnimeSearchSortable.Title),
               new OrderData( AnimeSearchSortable.StartDate),
               new OrderData( AnimeSearchSortable.EndDate),
            };
        }

        public static IList<DayOfWeek> FillDayOfWeek()
        {
            return new List<DayOfWeek>
            {
                    DayOfWeek.Sunday,
                    DayOfWeek.Monday,
                    DayOfWeek.Tuesday,
                    DayOfWeek.Wednesday,
                    DayOfWeek.Thursday,
                    DayOfWeek.Friday,
                    DayOfWeek.Saturday,
            };
        }

        public static IList<SortDirectionData> FillSortDirectionData()
        {
            return new List<SortDirectionData>
            {
                new SortDirectionData(SortDirection.Descending, true),
                new SortDirectionData(SortDirection.Ascending),
            };
        }
    }
}
