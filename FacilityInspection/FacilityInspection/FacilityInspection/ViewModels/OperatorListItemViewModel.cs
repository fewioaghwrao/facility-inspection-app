using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Domain.Operators;
using System;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public sealed partial class OperatorListItemViewModel
    : ObservableObject
{
    private readonly
        Action<OperatorListItemViewModel>
        _editRequested;

    private readonly
        Func<OperatorListItemViewModel, Task>
        _toggleActiveRequested;


    // ============================================
    // Constructor
    // ============================================

    public OperatorListItemViewModel(
        Guid id,
        string loginId,
        string displayName,
        OperatorRole role,
        bool isActive,
        DateTimeOffset? lastLoginAt,
        bool isCurrentUser,
        Action<OperatorListItemViewModel>
            editRequested,
        Func<OperatorListItemViewModel, Task>
            toggleActiveRequested)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            loginId);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            displayName);

        ArgumentNullException.ThrowIfNull(
            editRequested);

        ArgumentNullException.ThrowIfNull(
            toggleActiveRequested);


        Id =
            id;

        LoginId =
            loginId;

        DisplayName =
            displayName;

        Role =
            role;

        IsActive =
            isActive;

        LastLoginAt =
            lastLoginAt;

        IsCurrentUser =
            isCurrentUser;


        _editRequested =
            editRequested;

        _toggleActiveRequested =
            toggleActiveRequested;
    }


    // ============================================
    // Data
    // ============================================

    public Guid Id
    {
        get;
    }


    public string LoginId
    {
        get;
    }


    public string DisplayName
    {
        get;
    }


    public OperatorRole Role
    {
        get;
    }


    public bool IsActive
    {
        get;
    }


    public DateTimeOffset? LastLoginAt
    {
        get;
    }


    public bool IsCurrentUser
    {
        get;
    }


    // ============================================
    // Active
    // ============================================

    public bool CanToggleActive =>
        !IsCurrentUser;


    // ============================================
    // Role
    // ============================================

    public string RoleName =>
        Role switch
        {
            OperatorRole.Inspector =>
                "点検担当者",

            OperatorRole.MaintenanceManager =>
                "保全責任者",

            _ =>
                Role.ToString()
        };


    // ============================================
    // Status
    // ============================================

    public string StatusText =>
        IsActive
            ? "有効"
            : "無効";


    // ============================================
    // Last Login
    // ============================================

    public string LastLoginAtText =>
        LastLoginAt.HasValue
            ? LastLoginAt.Value
                .ToLocalTime()
                .ToString(
                    "yyyy/MM/dd HH:mm")
            : "未ログイン";


    // ============================================
    // Toggle Active
    // ============================================

    public string ToggleActiveText =>
        IsCurrentUser
            ? "ログイン中"
            : IsActive
                ? "無効化"
                : "有効化";


    // ============================================
    // Edit
    // ============================================

    [RelayCommand]
    private void Edit()
    {
        _editRequested(
            this);
    }


    // ============================================
    // Toggle Active
    // ============================================

    [RelayCommand(
        CanExecute =
            nameof(CanToggleActive))]
    private async Task ToggleActiveAsync()
    {
        await _toggleActiveRequested(
            this);
    }
}