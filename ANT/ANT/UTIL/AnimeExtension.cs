using JikanDotNet;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.ComponentModel;
using System.Reflection;

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
                hasAllGenres = anime.Genres.Any(p => p.MalId == int.Parse( genreId));

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
    }
}
