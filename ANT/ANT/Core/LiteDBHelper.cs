using ANT.Interfaces;
using ANT.Model;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using ANT.UTIL;

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

        public static void MigrateDatabase()
        {
            string newLocation = DependencyService.Get<IGetFolder>().GetApplicationDocumentsFolder();
            string fullPath = System.IO.Path.Combine(newLocation, "data");
            string completePath = $"Filename={fullPath}";

            //TODO: migrate não está funcionando, tentar pesquisar o que é
            //a correção momentânea mas que não ajuda a resolver o problema de mapeamento do Episodes dentro de anime
            //é reinstalar o app(deletando todos os dados de usuário no processo)
            //https://github.com/Ervie/jikan.net/blob/master/Changelog.md
            //para simular a migração, retornar para uma versão anterior a mudança do Episodes, registrar dados, atualizar novamente
            //para a att mais recente e depois abrir o app o código abaixo será executado

            //using (var db = new LiteDatabase(completePath))
            //{
            //    if (db.UserVersion == 0)
            //    {
            //        Exception exp = null;
            //        try
            //        {
            //            foreach (var doc in db.GetCollection("FavoritedAnime").FindAll())
            //            {
            //                doc["Anime"]["Episodes"] = ConvertStringToNullableInt(doc["Anime"]["Episodes"].AsString);
            //                db.GetCollection("FavoritedAnime").Update(doc);
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            exp = ex;
            //            ex.SaveExceptionData();
            //        }

            //        try
            //        {
            //            foreach (var doc in db.GetCollection("RecentVisualized").FindAll())
            //            {
            //                doc["RecentVisualized"]["FavoritedAnime"]["Anime"]["Episodes"] =
            //                    ConvertStringToNullableInt(doc["RecentVisualized"]["FavoritedAnime"]["Anime"]["Episodes"].AsString);

            //                db.GetCollection("RecentVisualized").Update(doc);
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            exp = ex;
            //            ex.SaveExceptionData();
            //        }

            //        if (exp == null)
            //            db.UserVersion = 1;
            //    }
            //}
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
