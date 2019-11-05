using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ANT.Core
{
    public static class AppPropertiesExtension
    {
        /// <summary>
        /// Cria ou atualiza um registro
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddOrUpdate(this IDictionary<string, object> dic, string key, object value)
        {
            bool hasKey = dic.ContainsKey(key);

            if (hasKey)
                dic[key] = value;
            else
                dic.Add(key, value);
        }
    }
}
