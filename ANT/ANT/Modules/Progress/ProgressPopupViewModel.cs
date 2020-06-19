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
        public ProgressPopupViewModel(IList<FavoritedAnime> animes)
        {
            _cancelationToken = new CancellationTokenSource();
            _animes = animes;
            InitializeTask = LoadAsync(null);
            CancelProcessCommand = new magno.Command(OnCancel);
        }

        private CancellationTokenSource _cancelationToken;
        private IList<FavoritedAnime> _animes;
        public Task InitializeTask { get; }
        public async Task LoadAsync(object param)
        {
            var favoriteCollection = App.liteDB.GetCollection<FavoritedAnime>();

            try
            {
                await Task.Run(async () =>
                {
                    for (int i = 0; i < _animes.Count; i++)
                    {
                        if (_cancelationToken.IsCancellationRequested)
                        {
                            await NavigationManager.PopPopUpPageAsync();
                            _cancelationToken.Token.ThrowIfCancellationRequested();
                        }

                        double result = (double)i / _animes.Count;
                        MessagingCenter.Send<ProgressPopupViewModel, double>(this, string.Empty, result);

                        if (favoriteCollection.FindAll().Any(p => p.Anime.MalId == _animes[i].Anime.MalId))
                            continue;

                        await App.DelayRequest(4);
                        Anime anime = await App.Jikan.GetAnime(_animes[i].Anime.MalId);
                        anime.RequestCached = true;

                        var favoritedAnime = new FavoritedAnime(anime, await anime.GetAllEpisodesAsync(_cancelationToken));
                        favoritedAnime.IsFavorited = true;
                        favoritedAnime.LastUpdateDate = DateTime.Today;
                        favoritedAnime.NextStreamDate = await favoritedAnime.NextEpisodeDateAsync();

                        int uniqueId = 0;

                        if (favoriteCollection.Count() > 0)
                        {
                            uniqueId = favoriteCollection.Max(p => p.UniqueNotificationID);

                            if (uniqueId == int.MaxValue)
                                uniqueId = 0;
                            else if (uniqueId < int.MaxValue)
                                uniqueId += 1;
                        }

                        favoritedAnime.UniqueNotificationID = uniqueId;

                        favoritedAnime.CanGenerateNotifications = favoritedAnime.Anime.Airing && favoritedAnime.NextStreamDate != null;

                        _animes[i].IsFavorited = true;

                        favoriteCollection.Upsert(favoritedAnime.Anime.MalId, favoritedAnime);
                    }
                }, _cancelationToken.Token);

            }
            catch (OperationCanceledException ex)
            {
                await App.DelayRequest(2);
                await NavigationManager.PopPopUpPageAsync();
                return;
            }
            catch (Exception ex)
            {
                await App.DelayRequest(2);
                await NavigationManager.PopPopUpPageAsync();
            }

            MessagingCenter.Send<ProgressPopupViewModel, double>(this, string.Empty, 1);
            //necessário para não bugar o comportamento da popup, abrir e fechar muito rápido causa efeitos não esperados e mantém a popup aberta para sempre
            await App.DelayRequest(2);
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
