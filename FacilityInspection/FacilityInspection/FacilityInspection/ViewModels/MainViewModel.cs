using CommunityToolkit.Mvvm.ComponentModel;
using FacilityInspection.Domain.Operators;
using FacilityInspection.Services.Authentication;
using System;

namespace FacilityInspection.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly CurrentUserSession _currentUserSession;

    [ObservableProperty]
    private ViewModelBase currentPage;

    public MainViewModel(
        IAuthenticationService authenticationService,
        CurrentUserSession currentUserSession)
    {
        _authenticationService = authenticationService;
        _currentUserSession = currentUserSession;

        currentPage = CreateLoginViewModel();
    }

    private LoginViewModel CreateLoginViewModel()
    {
        var loginViewModel =
            new LoginViewModel(_authenticationService);

        loginViewModel.LoginSucceeded =
            OnLoginSucceeded;

        return loginViewModel;
    }

    private void OnLoginSucceeded(
        SignedInOperator signedInOperator)
    {
        _currentUserSession.SignIn(
            signedInOperator);

        ViewModelBase destination =
            signedInOperator.Role switch
            {
                OperatorRole.Inspector =>
                    new EquipmentManagementViewModel(),

                OperatorRole.MaintenanceManager =>
                    new AdminDashboardViewModel(
                        signedInOperator.DisplayName,
                        Logout),

                _ => throw new InvalidOperationException(
                    $"未対応の権限です: {signedInOperator.Role}")
            };

        NavigateTo(destination);
    }

    public void NavigateTo(
        ViewModelBase destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        CurrentPage = destination;
    }

    public void Logout()
    {
        _currentUserSession.SignOut();

        NavigateTo(
            CreateLoginViewModel());
    }
}