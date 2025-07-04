using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Login.Frontend.Services;
using DTO.Auth;



namespace Login.Frontend.ViewModel
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string mfaCode = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool isMfaRequired;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            try
            {
                ErrorMessage = string.Empty;
                var token = await _authService.LoginAsync(new LoginDto(Email, Password));
                await Shell.Current.DisplayAlert("Success", "Logged in successfully!", "OK");
                // Navegar para página principal ou salvar token
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("MFA required"))
                {
                    IsMfaRequired = true;
                }
                else
                {
                    ErrorMessage = ex.Message;
                }
            }
        }

        [RelayCommand]
        private async Task VerifyMfaAsync()
        {
            try
            {
                ErrorMessage = string.Empty;
                var token = await _authService.VerifyMfaAsync(new VerifyMfaDto(Email, MfaCode));
                await Shell.Current.DisplayAlert("Success", "MFA verified! Logged in successfully!", "OK");
                // Navegar para página principal ou salvar token
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task GoogleLoginAsync()
        {
            // Implementar login com Google (necessita WebAuthenticator)
            await Shell.Current.DisplayAlert("Info", "Google login not implemented yet.", "OK");
        }

        [RelayCommand]
        private async Task GoToRegisterAsync()
        {
            await Shell.Current.GoToAsync("//RegisterPage");
        }

        [RelayCommand]
        private async Task ForgotPasswordAsync()
        {
            await Shell.Current.GoToAsync("//ForgotPasswordPage");
        }
    }
}
