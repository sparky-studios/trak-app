﻿using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using SparkyStudios.TrakLibrary.Model.Response;
using SparkyStudios.TrakLibrary.Model.Settings;
using SparkyStudios.TrakLibrary.Service;
using SparkyStudios.TrakLibrary.Service.Exception;
using SparkyStudios.TrakLibrary.ViewModel.Resources;
using SparkyStudios.TrakLibrary.ViewModel.Settings;

namespace SparkyStudios.TrakLibrary.ViewModel.Test.Settings
{
    [TestFixture]
    public class ChangePasswordViewModelTest
    {
        private Mock<INavigationService> _navigationService;
        private Mock<IStorageService> _storageService;
        private Mock<IAuthService> _authService;
        private Mock<IUserDialogs> _userDialogs;
        private TestScheduler _scheduler;

        private ChangePasswordViewModel _changePasswordViewModel;

        [SetUp]
        public void SetUp()
        {
            _navigationService = new Mock<INavigationService>();
            _storageService = new Mock<IStorageService>();
            _authService = new Mock<IAuthService>();
            _userDialogs = new Mock<IUserDialogs>();
            _scheduler = new TestScheduler();

            _changePasswordViewModel = new ChangePasswordViewModel(_scheduler, _navigationService.Object,
                _storageService.Object, _authService.Object, _userDialogs.Object);
        }

        [Test]
        public void ClearValidationCommand_WithNoData_DoesntThrowException()
        {
            // Assert
            Assert.DoesNotThrow(() =>
            {
                _changePasswordViewModel.ClearValidationCommand.Execute().Subscribe();
                _scheduler.Start();
            });
        }

        [Test]
        public void ChangeCommand_WithInvalidData_DoesntSendChangePasswordRequest()
        {
            // Act
            _changePasswordViewModel.ChangeCommand.Execute().Subscribe();
            _scheduler.Start();

            // Assert
            _authService.Verify(a => a.ChangePasswordAsync(It.IsAny<long>(), It.IsAny<ChangePasswordRequest>()), Times.Never);
        }

        [Test]
        public void ChangeCommand_ThrowsTaskCanceledException_SetsErrorMessageAsNoInternet()
        {
            // Arrange
            _changePasswordViewModel.CurrentPassword.Value = string.Concat(Enumerable.Repeat("a", 30));
            _changePasswordViewModel.NewPassword.Value = "Password123";
            _changePasswordViewModel.ConfirmNewPassword.Value = "Password123";

            _storageService.Setup(m => m.GetUserIdAsync())
                .ReturnsAsync(1L);

            _authService.Setup(m => m.ChangePasswordAsync(It.IsAny<long>(), It.IsAny<ChangePasswordRequest>()))
                .Throws(new TaskCanceledException());

            // Act
            _changePasswordViewModel.ChangeCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();
            
            // Assert
            Assert.IsTrue(_changePasswordViewModel.IsError,
                "_changePasswordViewModel.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageNoInternet, _changePasswordViewModel.ErrorMessage,
                "The error message is incorrect.");
            
            _navigationService.Verify(m => m.NavigateAsync(It.IsAny<string>()), Times.Never);
        }
        
