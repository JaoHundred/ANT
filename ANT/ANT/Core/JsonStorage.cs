using ANT.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ANT.Core
{
   public static class JsonStorage
    {
        //TODO: testar implementações e ver se funciona para salvar os settings, abandonar o properties se funcionar
        public async static Task SaveDataAsync<T>(T obj, string directoryPath, string fileName)
        {
            string path = Path.Combine(directoryPath, fileName);

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            await Task.Run(() =>
            {
                using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                using (StreamWriter writer = new StreamWriter(stream))
                using (JsonWriter jw = new JsonTextWriter(writer))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(jw, obj);
                }
            });
        }

        /// <summary>
        /// leitura de dados salvos na pasta dentro da aplicação
        /// </summary>
        /// <typeparam name="T">qualquer tipo de objeto</typeparam>
        /// <returns></returns>
        public async static Task<IList<T>> ReadDataAsync<T>(string directoryPath, string fileName)
        {
            IList<T> objects = null;

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            await Task.Run(() =>
            {
                using (FileStream stream = new FileStream(Path.Combine(directoryPath, fileName), FileMode.OpenOrCreate, FileAccess.Read))
                using (StreamReader reader = new StreamReader(stream))
                using (JsonReader jr = new JsonTextReader(reader))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    objects = serializer.Deserialize<IList<T>>(jr);
                }
            });

            return objects ?? new List<T>();
        }


        public  static Task SaveSettingsAsync(SettingsPreferences obj, string directoryPath, string fileName)
        {
            return Task.Run(() =>
            {
                string path = Path.Combine(directoryPath, fileName);

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                using (StreamWriter writer = new StreamWriter(stream))
                using (JsonWriter jw = new JsonTextWriter(writer))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(jw, obj);
                }
            });
        }

        /// <summary>
        /// leitura de dados salvos na pasta dentro da aplicação para Settings
        /// </summary>
        /// <returns></returns>
        public async static Task<SettingsPreferences> ReadSettingsAsync(string directoryPath, string fileName) 
        {
            SettingsPreferences sett = null;

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            await Task.Run(() =>
            {
                using (FileStream stream = new FileStream(Path.Combine(directoryPath, fileName), FileMode.OpenOrCreate, FileAccess.Read))
                using (StreamReader reader = new StreamReader(stream))
                using (JsonReader jr = new JsonTextReader(reader))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    sett = serializer.Deserialize<SettingsPreferences>(jr);
                }
            });

            return sett;
        }
    }
}
