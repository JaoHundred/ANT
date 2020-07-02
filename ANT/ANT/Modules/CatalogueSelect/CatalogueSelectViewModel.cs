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
            OpenCatalogueCommand = new AsyncCommand<CatalogueModeEnum>(OnOpenCatalogue);
        }

        #region comandos
        public ICommand OpenCatalogueCommand { get; private set; }
        private async Task OnOpenCatalogue(CatalogueModeEnum catalogueMode)
        {
            if (IsNotBusy)
            {
                IsBusy = true;
                await Task.Delay(TimeSpan.FromMilliseconds(500)); // necessário para não atropelar a animação do botão
                await NavigationManager.NavigateShellAsync<CatalogueViewModel>(catalogueMode);
                IsBusy = false;
            }
        }

        #endregion
    }
}
