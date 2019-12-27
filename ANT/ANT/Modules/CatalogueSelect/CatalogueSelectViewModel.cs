using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using ANT.Core;
using ANT.UTIL;
using Xamarin.Forms;

namespace ANT.Modules
{
    public class CatalogueSelectViewModel : BaseVMExtender
    {
        public CatalogueSelectViewModel()
        {

        }


        #region comandos
        public ICommand OpenSeasonCatalogueCommand => new Command(async () =>
        {
            bool canNavigate = await NavigationManager.CanShellNavigateAsync<CatalogueView>();

            if (canNavigate)
                await NavigationManager.NavigateShellAsync<CatalogueViewModel>();
        });
        #endregion
    }
}
