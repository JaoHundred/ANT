using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;
using magno = MvvmHelpers.Commands;
using ANT.Interfaces;
using System.Threading.Tasks;
using ANT.Model;

namespace ANT.Modules
{
    public class RecentViewModel : BaseViewModel
    {
        public RecentViewModel()
        {
            Recents = new ObservableRangeCollection<RecentVisualized>();
            Recents.AddRange(App.RecentAnimes);
        }

        #region propriedades vm
        private ObservableRangeCollection<RecentVisualized> _recents;
        public ObservableRangeCollection<RecentVisualized> Recents
        {
            get { return _recents; }
            set { SetProperty(ref _recents, value); }
        }

        #endregion
    }
}
