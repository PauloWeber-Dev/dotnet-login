using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Login.Frontend.Services;
using QRCoder;

namespace Login.Frontend.ViewModel
{
    public partial class MfaViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        public ImageSource qrCodeImage;

        [ObservableProperty]
        public string errorMessage = string.Empty;

        public MfaViewModel(IAuthService authService)
        {
            _authService = authService;
            QrCodeImage = ImageSource.FromStream(() => new MemoryStream());
        }

        [RelayCommand]
        private async Task EnableMfaAsync()
        {
            try
            {
                ErrorMessage = string.Empty;
                // Substitua '1' pelo ID do usuário obtido do JWT
                var qrCodeUrl = await _authService.EnableMfaAsync(1);
                var qrCode = GenerateQrCode(qrCodeUrl);
                QrCodeImage = ImageSource.FromStream(() => new MemoryStream(qrCode));
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private byte[] GenerateQrCode(string url)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new BitmapByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }
    }
}
