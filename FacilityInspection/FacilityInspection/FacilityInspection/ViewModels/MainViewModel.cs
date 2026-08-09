using CommunityToolkit.Mvvm.ComponentModel;
using FacilityInspection.Data;
using FacilityInspection.Domain.Operators;
using FacilityInspection.Services.Authentication;
using System;

namespace FacilityInspection.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IAuthenticationService
        _authenticationService;

    private readonly CurrentUserSession
        _currentUserSession;

    private readonly InspectionTemplateRepository
        _inspectionTemplateRepository;

    private readonly OperatorRepository
        _operatorRepository;

    private readonly ScheduleRepository
        _scheduleRepository;

    private readonly InspectionRepository
    _inspectionRepository;

    [ObservableProperty]
    private ViewModelBase currentPage = null!;

    public MainViewModel(
        IAuthenticationService authenticationService,
        CurrentUserSession currentUserSession,
        InspectionTemplateRepository
            inspectionTemplateRepository,
        OperatorRepository operatorRepository,
        ScheduleRepository scheduleRepository,
        InspectionRepository inspectionRepository)
    {
        ArgumentNullException.ThrowIfNull(
            authenticationService);

        ArgumentNullException.ThrowIfNull(
            currentUserSession);

        ArgumentNullException.ThrowIfNull(
            inspectionTemplateRepository);

        ArgumentNullException.ThrowIfNull(
            operatorRepository);

        ArgumentNullException.ThrowIfNull(
            scheduleRepository);

        ArgumentNullException.ThrowIfNull(
    inspectionRepository);
        _authenticationService =
            authenticationService;

        _currentUserSession =
            currentUserSession;

        _inspectionTemplateRepository =
            inspectionTemplateRepository;

        _operatorRepository =
            operatorRepository;

        _scheduleRepository =
            scheduleRepository;

        _inspectionRepository =
    inspectionRepository;

        CurrentPage = CreateLoginViewModel();
    }

    private LoginViewModel CreateLoginViewModel()
    {
        var loginViewModel =
            new LoginViewModel(
                _authenticationService);

        loginViewModel.LoginSucceeded =
            OnLoginSucceeded;

        return loginViewModel;
    }

    private void OnLoginSucceeded(
        SignedInOperator signedInOperator)
    {
        ArgumentNullException.ThrowIfNull(
            signedInOperator);

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
                    $"未対応の権限です: " +
                    $"{signedInOperator.Role}")
            };

        NavigateTo(destination);
    }

    private MemberShellViewModel
        CreateMemberShellViewModel(
            SignedInOperator signedInOperator)
    {
        return new MemberShellViewModel(
            signedInOperator.DisplayName,
            _scheduleRepository,
            Logout);
    }

    private AdminShellViewModel
        CreateAdminShellViewModel(
            SignedInOperator signedInOperator)
    {
        var adminDashboardViewModel =
            new AdminDashboardViewModel(
                signedInOperator.DisplayName);

        var equipmentManagementViewModel =
            new EquipmentManagementViewModel();

        var scheduleCalendarViewModel =
            new ScheduleCalendarViewModel(
                _scheduleRepository);

        var inspectionTemplateManagementViewModel =
            new InspectionTemplateManagementViewModel(
                _inspectionTemplateRepository);

        var operatorManagementViewModel =
            new OperatorManagementViewModel(
                _operatorRepository,
                signedInOperator.Id);

        var inspectionStatusViewModel =
    new InspectionStatusViewModel(
        _inspectionRepository);

        return new AdminShellViewModel(
            signedInOperator.DisplayName,
            adminDashboardViewModel,
            equipmentManagementViewModel,
            scheduleCalendarViewModel,
            inspectionStatusViewModel,
            inspectionTemplateManagementViewModel,
            operatorManagementViewModel,
            Logout);
    }

    public void NavigateTo(
        ViewModelBase destination)
    {
        ArgumentNullException.ThrowIfNull(
            destination);

        CurrentPage = destination;
    }

    public void Logout()
    {
        _currentUserSession.SignOut();

        NavigateTo(
            CreateLoginViewModel());
    }
}
