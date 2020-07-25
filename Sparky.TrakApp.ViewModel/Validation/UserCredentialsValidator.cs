﻿using FluentValidation;
using Sparky.TrakApp.Model.Login;
using Sparky.TrakApp.ViewModel.Resources;

namespace Sparky.TrakApp.ViewModel.Validation
{
    /// <summary>
    /// The <see cref="UserCredentialsValidator"/> is a validation class that validates the properties
    /// within the <see cref="UserCredentials"/> class to ensure that they can valid information before
    /// being passed onto an API call.
    /// </summary>
    public class UserCredentialsValidator : AbstractValidator<UserCredentials>
    {
        public UserCredentialsValidator()
        {
            RuleFor(r => r.Username)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage(Messages.LoginErrorMessageUsernameEmpty);

            RuleFor(r => r.Password)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage(Messages.LoginErrorMessagePasswordEmpty);
        }
    }
}