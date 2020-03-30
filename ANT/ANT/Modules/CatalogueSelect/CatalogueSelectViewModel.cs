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
            OpenGlobalCatalogueCommand = new AsyncCommand(OnOpenGlobalCatalogue);
        }

        #region comandos
        public ICommand OpenSeasonCatalogueCommand { get; private set; }
        private async Task OnOpenSeasonCatalogue()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500)); // necessário para não atropelar a animação do botão
            if (IsNotBusy)
            {
                IsBusy = true;
                await NavigationManager.NavigateShellAsync<CatalogueViewModel>();
                IsBusy = false;
            }
        }

        public ICommand OpenGlobalCatalogueCommand { get; private set; }
        private async Task OnOpenGlobalCatalogue()
        {
            await App.DelayRequest(); // só para simular alguma coisa por hora, remover depois que tiver funcionalidade

            //TODO: passar para catalogueview com um mecanismo indicando que é pra carregar TODOS os animes de todos os tempos
            //(ele vai precisar ter o mesmo funcionamento que os animes por gênero indo de AnimeSpecs para Catalogue ao clicar em um gênero)
        }
        #endregion
    }
}
