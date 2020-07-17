using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Text;
using ANT.Interfaces;
using System.Threading.Tasks;
using JikanDotNet;
using System.Linq;
using System.Threading;
using ANT.Model;
using Xamarin.Forms;

namespace ANT.Modules
{
    public class HomeViewModel : BaseViewModel
    {
        public HomeViewModel()
        { }

        public Task InitializeTask { get; }
        private CancellationTokenSource _cancellationTokenSource;

        public void Unload()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        public async Task LoadAsync()
        {
            while (App.liteDB == null)
                await Task.Delay(TimeSpan.FromMilliseconds(100));

            _cancellationTokenSource = new CancellationTokenSource();

            //TODO:carregar aleatório um dos animes favoritados
            var settings = App.liteDB.GetCollection<SettingsPreferences>().FindById(0);

            var animeCollection = settings.ShowNSFW
                 ? App.liteDB.GetCollection<FavoritedAnime>().FindAll().ToList()
                 : App.liteDB.GetCollection<FavoritedAnime>().Find(p => !p.IsNSFW).ToList();

            int collectionCount = animeCollection.Count;

            if (collectionCount == 0)
                return;

            try
            {
                await Task.Run(async () =>
                {
                    await App.DelayRequest(2);
                    var rand = new Random();

                    int indexPick = rand.Next(collectionCount);

                    FavoritedAnime animeAsRecommend = animeCollection[indexPick];

                    if (_cancellationTokenSource.IsCancellationRequested)
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    Recommendations recommendations = await App.Jikan.GetAnimeRecommendations(animeAsRecommend.Anime.MalId);

                    if (recommendations.RecommendationCollection.Count == 0)
                        return;

                    var recomendationsList = new HashSet<Recommendation>();
                    var recomendations = recommendations.RecommendationCollection.ToList();

                    for (int i = 0; i < 10; i++)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(50));
                        indexPick = rand.Next(recomendations.Count);

                        recomendationsList.Add(recomendations[indexPick]);
                    }

                    //TODO: guardar lista de recomendados no banco para quando não houver recomendações exibir a última sequência recomendada?

                    Recommendations = recomendationsList.ToList();

                }, _cancellationTokenSource.Token);
            }
            catch (JikanDotNet.Exceptions.JikanRequestException ex)
            {
                DependencyService.Get<IToast>().MakeToastMessageLong($"Erros code {ex.ResponseCode}");
            }
            catch (Exception ex)
            {
                DependencyService.Get<IToast>().MakeToastMessageLong(Lang.Lang.Error);
            }

        }

        #region properties
        private IList<Recommendation> _recomendations;
        public IList<Recommendation> Recommendations
        {
            get { return _recomendations; }
            set { SetProperty(ref _recomendations, value); }
        }
        #endregion

    }
}
