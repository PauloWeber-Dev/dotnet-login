using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO.Auth;
using Login.Frontend.Services;

namespace Login.Frontend.ViewModel
{
    public partial class ForgotPasswordViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        public string email = string.Empty;

        [ObservableProperty]
        public string token = string.Empty;

        [ObservableProperty]
        public string newPassword = string.Empty;

        [ObservableProperty]
        public string errorMessage = string.Empty;

        public ForgotPasswordViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task RequestPasswordResetAsync()
        {
            try
            {
                ErrorMessage = string.Empty;
                await _authService.RequestPasswordResetAsync(Email);
                await Shell.Current.DisplayAlert("Success", "Password reset email sent!", "OK");
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task ResetPasswordAsync()
        {
            try
            {
                ErrorMessage = string.Empty;
                await _authService.ResetPasswordAsync(Token, NewPassword);
                await Shell.Current.DisplayAlert("Success", "Password reset successfully!", "OK");
                await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
