﻿using System.Linq;
using FluentValidation;
using SparkyStudios.TrakLibrary.Model.Login;
using SparkyStudios.TrakLibrary.Model.Login.Validation;
using SparkyStudios.TrakLibrary.ViewModel.Resources;

namespace SparkyStudios.TrakLibrary.ViewModel.Validation
{
    /// <summary>
    /// The <see cref="RecoveryDetailsValidator"/> is a validation class that validates the properties
    /// within the <see cref="RecoveryDetails"/> class to ensure that they can valid information before
    /// being passed onto an API call.
    /// </summary>
    public class RecoveryDetailsValidator : AbstractValidator<RecoveryDetails>
    {
        public RecoveryDetailsValidator()
        {
            RuleFor(r => r.Username)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(Messages.RecoveryErrorMessageUsernameEmpty)
                .Must(p => !p.Any(char.IsWhiteSpace))
                .WithMessage(Messages.RecoveryErrorMessageUsernameWhitespace)
                .Length(1, 255)
                .WithMessage(Messages.RecoveryErrorMessageUsernameLength)
                .Matches(@"^\w+$")
                .WithMessage(Messages.RecoveryErrorMessageUsernameInvalidCharacters);

            RuleFor(r => r.RecoveryToken)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(Messages.RecoveryErrorMessageRecoveryTokenEmpty)
                .Must(p => !p.Any(char.IsWhiteSpace))
                .WithMessage(Messages.RecoveryErrorMessageRecoveryTokenWhitespace)
                .Length(30)
                .WithMessage(Messages.RecoveryErrorMessageRecoveryTokenLength)
                .Matches("^[a-zA-Z][a-zA-Z0-9]*$")
                .WithMessage(Messages.RecoveryErrorMessageRecoveryTokenAlphanumeric);
            
            RuleFor(r => r.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(Messages.RecoveryErrorMessagePasswordEmpty)
                .MinimumLength(8)
                .WithMessage(Messages.RecoveryErrorMessagePasswordMinimumLength)
                .Matches("[A-Z]")
                .WithMessage(Messages.RecoveryErrorMessagePasswordUppercaseCharacter)
                .Matches("[a-z]")
                .WithMessage(Messages.RecoveryErrorMessagePasswordLowercaseCharacter)
                .Matches("[0-9]")
                .WithMessage(Messages.RecoveryErrorMessagePasswordNumber)
                .Must(p => !p.Any(char.IsWhiteSpace))
                .WithMessage(Messages.RecoveryErrorMessagePasswordWhitespace);

            RuleFor(r => r.ConfirmPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(Messages.RecoveryErrorMessageConfirmPasswordEmpty)
                .Must((model, field) => string.Equals(model.Password, field))
                .WithMessage(Messages.RecoveryErrorMessageConfirmPasswordMismatch);
        }
    }
}