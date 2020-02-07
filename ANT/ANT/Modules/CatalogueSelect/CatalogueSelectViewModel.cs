using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using ANT.Core;
using ANT.UTIL;
using Xamarin.Forms;
using MvvmHelpers;
using MvvmHelpers.Commands;
using System.Threading.Tasks;

namespace ANT.Modules
{
    public class CatalogueSelectViewModel : BaseVMExtender
    {
        public CatalogueSelectViewModel()
        {
            OpenSeasonCatalogueCommand = new AsyncCommand(OnOpenSeasonCatalogue);
        }


        #region comandos
        public ICommand OpenSeasonCatalogueCommand { get; private set; }
        private async Task OnOpenSeasonCatalogue()
        {

            if (IsNotBusy)
            {
                IsBusy = true;
                await NavigationManager.NavigateShellAsync<CatalogueViewModel>();
                IsBusy = false;
            }
        }
        #endregion
    }
}
