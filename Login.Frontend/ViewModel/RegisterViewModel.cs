using BCrypt.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO.Auth;
using Login.Frontend.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Login.Frontend.ViewModel
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        public string firstName = string.Empty;

        [ObservableProperty]
        public string lastName = string.Empty;

        [ObservableProperty]
        public DateTime birthDate = DateTime.Today;

        [ObservableProperty]
        public string gender = string.Empty;

        [ObservableProperty]
        public string email = string.Empty;

        [ObservableProperty]
        public string password = string.Empty;

        [ObservableProperty]
        public string errorMessage = string.Empty;

        public RegisterViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            try
            {
                ErrorMessage = string.Empty;
                var dto = new RegisterUserDto(FirstName, LastName, BirthDate, Gender, Email,BCrypt.Net.BCrypt.HashPassword(Password));
                var message = await _authService.RegisterAsync(dto);
                await Shell.Current.DisplayAlert("Success", message, "OK");
                await Shell.Current.GoToAsync("//ConfirmEmailPage");
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task GoToLoginAsync()
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
