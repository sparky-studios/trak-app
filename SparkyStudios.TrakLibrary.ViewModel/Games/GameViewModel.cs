﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Microsoft.AppCenter.Crashes;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SparkyStudios.TrakLibrary.Model.Games;
using SparkyStudios.TrakLibrary.Model.Response;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.Service.Exception;
using SparkyStudios.TrakLibrary.ViewModel.Common;

namespace SparkyStudios.TrakLibrary.ViewModel.Games
{
    /// <summary>
    /// The <see cref="GameViewModel" /> is a simple view model that is associated with the game page view.
    /// Its responsibility is to display all the information for a given game and respond to events such as adding,
    /// moving or removing a game from a users' collection.
    /// </summary>
    public class GameViewModel : ReactiveViewModel
    {
        private readonly IRestService _restService;
        private readonly IStorageService _storageService;

        private long _gameId;
        private IEnumerable<Platform> _platforms;
        
        /// <summary>
        /// Constructor that is invoked by the Prism DI framework to inject all of the needed dependencies.
        /// The constructors should never be invoked outside of the Prism DI framework. All instantiation
        /// should be handled by the framework.
        /// </summary>
        /// <param name="scheduler">The <see cref="IScheduler" /> instance to inject.</param>
        /// <param name="navigationService">The <see cref="INavigationService" /> instance to inject.</param>
        /// <param name="storageService">The <see cref="IStorageService" /> instance to inject.</param>
        /// <param name="restService">The <see cref="IRestService" /> instance to inject.</param>
        public GameViewModel(IScheduler scheduler, INavigationService navigationService, IStorageService storageService,
            IRestService restService) : base(scheduler, navigationService)
        {
            _storageService = storageService;
            _restService = restService;

            // Default values
            Status = GameUserEntryStatus.None;

            // The page will be busy by default, as as soon as it's navigated to, API requests are made.
            SimilarGames = new ObservableCollection<ListItemViewModel>();

            OptionsCommand = ReactiveCommand.CreateFromTask(OptionsAsync, outputScheduler: scheduler);

            LoadGameInfoCommand = ReactiveCommand.CreateFromTask(LoadGameInfoAsync, outputScheduler: scheduler);
            // Report errors if an exception was thrown.
            LoadGameInfoCommand.ThrownExceptions.Subscribe(ex =>
            {
                IsError = true;
                if (!(ex is ApiException))
                {
                    Crashes.TrackError(ex);
                }
            });

            OnRatingTappedCommand =
                ReactiveCommand.CreateFromTask<string, bool>(async rating => await OnRatingTappedAsync(rating),
                    outputScheduler: scheduler);
            // Report errors if an exception was thrown.
            OnRatingTappedCommand.ThrownExceptions.Subscribe(ex =>
            {
                IsError = true;
                if (!(ex is ApiException))
                {
                    Crashes.TrackError(ex);
                }
            });

            this.WhenAnyObservable(x => x.LoadGameInfoCommand.IsExecuting)
                .ToPropertyEx(this, x => x.IsLoading, scheduler: scheduler);
        }

        /// <summary>
        /// A <see cref="bool"/> that specifies whether the page has been fully loaded.
        /// </summary>
        [Reactive]
        public bool HasLoaded { get; set; }

        /// <summary>
        /// A <see cref="Uri"/> which specifies the URI from which the <see cref="GameInfo"/> was loaded from.
        /// </summary>
        [Reactive]
        public Uri GameUrl { get; set; }

        /// <summary>
        /// A ID of the current <see cref="Platform"/> the <see cref="Game"/> has been added for.
        /// </summary>
        [Reactive]
        public long PlatformId { get; set; }

        /// <summary>
        /// A <see cref="Uri" /> that contains the URL of the image that is associated with the game within this view model.
        /// </summary>
        [Reactive]
        public Uri ImageUrl { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}" /> that contains all the platforms for the given game, or a single platform if
        /// it is already in the users library.
        /// </summary>
        [Reactive]
        public IEnumerable<Platform> Platforms { get; set; }

        /// <summary>
        /// A <see cref="string" /> that represents the name of the game.
        /// </summary>
        [Reactive]
        public string GameTitle { get; set; }

