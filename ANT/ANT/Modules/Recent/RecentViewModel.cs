using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;
using magno = MvvmHelpers.Commands;
using ANT.Interfaces;
using System.Threading.Tasks;
using ANT.Model;
using System.Linq;
using System.Windows.Input;
using ANT.Core;

namespace ANT.Modules
{
    public class RecentViewModel : BaseViewModel
    {
        public RecentViewModel()
        {
            ClearAllRecentCommand = new magno.AsyncCommand(OnClearAllRecent);

            Recents = new ObservableRangeCollection<RecentVisualized>();
        }

        public async Task LoadAsync(object param)
        {
            await Task.Run(() =>
            {
                var sortedRecents = App.RecentAnimes.OrderByDescending(p => p.Date).ToList();
                Recents.ReplaceRange(sortedRecents);
            });
        }

        #region propriedades vm
        private ObservableRangeCollection<RecentVisualized> _recents;
        public ObservableRangeCollection<RecentVisualized> Recents
        {
            get { return _recents; }
            set { SetProperty(ref _recents, value); }
        }

        //TODO: implementar o comando de item selecionado, navegar para a AnimeSpecsViewModel após clicar

        #endregion

        #region commands
        public ICommand ClearAllRecentCommand { get; private set; }
        private async Task OnClearAllRecent()
        {
            if (Recents.Count == 0)
                return;

            //TODO: chamar aqui o modal de confirmação quando ele existir 
            //https://github.com/JaoHundred/ANT/issues/16

            await Task.Run(() =>
            {
                App.RecentAnimes.Clear();
                JsonStorage.SaveDataAsync(App.RecentAnimes, StorageConsts.LocalAppDataFolder, StorageConsts.RecentAnimesFileName);
            });

            Recents.Clear();
        }
        #endregion
    }
}
