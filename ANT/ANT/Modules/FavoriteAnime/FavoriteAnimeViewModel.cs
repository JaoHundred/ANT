using System;
using System.Collections.Generic;
using System.Text;
using magno = MvvmHelpers.Commands;
using MvvmHelpers;
using ANT.UTIL;
using System.Windows.Input;
using System.Threading.Tasks;
using ANT.Interfaces;

namespace ANT.Modules
{
    public class FavoriteAnimeViewModel : BaseVMExtender
    {
        public FavoriteAnimeViewModel()
        {
            SearchCommand = new magno.AsyncCommand(OnSearch);
            ClearTextCommand = new magno.AsyncCommand(OnClearText);
            DeleteFavoriteCommand = new magno.AsyncCommand(OnDeleteFavoriteCommand);
        }

        //TODO: implementar esta VM e sua View
        public async Task LoadAsync(object param)
        {
            await App.DelayRequest();
        }

        #region Propriedades

        #endregion


        #region Commands
        public ICommand SearchCommand { get; private set; }
        private async Task OnSearch()
        {
            await App.DelayRequest();
        }

        public ICommand ClearTextCommand { get; private set; }

        private async Task OnClearText()
        {
            await App.DelayRequest();
        }

        public ICommand DeleteFavoriteCommand { get; private set; }

        private async Task OnDeleteFavoriteCommand()
        {
            await App.DelayRequest();
        } 
        #endregion

    }
}
