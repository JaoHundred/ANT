using ANT.Interfaces;
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
        public static void StartLiteDB()
        {
            string newLocation = DependencyService.Get<IGetFolder>().GetApplicationDocumentsFolder();

            string fullPath = System.IO.Path.Combine(newLocation, "data");

            BsonMapper bsonMapper = BsonMapper.Global;
            bsonMapper.Entity<TodayAnimes>().Id(todayAnimes => todayAnimes.Id);
            bsonMapper.Entity<RecommendationAnimes>().Id(recommendation => recommendation.Id);

            string completePath = $"Filename={fullPath}";
            App.liteDB = new LiteDatabase(completePath, bsonMapper);

            App.liteDB.Checkpoint();
        }

        /// <summary>
        /// Método para iniciar o liteErrorLogDB
        /// </summary>
        public static void StartErrorLogLiteDB()
        {
            string newLocation = DependencyService.Get<IGetFolder>().GetApplicationDocumentsFolder();

            string fullPath = System.IO.Path.Combine(newLocation, "errorLog");

            BsonMapper bsonMapper = BsonMapper.Global;
            bsonMapper.Entity<ErrorLog>().Id(errorLog => errorLog.Id);

            App.liteErrorLogDB = new LiteDatabase($"Filename={fullPath}", bsonMapper);

            App.liteErrorLogDB.Checkpoint();
        }

        public static void MigrateLiteDB()
        {
            try
            {
                App.liteDB.Dispose();
                App.liteDB = null;

                string newLocation = DependencyService.Get<IGetFolder>().GetApplicationDocumentsFolder();
                string fullPath = System.IO.Path.Combine(newLocation, "data");
                string completePath = $"Filename={fullPath}";

                //TODO: migrate não está funcionando, tentar pesquisar o que é
                //a correção momentânea mas que não ajuda a resolver o problema de mapeamento do Episodes dentro de anime
                //é reinstalar o app(deletando todos os dados de usuário no processo)
                //https://github.com/Ervie/jikan.net/blob/master/Changelog.md
                //para simular a migração, retornar para uma versão anterior a mudança do Episodes, registrar dados, atualizar novamente
                //para a att mais recente e depois abrir o app o código abaixo será executado

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

                    db.UserVersion++;
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

        private static int? ConvertStringToNullableInt(string str)
        {
            int result;

            if (int.TryParse(str, out result))
                return result;

            return null;
        }
    }
}