        [Test]
        public void ChangeCommand_ThrowsApiException_SetsErrorMessageAsApiError()
        {
            // Arrange
            _changePasswordViewModel.CurrentPassword.Value = string.Concat(Enumerable.Repeat("a", 30));
            _changePasswordViewModel.NewPassword.Value = "Password123";
            _changePasswordViewModel.ConfirmNewPassword.Value = "Password123";

            _storageService.Setup(m => m.GetUserIdAsync())
                .ReturnsAsync(1L);

            _authService.Setup(m => m.ChangePasswordAsync(It.IsAny<long>(), It.IsAny<ChangePasswordRequest>()))
                .Throws(new ApiException());

            // Act
            _changePasswordViewModel.ChangeCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();
            
            // Assert
            Assert.IsTrue(_changePasswordViewModel.IsError,
                "_changePasswordViewModel.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageApiError, _changePasswordViewModel.ErrorMessage,
                "The error message is incorrect.");
            
            _navigationService.Verify(m => m.NavigateAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ChangeCommand_ThrowsException_SetsErrorMessageAsApiGeneric()
        {
            // Arrange
            _changePasswordViewModel.CurrentPassword.Value = string.Concat(Enumerable.Repeat("a", 30));
            _changePasswordViewModel.NewPassword.Value = "Password123";
            _changePasswordViewModel.ConfirmNewPassword.Value = "Password123";

            _storageService.Setup(m => m.GetUserIdAsync())
                .ReturnsAsync(1L);

            _authService.Setup(m => m.ChangePasswordAsync(It.IsAny<long>(), It.IsAny<ChangePasswordRequest>()))
                .Throws(new Exception());

            // Act
            _changePasswordViewModel.ChangeCommand.Execute().Catch(Observable.Return(Unit.Default)).Subscribe();
            _scheduler.Start();
            
            // Assert
            Assert.IsTrue(_changePasswordViewModel.IsError,
                "_changePasswordViewModel.IsError should be true if an exception is thrown.");
            Assert.AreEqual(Messages.ErrorMessageGeneric, _changePasswordViewModel.ErrorMessage,
                "The error message is incorrect.");
            
            _navigationService.Verify(m => m.NavigateAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ChangeCommand_WithCheckedResponseError_SetsErrorMessage()
        {
            // Arrange
            _changePasswordViewModel.CurrentPassword.Value = string.Concat(Enumerable.Repeat("a", 30));
            _changePasswordViewModel.NewPassword.Value = "Password123";
            _changePasswordViewModel.ConfirmNewPassword.Value = "Password123";

            _storageService.Setup(m => m.GetUserIdAsync())
                .ReturnsAsync(1L);

            var response = new CheckedResponse<bool>
            {
                Error = true,
                ErrorMessage = "error-message"
            };
            
            _authService.Setup(m => m.ChangePasswordAsync(It.IsAny<long>(), It.IsAny<ChangePasswordRequest>()))
                .ReturnsAsync(response);
            
            // Act
            _changePasswordViewModel.ChangeCommand.Execute();
            _scheduler.Start();
            
            // Assert
            Assert.IsTrue(_changePasswordViewModel.IsError,
                "_changePasswordViewModel.IsError should be true if the response has an error.");
            Assert.AreEqual(response.ErrorMessage, _changePasswordViewModel.ErrorMessage,
                "The error message is incorrect.");
            
            _navigationService.Verify(m => m.NavigateAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ChangeCommand_WithSuccessfulCheckedResponse_LogsOutAndNavigatesToLoginPage()
        {
            // Arrange
            _changePasswordViewModel.CurrentPassword.Value = string.Concat(Enumerable.Repeat("a", 30));
            _changePasswordViewModel.NewPassword.Value = "Password123";
            _changePasswordViewModel.ConfirmNewPassword.Value = "Password123";

            _storageService.Setup(m => m.GetUserIdAsync())
                .ReturnsAsync(1L);
            
            _authService.Setup(m => m.ChangePasswordAsync(It.IsAny<long>(), It.IsAny<ChangePasswordRequest>()))
                .ReturnsAsync(new CheckedResponse<bool>
                {
                    Error = false
                });
            
            _navigationService.Setup(m => m.NavigateAsync("HomePage"))
                .ReturnsAsync(new NavigationResult());

            _userDialogs.Setup(m => m.AlertAsync(It.IsAny<AlertConfig>(), null))
                .Verifiable();
            
            // Act
            _changePasswordViewModel.ChangeCommand.Execute();
            _scheduler.Start();
            
            // Assert
            Assert.IsFalse(_changePasswordViewModel.IsError, "vm.IsError should be false if login was successful.");
            
            _storageService.Verify();
            _userDialogs.Verify();
        }
    }
}