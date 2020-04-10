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
            => Shell.Current.Navigation.NavigationStack.Count;

        public static int PopUpPageStackCount
            => Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopupStack.Count;

        public static IReadOnlyList<Page> PageStack
            => Shell.Current.Navigation.NavigationStack;

        public static IReadOnlyList<PopupPage> PopUpPageStack
            => Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopupStack;

        #region Métodos de navegação

        public static void RemovePageFromShellStack<T>() where T : BaseViewModel
        {
            Type viewType = GetViewFromViewModel<T>();
            var pageToRemove = Shell.Current.Navigation.NavigationStack.First(p => p?.GetType() == viewType);

            Shell.Current.Navigation.RemovePage(pageToRemove);
        }

        /// <summary>
        /// Remove navegações antigas, menos navegações para raiz e navegações da raiz hierárquica
        /// </summary>
        public static void RemoveAllPagesExceptRootAndHierarquicalRoot()
        {
            var pagesToRemove = new List<Page>();

            for (int i = PageStackCount - 1; i >= 0; i--)
                pagesToRemove.Add(Shell.Current.Navigation.NavigationStack[i]);

            foreach (var view in pagesToRemove)
                if (view != null
                    && view.GetType() != typeof(RecentView)
                    && view.GetType() != typeof(CatalogueSelectView))
                    Shell.Current.Navigation.RemovePage(view);
        }

        public static async Task NavigateShellAsync<T>(params object[] param) where T : BaseViewModel
        {
            var viewTask = CreatePageAndBindAsync<T>(param);

            await Shell.Current.Navigation.PushAsync(await viewTask, animated: true);
        }

        public static async Task NavigatePopUpAsync<T>(params object[] param) where T : BaseViewModel
        {
            Page view = await CreatePageAndBindAsync<T>(param);

            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync((PopupPage)view);
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
        /// Use esse método para previnir de executar duplo clique nas navegações entre views PopUp(impedindo de abrir mais de uma instância)
        /// Usar tipos View
        /// </summary>
        /// <param name="executeBeforeReturn">vai executar sempre antes do CanNavigate retornar</param>
        /// <returns></returns>
        public static Task<bool> CanPopUpNavigateAsync<T>(Action executeBeforeReturn = null) where T : BaseViewModel
        {
            var lastPageInStack = Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopupStack.LastOrDefault();
            return CanNavigateAsync<T>(executeBeforeReturn, lastPageInStack);
        }
        #endregion

        #region Métodos auxiliares
        private static async Task<Page> CreatePageAndBindAsync<T>(object[] param) where T : BaseViewModel
        {
            Type viewType = default;

            var reflectionTask = Task<ConstructorInfo>.Run(() =>
            {
                viewType = GetViewFromViewModel<T>();

                IEnumerable<Type> paramTypes = param.Select(p => p.GetType());

                return typeof(T).GetConstructor(paramTypes.ToArray());
            });

            ConstructorInfo constructor = await reflectionTask;
            var vm = (T)constructor.Invoke(param);

            ConstructorInfo viewConstructor = viewType.GetConstructor(Type.EmptyTypes);
            Page view = (Page)viewConstructor.Invoke(null);
            view.BindingContext = vm;

            return view;
        }

        private static Type GetViewFromViewModel<T>() where T : BaseViewModel
        {
            Type vmType;
            string absoluteName = typeof(T).AssemblyQualifiedName + typeof(T).FullName;

            vmType = Type.GetType(absoluteName.Replace("ViewModel", "View"));
            return vmType;
        }

        private static Task<bool> CanNavigateAsync<T>(Action executeBeforeReturn, Page lastPageInStack) where T : BaseViewModel
        {
            //checa se existe uma mesma página na última posição da navigation stack
            return Task.Run(() =>
            {
                Type viewType = GetViewFromViewModel<T>();

                if (lastPageInStack?.GetType() == viewType)
                {
                    if (executeBeforeReturn != null)
                        executeBeforeReturn.Invoke();

                    return false;
                }

                return true;
            });

        }
        #endregion
    }
}