        /// <summary>
        /// A <see cref="DateTime" /> that represents the release date of the game within the view model.
        /// </summary>
        [Reactive]
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        /// A <see cref="string" /> that contains a comma-separated list of all publishers associated with the game within
        /// the view model.
        /// </summary>
        [Reactive]
        public IEnumerable<Publisher> Publishers { get; set; }

        /// <summary>
        /// A <see cref="bool" /> that is used to represent whether the game being displayed is currently within the logged in
        /// users library. If this game is within the users library, additional information such as rating and status will
        /// be displayed.
        /// </summary>
        [Reactive]
        public bool InLibrary { get; set; }

        /// <summary>
        /// A <see cref="short" /> that represents the current user rating of the game. If the game being displayed isn't
        /// within the users collection, this value will be set to 0.
        /// </summary>
        [Reactive]
        public short Rating { get; set; }

        /// <summary>
        /// An <see cref="IEnumerable{T}" /> that contains all of the <see cref="Genre"/> objects associated with the given
        /// <see cref="Game"/>.
        /// </summary>
        [Reactive]
        public IEnumerable<Genre> Genres { get; set; }

        /// <summary>
        /// A <see cref="string" /> that represents the description of the game.
        /// </summary>
        [Reactive]
        public string Description { get; set; }

        /// <summary>
        /// A <see cref="GameUserEntryStatus" /> that represents the current status of the game. If the game being
        /// displayed isn't within the users collection, the status will be set to <see cref="GameUserEntryStatus.None" />.
        /// </summary>
        [Reactive]
        public GameUserEntryStatus Status { get; set; }

        /// <summary>
        /// A <see cref="ObservableCollection{T}" /> that is used to represent a short collection of games that
        /// are within the same genre as the one that is being loaded by the user.
        /// </summary>
        [Reactive]
        public ObservableCollection<ListItemViewModel> SimilarGames { get; set; }

        /// <summary>
        /// Command that is invoked each time the Add or options button is tapped by the user on the view.
        /// When called, the command will propagate the request and call the <see cref="OptionsAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> OptionsCommand { get; }

        /// <summary>
        /// Command that is invoked each time the page is first navigated to. When called, the command will
        /// propagate the request and call the <see cref="LoadGameInfoAsync"/> method.
        /// </summary>
        public ReactiveCommand<Unit, Unit> LoadGameInfoCommand { get; }

        /// <summary>
        /// Command that is invoked each time any of the rating stars are tapped by the user. When called,
        /// the command will propagate the request and call the <see cref="OnRatingTappedAsync"/> method.
        /// </summary>
        public ReactiveCommand<string, bool> OnRatingTappedCommand { get; }

        /// <summary>
        /// Overriden method that is automatically invoked when the page is navigated to. Its purpose is to retrieve
        /// values from the <see cref="NavigationParameters" /> before invoking <see cref="LoadGameInfoAsync" /> to
        /// load and display additional information to the view.
        /// </summary>
        /// <param name="parameters">The <see cref="NavigationParameters" />, which contains information for display purposes.</param>
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            // Retrieve the url we're going to use to retrieve the base game data.
            GameUrl = parameters["game-url"] as Uri;
            PlatformId = parameters["platform-id"] as long? ?? 0L;

            // This information will only be valid if the user clicked on a game entry from their own collection.
            InLibrary = parameters["in-library"] as bool? ?? false;
            Rating = parameters["rating"] as short? ?? (short) 0;
            Status = parameters["status"] as GameUserEntryStatus? ?? GameUserEntryStatus.None;

            LoadGameInfoCommand.Execute().Subscribe();
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="OptionsCommand"/>. When invoked, it will either navigate the user to the
        /// game options page or the add game page, depending on whether the currently selected game is already within the user collection
        /// or not.
        /// </summary>
        /// <returns>A <see cref="Task"/> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task OptionsAsync()
        {
            if (InLibrary)
            {
                var parameters = new NavigationParameters
                {
                    {"game-url", GameUrl},
                    {"game-id", _gameId},
                    {"platform-id", PlatformId},
                    {"status", Status}
                };

                await NavigationService.NavigateAsync("GameOptionsPage", parameters);
            }
            else
            {
                var parameters = new NavigationParameters
                {
                    {"game-url", GameUrl},
                    {"platforms", _platforms},
                    {"game-id", _gameId}
                };

                await NavigationService.NavigateAsync("AddGamePage", parameters);
            }
        }

