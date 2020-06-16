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

        public static Task<bool> HasAnyDayOfWeekAsync(this FavoritedAnime favoriteAnime, params DayOfWeekFilterDate[] dayOfWeekFilterDates)
        {
            return Task.Run(() =>
            {
                List<bool> hasAnyDayOfWeek = new List<bool>();

                if (dayOfWeekFilterDates.Length == 0)
                    return true;

                foreach (var dayOfWeek in dayOfWeekFilterDates)
                {
                    if (!favoriteAnime.NextStreamDate.HasValue)
                    {
                        if (dayOfWeekFilterDates.Any(p => p.TodayDayOfWeek == TodayDayOfWeek.Unknown))
                            hasAnyDayOfWeek.Add(true);

                        continue;
                    }

                    if (favoriteAnime.NextStreamDate.HasValue && !favoriteAnime.Anime.Airing)
                    {
                        if (dayOfWeekFilterDates.Any(p => p.TodayDayOfWeek == TodayDayOfWeek.FinishedAiring))
                            hasAnyDayOfWeek.Add(true);
                        continue;
                    }

                    bool hasWeekDay = favoriteAnime.NextStreamDate.Value.DayOfWeek.ToString() == dayOfWeek.TodayDayOfWeek.ToString();
                    bool hasToday = false;

                    if (dayOfWeek.TodayDayOfWeek == TodayDayOfWeek.Today)
                        hasToday = favoriteAnime.NextStreamDate.Value.DayOfWeek == DateTime.Today.DayOfWeek;

                    if (hasWeekDay || hasToday)
                        hasAnyDayOfWeek.Add(hasWeekDay | hasToday);
                }

                return hasAnyDayOfWeek.Any(p => p);
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

        /// <summary>
        /// Retorna a data do próximo dia de semana que o anime irá passar, se não houver data, retorna null
        /// </summary>
        /// <param name="favoritedAnime"></param>
        /// <returns></returns>
        public static Task<DateTime?> NextEpisodeDateAsync(this FavoritedAnime favoritedAnime)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrEmpty(favoritedAnime.Anime.Broadcast))
                    return null;

                var daysOfWeek = Enum.GetNames(typeof(DayOfWeek)).Select(p => new string(p.Append('s').ToArray()).ToString().ToLowerInvariant()).ToList();
                DayOfWeek? nextEpisodeDay = null;

                string[] broadCastVector = favoritedAnime.Anime.Broadcast.Split(' ');

                foreach (var day in daysOfWeek)
                {
                    string broadCastDay = broadCastVector.FirstOrDefault(p => p.ToLowerInvariant() == day);

                    if (string.IsNullOrWhiteSpace(broadCastDay))
                        continue;

                    broadCastDay = broadCastDay.ToLowerInvariant();

                    switch (broadCastDay)
                    {
                        case "sundays":
                            nextEpisodeDay = DayOfWeek.Sunday;
                            break;
                        case "mondays":
                            nextEpisodeDay = DayOfWeek.Monday;
                            break;
                        case "tuesdays":
                            nextEpisodeDay = DayOfWeek.Tuesday;
                            break;
                        case "wednesdays":
                            nextEpisodeDay = DayOfWeek.Wednesday;
                            break;
                        case "thursdays":
                            nextEpisodeDay = DayOfWeek.Thursday;
                            break;
                        case "fridays":
                            nextEpisodeDay = DayOfWeek.Friday;
                            break;
                        case "saturdays":
                            nextEpisodeDay = DayOfWeek.Saturday;
                            break;
                    }
                }

                if (nextEpisodeDay == null)
                    return null;

                int daysToSchedule = 0;

                if (nextEpisodeDay > DateTime.Today.DayOfWeek)
                    daysToSchedule = (int)nextEpisodeDay - (int)DateTime.Today.DayOfWeek;

                else if (nextEpisodeDay <= DateTime.Today.DayOfWeek)
                    daysToSchedule = ((int)nextEpisodeDay + 7) - (int)DateTime.Today.DayOfWeek;
                // TODO: ficar de olho nessa condição, suspeito que se acontecer do dia de atualização coincidir com o mesmo dia que passa o anime, nenhuma notificação será gerada para a próxima semana

                DateTime? nextEpisodeDate = DateTime.Today.AddDays(daysToSchedule).AddHours(12);

                return nextEpisodeDate;
            });
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

        public static IList<DayOfWeekFilterDate> FillTodayDayOfWeek()
        {
            return new List<DayOfWeekFilterDate>
            {
                 new DayOfWeekFilterDate(TodayDayOfWeek.Today),
                 new DayOfWeekFilterDate(TodayDayOfWeek.Unknown),
                 new DayOfWeekFilterDate(TodayDayOfWeek.FinishedAiring),
                 new DayOfWeekFilterDate(TodayDayOfWeek.Sunday),
                 new DayOfWeekFilterDate(TodayDayOfWeek.Monday),
                 new DayOfWeekFilterDate(TodayDayOfWeek.Tuesday),
                 new DayOfWeekFilterDate(TodayDayOfWeek.Wednesday),
                 new DayOfWeekFilterDate(TodayDayOfWeek.Thursday),
                 new DayOfWeekFilterDate(TodayDayOfWeek.Friday),
                 new DayOfWeekFilterDate(TodayDayOfWeek.Saturday),
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
