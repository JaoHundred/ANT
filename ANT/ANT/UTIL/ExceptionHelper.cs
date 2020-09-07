using ANT.Interfaces;
using ANT.Model;
using JikanDotNet.Exceptions;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace ANT.UTIL
{
    public static class ExceptionHelper
    {
        /// <summary>
        /// Salva resultados da exceção em log liteDB
        /// </summary>
        /// <param name="exception"></param>
        public static void SaveExceptionData(this System.Exception exception)
        {
            var error = new ErrorLog()
            {
                Message = exception.Message,
                Source = exception.Source,
                StackTrace = exception.StackTrace,
                ExceptionDate = DateTime.Now,
                ExceptionTypeName = exception.GetType().ToString(),
            };

            switch (exception)
            {
                case JikanRequestException ex:

                    string responseCode = $"{ex.ResponseCode} { (int)ex.ResponseCode}";
#if DEBUG
                    Console.WriteLine($"Problema encontrado em :{responseCode}");
#endif
                    DependencyService.Get<IToast>().MakeToastMessageLong(responseCode);

                    error.AdditionalInfo = responseCode;

                    break;

                case System.Exception exc:
#if DEBUG
                    Console.WriteLine($"Problema encontrado em :{exc.Message}");
#endif
                    DependencyService.Get<IToast>().MakeToastMessageLong(Lang.Lang.Error);

                    break;
            }

            if (App.liteErrorLogDB == null)
                App.StartErrorLogLiteDB();

                var errorLogCollection = App.liteErrorLogDB.GetCollection<ErrorLog>();
                errorLogCollection.Insert(error);

#if DEBUG
                foreach (var exc in errorLogCollection.FindAll())
                    Console.WriteLine($"exceção encontrada em: {exc.Id} {exc.ExceptionTypeName} {exc.Message}");
#endif
        }
    }
}