        /// <summary>
        /// Private method that is invoked by the <see cref="LoadGameInfoCommand" /> when activated by the associated
        /// view. This method will attempt to retrieve the game information from the url provided by the
        /// <see cref="NavigationParameters" /> and populate all of the information within this view model with data
        /// from the returned <see cref="GameInfo" />. If any errors occur during the API requests, the exceptions
        /// are caught and the errors the IsError boolean to true.
        /// </summary>
        /// <returns>A <see cref="Task" /> which specifies whether the asynchronous task completed successfully.</returns>
        private async Task LoadGameInfoAsync()
        {
            // We're going to make some requests, so we're busy and remove any current errors.
            IsError = false;
            HasLoaded = false;
            
            // Retrieve the game and set some game information on the view.
            var gameInfo = await _restService.GetAsync<GameInfo>(GameUrl.OriginalString);
            _gameId = gameInfo.Id;
            ImageUrl = gameInfo.GetLink("image");
            GameTitle = gameInfo.Title;
            ReleaseDate = gameInfo.ReleaseDate;
            Description = gameInfo.Description;
            Publishers = gameInfo.Publishers;
            Genres = gameInfo.Genres;

            _platforms = gameInfo.Platforms;
            Platforms = InLibrary ? new[] {gameInfo.Platforms.First(p => p.Id == PlatformId)} : _platforms;

            // Grab the first genre and load some game information from it.
            if (Genres.Any())
            {
                var games = await _restService.GetAsync<HateoasPage<GameInfo>>(
                    Genres.First().GetLink("gameInfos").OriginalString);

                CreateSimilarGamesList(games.Embedded.Data.Where(x => x.Id != gameInfo.Id));
            }

            HasLoaded = true;
        }

        private async Task<bool> OnRatingTappedAsync(string rating)
        {
            // Update the rating to reflect on the view.
            Rating = short.Parse(rating);
            
            // Get the user ID, needed to update the correct game user entry.
            var userId = await _storageService.GetUserIdAsync();
            
            // Get the details of the user entry that the rating is going to be updated for.
            var entry = await _restService.GetAsync<HateoasPage<GameUserEntry>>(
                $"games/entries?user-id={userId}&platform-id={PlatformId}&game-id={_gameId}");

            if (entry.Embedded != null)
            {
                // Update the entry via patch with only the rating changed.
                await _restService.PatchAsync<GameUserEntry>($"games/entries/{entry.Embedded.Data.First().Id}",
                    new Dictionary<string, object>
                    {
                        {nameof(GameUserEntry.Rating), Rating}
                    });
            }
            
            return true;
        }

        /// <summary>
        /// Private method that is invoked within the <see cref="LoadGameInfoAsync" /> method. Its purpose
        /// is to convert the provided <see cref="IEnumerable{T}" /> of <see cref="GameInfo" /> instances into
        /// <see cref="ListItemViewModel" /> instances for displaying within the list on the game page view.
        /// </summary>
        /// <param name="games">The <see cref="IEnumerable{T}" /> to convert into <see cref="ListItemViewModel" /> instances.</param>
        private void CreateSimilarGamesList(IEnumerable<GameInfo> games)
        {
            var similarGames = new ObservableCollection<ListItemViewModel>();
            foreach (var game in games)
                similarGames.Add(new ListItemViewModel
                {
                    ImageUrl = game.GetLink("image"),
                    Header = string.Join(", ", game.Platforms.Select(x => x.Name)),
                    ItemTitle = game.Title,
                    ItemSubTitle = $"{game.ReleaseDate:MMMM yyyy}, {string.Join(", ", game.Publishers.Select(x => x.Name))}"
                });

            SimilarGames = similarGames;
        }
    }
}