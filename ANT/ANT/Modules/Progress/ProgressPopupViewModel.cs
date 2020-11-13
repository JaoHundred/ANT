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
using JikanDotNet.Exceptions;

namespace ANT.Modules
{
    public class ProgressPopupViewModel : BaseViewModel, IAsyncInitialization
    {

        //TODO: https://github.com/JaoHundred/ANT/issues/80

        public ProgressPopupViewModel(object collection, BaseViewModel viewModelType)
        {
            _cancelationToken = new CancellationTokenSource();
            _collection = collection;
            _viewModelType = viewModelType;
            InitializeTask = LoadAsync(null);
            CancelProcessCommand = new magno.Command(OnCancel);
        }

        public ProgressPopupViewModel(object collection, BaseViewModel viewModelType, Action actionAfterClose)
        {
            _cancelationToken = new CancellationTokenSource();
            _collection = collection;
            _viewModelType = viewModelType;
            _actionAfterClose = actionAfterClose;
            InitializeTask = LoadAsync(null);
            CancelProcessCommand = new magno.Command(OnCancel);
        }

        private CancellationTokenSource _cancelationToken;
        private BaseViewModel _viewModelType;
        private Action _actionAfterClose;
        private object _collection;
        public Task InitializeTask { get; }
        public async Task LoadAsync(object param)
        {
            try
            {
                await Task.Run(async () =>
                {
                    if (_viewModelType is CatalogueViewModel && _collection is IList<FavoritedAnime>)
                    {
                        await FavoriteAnimesFromCatalogue();
                    }
                    else if (_viewModelType is FavoriteAnimeViewModel && _collection is IList<FavoritedAnime>)
                    {
                        await UpdateAnimesFromFavorited();

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            DependencyService.Get<IToast>().MakeToastMessageLong(Lang.Lang.UpdatedAnimes);
                        });
                    }
                    else if (_viewModelType is FavoriteCharacterViewModel && _collection is IList<FavoritedAnimeCharacter>)
                    {
                        await UpdateCharactersFromFavorited();

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            DependencyService.Get<IToast>().MakeToastMessageLong(Lang.Lang.UpdatedCharacters);
                        });
                    }
                    else if (_viewModelType is FavoriteVoiceActorViewModel && _collection is IList<FavoritedVoiceActor>)
                    {
                        await UpdateVoiceActorFromFavorited();

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            DependencyService.Get<IToast>().MakeToastMessageLong(Lang.Lang.UpdatedVoiceActors);
                        });
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
            finally
            {
                if (_actionAfterClose != null)
                    _actionAfterClose.Invoke();

                MessagingCenter.Send<ProgressPopupViewModel, double>(this, string.Empty, 1);
                //necessário para não bugar o comportamento da popup, abrir e fechar muito rápido causa efeitos não esperados e mantém a popup aberta para sempre
                await App.DelayRequest(2);

                await NavigationManager.PopPopUpPageAsync();
            }
        }

        private async Task FavoriteAnimesFromCatalogue()
        {
            try
            {
                var favoriteCollection = App.liteDB.GetCollection<FavoritedAnime>();
                var collection = _collection as IList<FavoritedAnime>;

                for (int i = 0; i < collection.Count; i++)
                {
                    double result = (double)i / collection.Count;
                    MessagingCenter.Send<ProgressPopupViewModel, double>(this, string.Empty, result);

                    if (favoriteCollection.FindById(collection[i].Anime.MalId) != null)
                        continue;

                    await App.DelayRequest(4);

                    if (_cancelationToken != null && _cancelationToken.IsCancellationRequested)
                        _cancelationToken.Token.ThrowIfCancellationRequested();

                    Anime anime = await App.Jikan.GetAnime(collection[i].Anime.MalId);
                    anime.RequestCached = true;

                    var favoritedAnime = new FavoritedAnime(anime/*, await anime.GetAllEpisodesAsync(_cancelationToken)*/);
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

                    collection[i].IsFavorited = true;

                    favoriteCollection.Upsert(favoritedAnime.Anime.MalId, favoritedAnime);


                }
            }
            catch (JikanRequestException ex)
            {
                ex.SaveExceptionData();
            }
            catch (OperationCanceledException ex)
            { }
            catch (Exception ex)
            {
                ex.SaveExceptionData();
            }
        }

        private async Task UpdateAnimesFromFavorited()
        {

            //TODO: https://github.com/JaoHundred/ANT/issues/80


            //TODO:testar mais algumas vezes, na primeira tentativa no dispositivo real
            //foram feitas trocas de aplicativo enquanto essa função continuava funcionando
            //ao terminar não foi completado todos as atualizações da lista, mas o processamento não parou
            //depois que terminou, ao clicar mais vezes em atualizar o restante que não tinha sido atualizado foi atualizando
            try
            {
                var db = App.liteDB.GetCollection<FavoritedAnime>();
                var animes = _collection as List<FavoritedAnime>;
                double total = animes.Count;

                for (int i = 0; i < animes.Count; i++)
                {
                    double result = (double)i / total;
                    MessagingCenter.Send<ProgressPopupViewModel, double>(this, string.Empty, result);

                    if (_cancelationToken != null && _cancelationToken.IsCancellationRequested)
                        _cancelationToken.Token.ThrowIfCancellationRequested();

                    var favoriteAnime = animes[i];

                    if ((favoriteAnime.LastUpdateDate == null)
                        || (favoriteAnime.LastUpdateDate != null && favoriteAnime.LastUpdateDate != DateTime.Today))
                    {
                        await App.DelayRequest(4);

                        Anime anime = await App.Jikan.GetAnime(favoriteAnime.Anime.MalId);
                        anime.RequestCached = true;

                        int lastEpisode = favoriteAnime.LastEpisodeSeen;
                        bool hasNotification = favoriteAnime.CanGenerateNotifications;

                        //TODO:linha dos episódios comentados pelo motivo do MAL estar instável e isso gerar uma carga desnecessária
                        //a informação importante é apenas os dados do anime
                        favoriteAnime = new FavoritedAnime(anime/*, await anime.GetAllEpisodesAsync(_cancelationToken)*/);
                        favoriteAnime.LastUpdateDate = DateTime.Today;
                        favoriteAnime.IsFavorited = true;
                        favoriteAnime.LastEpisodeSeen = lastEpisode;
                        favoriteAnime.NextStreamDate = await favoriteAnime.NextEpisodeDateAsync();


                        //TODO:linha abaixo de testes, remover quando conseguir corrigir o problema da https://github.com/JaoHundred/ANT/issues/80
                        //favoriteAnime.Anime.Airing = false;

                        //se está exibindo e possui data de estreia
                        favoriteAnime.CanGenerateNotifications =
                            favoriteAnime.Anime.Airing && favoriteAnime.NextStreamDate != null ? hasNotification : false;

                        db.Update(favoriteAnime.Anime.MalId, favoriteAnime);
                       
                    }
                }
            }
            catch (JikanRequestException ex)
            {
                ex.SaveExceptionData();
            }
            catch (OperationCanceledException ex)
            { }
            catch (Exception ex)
            {
                ex.SaveExceptionData();
            }
            finally
            {
                App.liteDB.Checkpoint();
            }
        }
        private async Task UpdateCharactersFromFavorited()
        {
            //TODO:testar mais algumas vezes, na primeira tentativa no dispositivo real
            //foram feitas trocas de aplicativo enquanto essa função continuava funcionando
            //ao terminar não foi completado todos as atualizações da lista, mas o processamento não parou
            //depois que terminou, ao clicar mais vezes em atualizar o restante que não tinha sido atualizado foi atualizando
            try
            {
                var db = App.liteDB.GetCollection<FavoritedAnimeCharacter>();
                var characters = _collection as List<FavoritedAnimeCharacter>;
                double total = characters.Count;

                for (int i = 0; i < characters.Count; i++)
                {
                    double result = (double)i / total;
                    MessagingCenter.Send<ProgressPopupViewModel, double>(this, string.Empty, result);

                    await App.DelayRequest(4);

                    if (_cancelationToken != null && _cancelationToken.IsCancellationRequested)
                        _cancelationToken.Token.ThrowIfCancellationRequested();

                    var chara = db.FindById(characters[i].Character.MalId);

                    Character character = await App.Jikan.GetCharacter(chara.Character.MalId);
                    character.RequestCached = true;

                    await App.DelayRequest(4);

                    CharacterPictures characterPictures = await App.Jikan.GetCharacterPictures(chara.Character.MalId);
                    characterPictures.RequestCached = true;

                    chara = new FavoritedAnimeCharacter(character, characterPictures.Pictures.ToList());
                    chara.IsFavorited = true;

                    db.Update(chara.Character.MalId, chara);
                }
            }
            catch (JikanRequestException ex)
            {
                ex.SaveExceptionData();
            }
            catch (OperationCanceledException ex)
            { }
            catch (Exception ex)
            {
                ex.SaveExceptionData();
            }
        }

        private async Task UpdateVoiceActorFromFavorited()
        {
            try
            {
                var db = App.liteDB.GetCollection<FavoritedVoiceActor>();
                var voiceActors = _collection as IList<FavoritedVoiceActor>;
                double total = voiceActors.Count;

                for (int i = 0; i < voiceActors.Count; i++)
                {
                    double result = (double)i / total;
                    MessagingCenter.Send<ProgressPopupViewModel, double>(this, string.Empty, result);

                    await App.DelayRequest(4);

                    if (_cancelationToken != null && _cancelationToken.IsCancellationRequested)
                        _cancelationToken.Token.ThrowIfCancellationRequested();

                    var voiceAc = db.FindById(voiceActors[i].VoiceActor.MalId);

                    Person person = await App.Jikan.GetPerson(voiceAc.VoiceActor.MalId);
                    person.RequestCached = true;

                    await App.DelayRequest(4);

                    PersonPictures characterPictures = await App.Jikan.GetPersonPictures(voiceAc.VoiceActor.MalId);
                    characterPictures.RequestCached = true;

                    voiceAc = new FavoritedVoiceActor(person, characterPictures.Pictures.ToList());
                    voiceAc.IsFavorited = true;

                    db.Update(voiceAc.VoiceActor.MalId, voiceAc);
                }
            }
            catch (JikanRequestException ex)
            {
                ex.SaveExceptionData();
            }
            catch (OperationCanceledException ex)
            { }
            catch (Exception ex)
            {
                ex.SaveExceptionData();
            }
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
