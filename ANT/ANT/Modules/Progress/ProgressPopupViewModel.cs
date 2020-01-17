﻿using System;
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

namespace ANT.Modules
{
    public class ProgressPopupViewModel : BaseViewModel, IAsyncInitialization
    {
        public ProgressPopupViewModel(IList<FavoritedAnimeSubEntry> animes)
        {
            _cancelationToken = new CancellationTokenSource();
            InitializeTask = LoadAsync(animes);
            CancelProcessCommand = new magno.Command(OnCancel);
        }

        private CancellationTokenSource _cancelationToken;
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
                            await JsonStorage.SaveDataAsync(App.FavoritedAnimes, StorageConsts.LocalAppDataFolder, StorageConsts.FavoritedAnimesFileName);

                        await NavigationManager.PopPopUpPageAsync();
                        _cancelationToken.Token.ThrowIfCancellationRequested();
                    }

                    Device.BeginInvokeOnMainThread(() => { ProgressValue = (double)i / animeSubList.Count; });

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
                await JsonStorage.SaveDataAsync(App.FavoritedAnimes, StorageConsts.LocalAppDataFolder, StorageConsts.FavoritedAnimesFileName);

            ProgressValue = 1;
            //necessário para não bugar o comportamento da popup, abrir e fechar muito rápido causa efeitos não esperados e mantém a popup aberta para sempre
            await Task.Delay(TimeSpan.FromSeconds(2));
            await NavigationManager.PopPopUpPageAsync();
        }

        #region propriedades
        private double _progressValue;
        public double ProgressValue
        {
            get { return _progressValue; }
            set { SetProperty(ref _progressValue, value); }
        }

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
