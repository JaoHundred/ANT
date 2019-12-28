using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ANT.Core;

namespace ANT.Lang
{
    public static class TranslateBroadcastFormat
    {
        public static string TranslateBroadcast(string broadcast)
        {
            var split = broadcast.Split(' ');
            string dayOfWeek = split[0];

            //TODO: a tradução não se mantém se o usuário trocar de idioma com o app aberto, 
            //o código abaixo vai pegar a última tradução do sistema em que o aplicativo foi aberto,
            //ex: app foi aberto com o sistema em inglês mas foi trocado com o app aberto para português, o código abaixo vai continuar retornarndo inglês

            string formatedBroadcast = string.Empty;
            string formatedPhrase = string.Empty;

            if (split.Contains(Consts.Unknown))
                return Lang.UnknownDate;
            else
                formatedPhrase = "{0} " + $"{Lang.At} {split[2]} {split[3]}";

            switch (dayOfWeek)
            {
                case "Fridays":
                    formatedBroadcast = string.Format(formatedPhrase, Lang.Fridays);
                    break;
                case "Mondays":
                    formatedBroadcast = string.Format(formatedPhrase, Lang.Mondays);
                    break;
                case "Saturdays":
                    formatedBroadcast = string.Format(formatedPhrase, Lang.Saturdays);
                    break;
                case "Sundays":
                    formatedBroadcast = string.Format(formatedPhrase, Lang.Sundays);
                    break;
                case "Thursdays":
                    formatedBroadcast = string.Format(formatedPhrase, Lang.Thursdays);
                    break;
                case "Tuesdays":
                    formatedBroadcast = string.Format(formatedPhrase, Lang.Tuesdays);
                    break;
                case "Wednesdays":
                    formatedBroadcast = string.Format(formatedPhrase, Lang.Wednesdays);
                    break;
            }

            return formatedBroadcast;
        }
    }
}
