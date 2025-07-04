using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO.Auth;
using Login.Frontend.Services;

namespace Login.Frontend.ViewModel
{
    public partial class ConfirmEmailViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        public string email = string.Empty;

        [ObservableProperty]
        public string code = string.Empty;

        [ObservableProperty]
        public string errorMessage = string.Empty;

        public ConfirmEmailViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task ConfirmEmailAsync()
        {
            try
            {
                ErrorMessage = string.Empty;
                await _authService.ConfirmEmailAsync(new ConfirmEmailDto(Email, Code));
                await Shell.Current.DisplayAlert("Success", "Email confirmed successfully!", "OK");
                await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
