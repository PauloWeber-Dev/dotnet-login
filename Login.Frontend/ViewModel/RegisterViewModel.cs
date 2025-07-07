using BCrypt.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO.Auth;
using Login.Frontend.Services;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;


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
        private string confirmPassword = string.Empty;
        
        [ObservableProperty]
        public string errorMessage = string.Empty;

        public ObservableCollection<string> GenderOptions { get; } = new ObservableCollection<string>
    {
        "Masculino",
        "Feminino",
        "Outros/Prefiro não informar"
    };

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
                ErrorMessage = string.Empty;

                // Validar email
                if (!IsValidEmail(Email))
                {
                    ErrorMessage = "Por favor, insira um email válido.";
                    return;
                }

                // Validar senhas
                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "As senhas não coincidem.";
                    return;
                }

                // Validar se Gender foi selecionado
                if (string.IsNullOrEmpty(Gender))
                {
                    ErrorMessage = "Por favor, selecione um gênero.";
                    return;
                }

                // Hashear a senha antes de enviar
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(Password);
                var dto = new RegisterUserDto(FirstName, LastName, BirthDate, Gender, Email,BCrypt.Net.BCrypt.HashPassword(Password), BCrypt.Net.BCrypt.HashPassword(ConfirmPassword));
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

        [RelayCommand]
        private async Task GoToTermsAsync()
        {
            await Shell.Current.GoToAsync("TermsOfUsePage");
        }

        [RelayCommand]
        private async Task GoToPrivacyPolicyAsync()
        {
            await Shell.Current.GoToAsync("PrivacyPolicyPage");
        }
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Expressão regular para validar email
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
    }
}
