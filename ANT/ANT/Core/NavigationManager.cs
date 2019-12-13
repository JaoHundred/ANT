using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Rg.Plugins.Popup;
using Rg.Plugins.Popup.Pages;
using System.Linq;
using ANT.UTIL;
using MvvmHelpers;
using System.Reflection;
using ANT.Modules;

namespace ANT.Core
{
    /// <summary>
    /// Gerenciador de navegações, usado para navegações hierarquicas e popup, possui formas de disparar navegação somente uma vez
    /// (evitando o duplo clique equivocado do usuário)
    /// </summary>
    public static class NavigationManager
    {

        public static int PageStackCount
        {
            get
            {
                return Shell.Current.Navigation.NavigationStack.Count;
            }
        }

        public static int PopUpPageStackCount
        {
            get
            {
                return Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopupStack.Count;
            }
        }

        #region Métodos de navegação

        public static async Task NavigateShellAsync<T>(params object[] param) where T : BaseViewModel
        {
            Page view = await CreatePageAndBindAsync<T>(param);

            await Shell.Current.Navigation.PushAsync(view, animated: true);
        }

        public static async Task NavigatePopUpAsync<T>(params object[] param) where T : BaseViewModel
        {
            PopupPage view = await CreatePopUpPageAndBind<T>(param);

            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(view);
        }

        public static async Task PopShellPageAsync(bool animated = true)
        {
            await Shell.Current.Navigation.PopAsync(animated);
        }

        public static async Task PopPopUpPageAsync(bool animated = true)
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync(animated);
        }

        /// <summary>
        /// Use esse método para previnir de executar duplo clique nas navegações entre views(impedindo de abrir mais de uma instância)
        /// usar tipos View
        /// </summary>
        /// <param name="executeBeforeReturn">vai executar sempre antes do CanNavigate retornar</param>
        /// <returns></returns>
        public static Task<bool> CanShellNavigateAsync<T>(Action executeBeforeReturn = null) where T : Page
        {
            return Task.Run(() =>
            {
                var lastPageInStack = Shell.Current.Navigation.NavigationStack.LastOrDefault();
                return CanNavigate<T>(executeBeforeReturn, lastPageInStack);
            });
        }

        /// <summary>
        /// Use esse método para previnir de executar duplo clique nas navegações entre views PopUp(impedindo de abrir mais de uma instância)
        /// Usar tipos View
        /// </summary>
        /// <param name="executeBeforeReturn">vai executar sempre antes do CanNavigate retornar</param>
        /// <returns></returns>
        public static Task<bool> CanPopUpNavigateAsync<T>(Action executeBeforeReturn = null) where T : Page
        {
            return Task.Run(() =>
            {
                var lastPageInStack = Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopupStack.LastOrDefault();
                return CanNavigate<T>(executeBeforeReturn, lastPageInStack);
            });
        }
        #endregion

        #region Métodos auxiliares
        private static Task<Page> CreatePageAndBindAsync<T>(object[] param) where T : BaseViewModel
        {
            return Task.Run(() =>
            {
                string absoluteName = typeof(T).AssemblyQualifiedName + typeof(T).FullName;

                Type type = Type.GetType(absoluteName.Replace("ViewModel", "View"));
                Page view = (Page)Activator.CreateInstance(type);

                BaseVMExtender viewModel =
                    param != null ?
                    (BaseVMExtender)Activator.CreateInstance(Type.GetType(absoluteName), param)
                    : (BaseVMExtender)Activator.CreateInstance(Type.GetType(absoluteName));

                view.BindingContext = viewModel;
                return view;
            });
        }

        private static Task<PopupPage> CreatePopUpPageAndBind<T>(object[] param) where T : BaseViewModel
        {
            return Task.Run(() =>
            {
                string absoluteName = typeof(T).AssemblyQualifiedName + typeof(T).FullName;

                Type type = Type.GetType(absoluteName.Replace("ViewModel", "View"));
                PopupPage view = (PopupPage)Activator.CreateInstance(type);

                BaseVMExtender viewModel =
                    param != null ?
                    (BaseVMExtender)Activator.CreateInstance(Type.GetType(absoluteName), param)
                    : (BaseVMExtender)Activator.CreateInstance(Type.GetType(absoluteName));

                view.BindingContext = viewModel;
                return view;
            });
        }

        private static bool CanNavigate<T>(Action executeBeforeReturn, Page lastPageInStack) where T : Page
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
        #endregion
    }
}
