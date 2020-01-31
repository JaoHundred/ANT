using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;
using magno = MvvmHelpers.Commands;
using ANT.Interfaces;
using System.Threading.Tasks;
using ANT.Model;
using System.Linq;

namespace ANT.Modules
{
    public class RecentViewModel : BaseViewModel
    {
        public RecentViewModel()
        {
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
    }
}
