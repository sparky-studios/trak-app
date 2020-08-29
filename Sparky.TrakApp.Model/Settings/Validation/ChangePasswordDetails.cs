﻿using System.Diagnostics.CodeAnalysis;

namespace Sparky.TrakApp.Model.Settings.Validation
{
    [ExcludeFromCodeCoverage]
    public class ChangePasswordDetails
    {
        public string RecoveryToken { get; set; }
        
        public string NewPassword { get; set; }
        
        public string ConfirmNewPassword { get; set; }
    }
}