using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using System.Linq;
using System.Threading.Tasks;

namespace ANT.Helpers
{
    /// <summary>
    /// Classe usada para lidar com problemas comuns da navegação como duplo clique indesejado por exemplo
    /// </summary>
    public static class NavigateHelper
    {
        /// <summary>
        /// Use esse método para previnir de executar duplo clique nas navegações entre views(impedindo de abrir mais de uma instância)
        /// </summary>
        /// <typeparam name="T">Tipo de View que será checada no navigation stack</typeparam>
        /// <param name="executeBeforeReturn">vai executar sempre antes do CanNavigate retornar</param>
        /// <returns></returns>
        public static Task<bool> CanShellNavigateAsync<T>(Action executeBeforeReturn = null)
        {
            return Task.Run(() =>
            {
                var lastPageInStack = Shell.Current.Navigation.NavigationStack.LastOrDefault();
                return CanNavigate<T>(executeBeforeReturn, lastPageInStack);
            });
        }

        private static bool CanNavigate<T>(Action executeBeforeReturn, Page lastPageInStack)
        {
            //checa se existe uma mesma página na última posição da navigation stack
            if (lastPageInStack?.GetType() == typeof(T))
            {
                if (executeBeforeReturn != null)
                    executeBeforeReturn.Invoke();

                return false;
            }

            return true;
        }

        /// <summary>
        /// Use esse método para previnir de executar duplo clique nas navegações entre views PopUp(impedindo de abrir mais de uma instância)
        /// </summary>
        /// <typeparam name="T">Tipo de View popup que será checada no navigation stack</typeparam>
        /// <param name="executeBeforeReturn">vai executar sempre antes do CanNavigate retornar</param>
        /// <returns></returns>
        public static Task<bool> CanPopUpNavigateAsync<T>(Action executeBeforeReturn = null)
        {
            return Task.Run(() =>
            {
                var lastPageInStack = Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopupStack.LastOrDefault();
                return CanNavigate<T>(executeBeforeReturn, lastPageInStack);
            });
        }
    }
}
