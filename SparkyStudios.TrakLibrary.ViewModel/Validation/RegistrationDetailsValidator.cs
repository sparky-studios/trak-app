﻿using System.Linq;
using FluentValidation;
using SparkyStudios.TrakLibrary.Model.Login;
using SparkyStudios.TrakLibrary.Model.Login.Validation;
using SparkyStudios.TrakLibrary.ViewModel.Resources;

namespace SparkyStudios.TrakLibrary.ViewModel.Validation
{
    /// <summary>
    /// The <see cref="RegistrationDetailsValidator"/> is a validation class that validates the properties
    /// within the <see cref="RegistrationDetails"/> class to ensure that they can valid information before
    /// being passed onto an API call.
    /// </summary>
    public class RegistrationDetailsValidator : AbstractValidator<RegistrationDetails>
    {
        public RegistrationDetailsValidator()
        {
            RuleFor(r => r.Username)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(Messages.RegistrationErrorMessageUsernameEmpty)
                .Must(p => !p.Any(char.IsWhiteSpace))
                .WithMessage(Messages.RegistrationErrorMessageUsernameWhitespace)
                .Length(1, 255)
                .WithMessage(Messages.RegistrationErrorMessageUsernameLength)
                .Matches(@"^\w+$")
                .WithMessage(Messages.RegistrationErrorMessageUsernameInvalidCharacters);

            RuleFor(r => r.EmailAddress)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(Messages.RegistrationErrorMessageEmailAddressEmpty)
                .EmailAddress()
                .WithMessage(Messages.RegistrationErrorMessageEmailAddressInvalid);

            RuleFor(r => r.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(Messages.RegistrationErrorMessagePasswordEmpty)
                .MinimumLength(8)
                .WithMessage(Messages.RegistrationErrorMessagePasswordMinimumLength)
                .Matches("[A-Z]")
                .WithMessage(Messages.RegistrationErrorMessagePasswordUppercaseCharacter)
                .Matches("[a-z]")
                .WithMessage(Messages.RegistrationErrorMessagePasswordLowercaseCharacter)
                .Matches("[0-9]")
                .WithMessage(Messages.RegistrationErrorMessagePasswordNumber)
                .Must(p => !p.Any(char.IsWhiteSpace))
                .WithMessage(Messages.RegistrationErrorMessagePasswordWhitespace);

            RuleFor(r => r.ConfirmPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(Messages.RegistrationErrorMessageConfirmPasswordEmpty)
                .Must((model, field) => string.Equals(model.Password, field))
                .WithMessage(Messages.RegistrationErrorMessageConfirmPasswordMismatch);
        }
    }
}