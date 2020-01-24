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
        public ProgressPopupViewModel(IList<FavoritedAnimeSubEntry> animes)
        {
            _cancelationToken = new CancellationTokenSource();
            _animes = animes;
            InitializeTask = LoadAsync(null);
            CancelProcessCommand = new magno.Command(OnCancel);
        }

        private CancellationTokenSource _cancelationToken;
        private IList<FavoritedAnimeSubEntry> _animes;
        public Task InitializeTask { get; }
        public async Task LoadAsync(object param)
        {
            int initialAnimeCount = App.FavoritedAnimes.Count;
            int finalAnimeCount = initialAnimeCount;

            await Task.Run(async () =>
            {
                for (int i = 0; i < _animes.Count; i++)
                {
                    if (_cancelationToken.IsCancellationRequested)
                    {
                        if (finalAnimeCount > initialAnimeCount)
                            await JsonStorage.SaveDataAsync(App.FavoritedAnimes, StorageConsts.LocalAppDataFolder, StorageConsts.FavoritedAnimesFileName);

                        await NavigationManager.PopPopUpPageAsync();
                        _cancelationToken.Token.ThrowIfCancellationRequested();
                    }

                    double result = (double)i / _animes.Count;
                    MessagingCenter.Send<ProgressPopupViewModel, double>(this, string.Empty, result);

                    FavoritedAnimeSubEntry favoritedSubAnimeAnime = _animes[i];
                    if (App.FavoritedAnimes.Exists(p => p.Anime.MalId == favoritedSubAnimeAnime.FavoritedAnime.MalId))
                        continue;

                    await Task.Delay(TimeSpan.FromSeconds(4));
                    Anime anime = await App.Jikan.GetAnime(favoritedSubAnimeAnime.FavoritedAnime.MalId);
                    anime.RequestCached = true;

                    var favoritedAnime = new FavoritedAnime(anime, await anime.GetAllEpisodesAsync());
                    favoritedAnime.IsFavorited = true;
                    favoritedSubAnimeAnime.IsFavorited = true;

                    App.FavoritedAnimes.Add(favoritedAnime);
                    finalAnimeCount++;
                }
            }, _cancelationToken.Token);

            if (finalAnimeCount > initialAnimeCount)
                await JsonStorage.SaveDataAsync(App.FavoritedAnimes, StorageConsts.LocalAppDataFolder, StorageConsts.FavoritedAnimesFileName);

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
