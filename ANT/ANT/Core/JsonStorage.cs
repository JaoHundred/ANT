using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace ANT.Core
{
    public static class JsonStorage
    {
        public  static Task SaveDataAsync<T>(T obj, string directoryPath, string fileName)
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

        public async static Task<T> ReadDataAsync<T>(string directoryPath, string fileName) 
        {
            T sett = default;

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            await Task.Run(() =>
            {
                using (FileStream stream = new FileStream(Path.Combine(directoryPath, fileName), FileMode.OpenOrCreate, FileAccess.Read))
                using (StreamReader reader = new StreamReader(stream))
                using (JsonReader jr = new JsonTextReader(reader))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    sett = serializer.Deserialize<T>(jr);
                }
            });

            return sett;
        }
    }
}
