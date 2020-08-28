﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using Acr.UserDialogs;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using Sparky.TrakApp.Model.Games;
using Sparky.TrakApp.Model.Response;
using Sparky.TrakApp.Service;
using Sparky.TrakApp.Service.Exception;
using Sparky.TrakApp.ViewModel.Games;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Test.Games
{
    [TestFixture]
    public class GameLibraryListViewModelTest
    {
        private Mock<INavigationService> _navigationService;
        private Mock<IRestService> _restService;
        private Mock<IStorageService> _storageService;
        private Mock<IUserDialogs> _userDialogs;
        private TestScheduler _scheduler;
        
        private GameLibraryListViewModel _gameLibraryListViewModel;
        
        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _restService = new Mock<IRestService>();
            _storageService = new Mock<IStorageService>();
            _userDialogs = new Mock<IUserDialogs>();
            _scheduler = new TestScheduler();

            _gameLibraryListViewModel = new GameLibraryListViewModel(_scheduler, _navigationService.Object, _restService.Object, _storageService.Object, _userDialogs.Object);
        }

        [Test]
        public void SearchCommand_ThrowsApiException_SetsErrorMessageAsApiError()
        {
            // Arrange
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameInfo>>(It.IsAny<string>()))
                .Throws(new ApiException {StatusCode = HttpStatusCode.InternalServerError});

            // Act
            _gameLibraryListViewModel.SearchCommand.Execute().Catch(Observable.Return(Enumerable.Empty<GameInfo>())).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameLibraryListViewModel.IsError, "vm.IsError should be true if an API exception is thrown.");
            Assert.AreEqual(Messages.GameLibraryListPageEmptyServerError, _gameLibraryListViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void SearchCommand_ThrowsGenericException_SetsErrorMessageAsGenericError()
        {
            // Arrange
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameInfo>>(It.IsAny<string>()))
                .Throws(new Exception());

            // Act
            _gameLibraryListViewModel.SearchCommand.Execute().Catch(Observable.Return(Enumerable.Empty<GameInfo>())).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameLibraryListViewModel.IsError, "vm.IsError should be true if a generic exception is thrown.");
            Assert.AreEqual(Messages.GameLibraryListPageEmptyGenericError, _gameLibraryListViewModel.ErrorMessage,
                "The error message is incorrect.");
        }

        [Test]
        public void SearchCommand_WithEmptyResult_SetIsEmptyToTrue()
        {
            // Arrange
            _gameLibraryListViewModel.SearchQuery = "search";
            
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameInfo>>(It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameInfo>());
            
            // Act
            _gameLibraryListViewModel.SearchCommand.Execute().Subscribe();
            _scheduler.Start();
            
            // Assert
            Assert.IsTrue(_gameLibraryListViewModel.IsEmpty, "_gameLibraryListViewModel.IsEmpty should be true if the API returns no results.");
            Assert.AreEqual(0, _gameLibraryListViewModel.Items.Count, "There should be no items in the list for an empty result.");
        }
        
        [Test]
        public void SearchCommand_WithResults_SetIsEmptyToFalseAndConvertsToItems()
        {
            // Arrange
            _gameLibraryListViewModel.SearchQuery = "search";
            
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameInfo>>(It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameInfo>
                {
                    Embedded = new HateoasResources<GameInfo>
                    {
                        Data = new []
                        {
                            new GameInfo
                            {
                                Title = "test-title",
                                Platforms = new SortedSet<Platform>
                                {
                                    new Platform()
                                },
                                Publishers = new SortedSet<Publisher>
                                {
                                    new Publisher()
                                },
                                ReleaseDate = DateTime.Now,
                                Genres = new SortedSet<Genre>()
                                {
                                    new Genre()
                                }
                            }
                        }
                    }
                });
            
            // Act
            _gameLibraryListViewModel.SearchCommand.Execute().Subscribe();
            _scheduler.Start();
            
            // Assert
            Assert.IsFalse(_gameLibraryListViewModel.IsEmpty, "_gameLibraryListViewModel.IsEmpty should be false if the API returns results.");
            Assert.AreEqual(1, _gameLibraryListViewModel.Items.Count, "There should be items in the list.");
        }
        
        [Test]
        public void LoadMoreCommand_ThrowsApiException_CallsUserDialog()
        {
            // Arrange
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameInfo>>(It.IsAny<string>()))
                .Throws(new ApiException {StatusCode = HttpStatusCode.InternalServerError});

            _userDialogs.Setup(mock => mock.Toast(It.IsAny<ToastConfig>()))
                .Verifiable();
            
            // Act
            _gameLibraryListViewModel.LoadMoreCommand.Execute().Catch(Observable.Return(Enumerable.Empty<GameInfo>())).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameLibraryListViewModel.IsError, "vm.IsError should be true if an API exception is thrown.");
            _userDialogs.Verify();
        }

        [Test]
        public void LoadMoreCommand_ThrowsGenericException_CallsUserDialog()
        {
            // Arrange
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameInfo>>(It.IsAny<string>()))
                .Throws(new Exception());

            _userDialogs.Setup(mock => mock.Toast(It.IsAny<ToastConfig>()))
                .Verifiable();
            
            // Act
            _gameLibraryListViewModel.LoadMoreCommand.Execute().Catch(Observable.Return(Enumerable.Empty<GameInfo>())).Subscribe();
            _scheduler.Start();

            // Assert
            Assert.IsTrue(_gameLibraryListViewModel.IsError, "vm.IsError should be true if a generic exception is thrown.");
            _userDialogs.Verify();
        }
        
        [Test]
        public void LoadMoreCommand_WithEmptyResult_DoesntAddNewItems()
        {
            // Arrange
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameInfo>>(It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameInfo>());
            
            // Act
            _gameLibraryListViewModel.LoadMoreCommand.Execute().Subscribe();
            _scheduler.Start();
            
            // Assert
            Assert.AreEqual(0, _gameLibraryListViewModel.Items.Count, "New items shouldn't be added if the result for loading more is empty.");
        }
        
        [Test]
        public void LoadMoreCommand_WithResults_SetIsEmptyToFalseAndConvertsToItems()
        {
            // Arrange
            _restService
                .Setup(mock => mock.GetAsync<HateoasPage<GameInfo>>(It.IsAny<string>()))
                .ReturnsAsync(new HateoasPage<GameInfo>
                {
                    Embedded = new HateoasResources<GameInfo>
                    {
                        Data = new []
                        {
                            new GameInfo
                            {
                                Title = "test-title",
                                Platforms = new SortedSet<Platform>
                                {
                                    new Platform()
                                },
                                Publishers = new SortedSet<Publisher>
                                {
                                    new Publisher()
                                },
                                ReleaseDate = DateTime.Now,
                                Genres = new SortedSet<Genre>
                                {
                                    new Genre()
                                }
                            }
                        }
                    }
                });
            
            // Act
            _gameLibraryListViewModel.LoadMoreCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            Assert.AreEqual(1, _gameLibraryListViewModel.Items.Count, "There should be items in the list.");
        }
        
        [Test]
        public void RequestCommand_WithNoData_DoesntThrowException()
        {
            _navigationService.Setup(mock => mock.NavigateAsync(It.IsAny<string>()))
                .Verifiable();
            
            Assert.DoesNotThrow(() =>
            {
                // Act
                _gameLibraryListViewModel.RequestCommand.Execute().Subscribe();
                _scheduler.Start();
            });    
            
            // Assert
            _navigationService.Verify();
        }
    }
}