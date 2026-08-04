using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Services.Authentication;
using System;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;

    public LoginViewModel(
        IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public Action<SignedInOperator>? LoginSucceeded { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string loginId = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string password = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PasswordToggleLabel))]
    private bool isPasswordVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private bool isBusy;

    public string PasswordToggleLabel =>
        IsPasswordVisible ? "隠す" : "表示";

    public bool HasError =>
        !string.IsNullOrWhiteSpace(ErrorMessage);

    private bool CanLogin()
    {
        return !IsBusy
            && !string.IsNullOrWhiteSpace(LoginId)
            && !string.IsNullOrWhiteSpace(Password);
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;
        IsBusy = true;

        try
        {
            var result =
                await _authenticationService.SignInAsync(
                    LoginId,
                    Password);

            if (!result.Succeeded || result.User is null)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            Password = string.Empty;
            IsPasswordVisible = false;

            LoginSucceeded?.Invoke(result.User);
        }
        catch (Exception)
        {
            ErrorMessage =
                "ログイン処理中にエラーが発生しました。";
        }
        finally
        {
            IsBusy = false;
        }
    }
}