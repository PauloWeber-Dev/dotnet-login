using Login.Frontend.ViewModel;

namespace Login.Frontend.Views;

public partial class ConfirmEmailPage : ContentPage
{
    public ConfirmEmailPage(ConfirmEmailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}