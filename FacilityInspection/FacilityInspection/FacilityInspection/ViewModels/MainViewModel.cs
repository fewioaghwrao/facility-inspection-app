using CommunityToolkit.Mvvm.ComponentModel;
using FacilityInspection.Data;
using FacilityInspection.Domain.Operators;
using FacilityInspection.Services.Authentication;
using System;

namespace FacilityInspection.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly CurrentUserSession _currentUserSession;
    private readonly InspectionTemplateRepository
    _inspectionTemplateRepository;

    [ObservableProperty]
    private ViewModelBase currentPage = null!;

    public MainViewModel(
       IAuthenticationService authenticationService,
       CurrentUserSession currentUserSession,
       InspectionTemplateRepository inspectionTemplateRepository)
    {
        ArgumentNullException.ThrowIfNull(authenticationService);
        ArgumentNullException.ThrowIfNull(currentUserSession);
        ArgumentNullException.ThrowIfNull(
            inspectionTemplateRepository);

        _authenticationService = authenticationService;
        _currentUserSession = currentUserSession;
        _inspectionTemplateRepository =
            inspectionTemplateRepository;

        CurrentPage = CreateLoginViewModel();
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
        ArgumentNullException.ThrowIfNull(signedInOperator);

        _currentUserSession.SignIn(
            signedInOperator);

        ViewModelBase destination =
            signedInOperator.Role switch
            {
                OperatorRole.Inspector =>
                    CreateMemberShellViewModel(
                        signedInOperator),

                OperatorRole.MaintenanceManager =>
                    CreateAdminShellViewModel(
                        signedInOperator),

                _ => throw new InvalidOperationException(
                    $"未対応の権限です: {signedInOperator.Role}")
            };

        NavigateTo(destination);
    }

    private MemberShellViewModel CreateMemberShellViewModel(
        SignedInOperator signedInOperator)
    {
        return new MemberShellViewModel(
            signedInOperator.DisplayName,
            Logout);
    }

    private AdminShellViewModel CreateAdminShellViewModel(
        SignedInOperator signedInOperator)
    {
        var adminDashboardViewModel =
            new AdminDashboardViewModel(
                signedInOperator.DisplayName);

        var equipmentManagementViewModel =
            new EquipmentManagementViewModel();

        var scheduleCalendarViewModel =
            new ScheduleCalendarViewModel();

        var inspectionTemplateManagementViewModel =
            new InspectionTemplateManagementViewModel(
                _inspectionTemplateRepository);

        return new AdminShellViewModel(
            signedInOperator.DisplayName,
            adminDashboardViewModel,
            equipmentManagementViewModel,
            scheduleCalendarViewModel,
            inspectionTemplateManagementViewModel,
            Logout);
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