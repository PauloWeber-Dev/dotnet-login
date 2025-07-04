using Login.Frontend.ViewModel;

namespace Login.Frontend.Views;

public partial class MfaPage : ContentPage
{
    public MfaPage(MfaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}