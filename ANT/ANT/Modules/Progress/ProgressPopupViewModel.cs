using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ANT.Interfaces;
using MvvmHelpers;
using magno = MvvmHelpers.Commands;
using JikanDotNet;
using ANT.Model;
using System.Threading;
using ANT.Core;
using ANT.UTIL;
using Xamarin.Forms;
using System.Linq;

namespace ANT.Modules
{
    public class ProgressPopupViewModel : BaseViewModel, IAsyncInitialization
    {
        public ProgressPopupViewModel(ICommand updateCommand, IList<FavoritedAnimeSubEntry> animes)
        {
            _cancelationToken = new CancellationTokenSource();
            _updateCommand = updateCommand;
            InitializeTask = LoadAsync(animes);
            CancelProcessCommand = new magno.Command(OnCancel);
        }

        private CancellationTokenSource _cancelationToken;
        private ICommand _updateCommand;
        public Task InitializeTask { get; }
        public async Task LoadAsync(object param)
        {
            int initialAnimeCount = App.FavoritedAnimes.Count;
            int finalAnimeCount = initialAnimeCount;

            await Task.Run(async () =>
            {
                var animeSubList = (IList<FavoritedAnimeSubEntry>)param;

                for (int i = 0; i < animeSubList.Count; i++)
                {
                    if (_cancelationToken.IsCancellationRequested)
                    {
                        if (finalAnimeCount > initialAnimeCount)
                        {
                            await JsonStorage.SaveDataAsync(App.FavoritedAnimes, StorageConsts.LocalAppDataFolder, StorageConsts.FavoritedAnimesFileName);
                            Device.BeginInvokeOnMainThread(() => { _updateCommand.Execute(null); });
                        }

                        await NavigationManager.PopPopUpPageAsync();
                        _cancelationToken.Token.ThrowIfCancellationRequested();
                    }

                    double result = (double)i / animeSubList.Count;
                    MessagingCenter.Send<ProgressPopupViewModel, double>(this, string.Empty, result);

                    long id = animeSubList[i].FavoritedAnime.MalId;
                    if (App.FavoritedAnimes.Exists(p => p.Anime.MalId == id))
                        continue;

                    await Task.Delay(TimeSpan.FromSeconds(4));
                    Anime anime = await App.Jikan.GetAnime(id);
                    anime.RequestCached = true;

                    var favoritedAnime = new FavoritedAnime(anime, await anime.GetAllEpisodesAsync());
                    favoritedAnime.IsFavorited = true;

                    App.FavoritedAnimes.Add(favoritedAnime);
                    finalAnimeCount++;
                }
            }, _cancelationToken.Token);

            if (finalAnimeCount > initialAnimeCount)
            {
                await JsonStorage.SaveDataAsync(App.FavoritedAnimes, StorageConsts.LocalAppDataFolder, StorageConsts.FavoritedAnimesFileName);

                _updateCommand.Execute(null);
            }

            MessagingCenter.Send<ProgressPopupViewModel, double>(this, string.Empty, 1);
            //necessário para não bugar o comportamento da popup, abrir e fechar muito rápido causa efeitos não esperados e mantém a popup aberta para sempre
            await Task.Delay(TimeSpan.FromSeconds(2));
            await NavigationManager.PopPopUpPageAsync();
        }

        #region propriedades
        private bool _isFinalizing;
        public bool IsFinalizing
        {
            get { return _isFinalizing; }
            set { SetProperty(ref _isFinalizing, value); }
        }
        #endregion

        #region comandos
        public ICommand CancelProcessCommand { get; set; }
        private void OnCancel()
        {
            _cancelationToken.Cancel();
            IsFinalizing = true;
        }
        #endregion
    }
}
