using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANT.UTIL
{
    public static class StringExtension
    {
        //já que o trim não funcionava, fiz meu próprio formatador de string que remove caracteres escolhidos
        public static string RemoveOcurrencesFromString(this string originalString, params char[] ocurrences)
        {

            var builder = new StringBuilder();

            for (int i = 0; i < originalString.Length; i++)
            {
                char c = originalString[i];

                if (!ocurrences.Contains(c))
                    builder.Append(c);
            }

            return builder.ToString();
        }
    }
}
