﻿using ANT.Interfaces;
using ANT.Model;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using ANT.UTIL;
using System.Linq;
using Android.OS;
using System.Reflection;

namespace ANT.Core
{
    public class LiteDBHelper
    {
        public const string LiteDataName = "data";
        public const string LiteErrordbLog = "errorLog";

        public static string GetLiteDBPath(string databaseName)
        {
            return System.IO.Path.Combine(DependencyService.Get<IGetFolder>().GetApplicationDocumentsFolder(), databaseName);
        }

        public static void StartLiteDB()
        {
            BsonMapper bsonMapper = BsonMapper.Global;
            bsonMapper.Entity<TodayAnimes>().Id(todayAnimes => todayAnimes.Id);
            bsonMapper.Entity<RecommendationAnimes>().Id(recommendation => recommendation.Id);

            string completePath = $"Filename={GetLiteDBPath(LiteDataName)}";
            App.liteDB = new LiteDatabase(completePath, bsonMapper);

            App.liteDB.Checkpoint();
        }

        /// <summary>
        /// Método para iniciar o liteErrorLogDB
        /// </summary>
        public static void StartErrorLogLiteDB()
        {
            BsonMapper bsonMapper = BsonMapper.Global;
            bsonMapper.Entity<ErrorLog>().Id(errorLog => errorLog.Id);

            App.liteErrorLogDB = new LiteDatabase($"Filename={GetLiteDBPath(LiteErrordbLog)}", bsonMapper);

            App.liteErrorLogDB.Checkpoint();
        }

        public static void MigrateLiteDB()
        {
            if (App.liteDB.UserVersion == 0)
            {
                try
                {
                    App.liteDB.Dispose();
                    App.liteDB = null;

                    string completePath = $"Filename={GetLiteDBPath(LiteDataName)}";

                    using (var db = new LiteDatabase(completePath))
                    {

                        var favorites = db.GetCollection("FavoritedAnime").FindAll();

                        foreach (var doc in favorites)
                        {
                            string rawValue = doc["Anime"]["Episodes"].RawValue as string;

                            doc["Anime"]["Episodes"] = ConvertStringToNullableInt(rawValue);
                            db.GetCollection("FavoritedAnime").Update(doc);
                        }

                        var recents = db.GetCollection("RecentVisualized").FindAll();

                        foreach (var doc in recents)
                        {
                            string rawValue = doc["FavoritedAnime"]["Anime"]["Episodes"].RawValue as string;

                            doc["FavoritedAnime"]["Anime"]["Episodes"] = ConvertStringToNullableInt(rawValue);

                            db.GetCollection("RecentVisualized").Update(doc);
                        }

                        db.UserVersion = 1;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine(ex.Source);
                    Console.WriteLine(ex.InnerException);
                    Console.WriteLine(ex.TargetSite.Name);

                    ex.SaveExceptionData();
                }
                finally
                {
                    if (App.liteDB == null)
                        StartLiteDB();
                }
            }
        }

        private static int? ConvertStringToNullableInt(string str)
        {
            int result;

            if (int.TryParse(str, out result))
                return result;

            return null;
        }
    }
}
