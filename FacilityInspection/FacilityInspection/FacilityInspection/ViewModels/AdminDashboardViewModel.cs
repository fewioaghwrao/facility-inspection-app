using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public sealed partial class AdminDashboardViewModel
    : ViewModelBase
{
    private readonly InspectionRepository
        _inspectionRepository;


    // ============================================
    // Navigation
    // ============================================

    public Action? InspectionStatusRequested
    {
        get;
        set;
    }

    public Action? NotStartedRequested
    {
        get;
        set;
    }

    public Action? ApprovalPendingRequested
    {
        get;
        set;
    }

    public Action? AbnormalListRequested
    {
        get;
        set;
    }


    // ============================================
    // Constructor
    // ============================================

    public AdminDashboardViewModel(
        string displayName,
        InspectionRepository inspectionRepository)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            displayName);

        ArgumentNullException.ThrowIfNull(
            inspectionRepository);

        DisplayName =
            displayName;

        _inspectionRepository =
            inspectionRepository;

        _ = LoadAsync();
    }


    // ============================================
    // Header
    // ============================================

    public string DisplayName
    {
        get;
    }

    public string WelcomeMessage =>
        $"{DisplayName}さん、お疲れさまです。";

    public string CurrentDateText =>
        DateTime.Now.ToString(
            "yyyy年M月d日");


    // ============================================
    // State
    // ============================================

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasError))]
    private string? errorMessage;

    public bool HasError =>
        !string.IsNullOrWhiteSpace(
            ErrorMessage);


    // ============================================
    // Dashboard Counts
    // ============================================

    /// <summary>
    /// 本日の点検予定件数。
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(TodaySummaryText))]
    private int todayInspectionCount;


    /// <summary>
    /// 本日の完了件数。
    /// Completed / Approved を完了扱いとする。
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(TodaySummaryText))]
    private int todayCompletedCount;


    /// <summary>
    /// 本日の未実施件数。
    /// </summary>
    [ObservableProperty]
    private int todayNotStartedCount;


    /// <summary>
    /// 全体の承認待ち件数。
    /// </summary>
    [ObservableProperty]
    private int approvalPendingCount;


    /// <summary>
    /// 本日の異常項目件数。
    /// </summary>
    [ObservableProperty]
    private int todayAbnormalCount;


    /// <summary>
    /// 本日の点検完了率。
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(CompletionRateText))]
    private int completionRate;


    // ============================================
    // Calculated
    // ============================================

    public string CompletionRateText =>
        $"{CompletionRate}%";

    public string TodaySummaryText =>
        TodayInspectionCount == 0
            ? "本日の点検予定はありません。"
            : $"{TodayInspectionCount}件中 " +
              $"{TodayCompletedCount}件が完了しています。";


    // ============================================
    // Load
    // ============================================

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsLoading)
        {
            return;
        }

        try
        {
            IsLoading =
                true;

            ErrorMessage =
                null;

            var rows =
                await _inspectionRepository
                    .GetAllAsync();

            var today =
                DateOnly.FromDateTime(
                    DateTime.Now);

            var todayRows =
                rows
                    .Where(x =>
                        x.ScheduledDate ==
                        today)
                    .ToList();


            // ----------------------------------------
            // 本日の予定
            // ----------------------------------------

            TodayInspectionCount =
                todayRows.Count;


            // ----------------------------------------
            // 本日の完了
            //
            // Completed:
            // 点検完了・承認待ち
            //
            // Approved:
            // 承認済み
            // ----------------------------------------

            TodayCompletedCount =
                todayRows.Count(x =>
                    x.Status ==
                        InspectionStatus.Completed ||
                    x.Status ==
                        InspectionStatus.Approved);


            // ----------------------------------------
            // 本日未実施
            // ----------------------------------------

            TodayNotStartedCount =
                todayRows.Count(x =>
                    x.Status ==
                        InspectionStatus.NotStarted);


            // ----------------------------------------
            // 承認待ち
            //
            // 日付を限定せず、
            // 現在残っている承認待ちを表示。
            // ----------------------------------------

            ApprovalPendingCount =
                rows.Count(x =>
                    x.Status ==
                        InspectionStatus.Completed);


            // ----------------------------------------
            // 本日の異常項目
            // ----------------------------------------

            TodayAbnormalCount =
                todayRows.Sum(x =>
                    x.AbnormalCount);


            // ----------------------------------------
            // 完了率
            // ----------------------------------------

            CompletionRate =
                TodayInspectionCount == 0
                    ? 0
                    : (int)Math.Round(
                        (double)
                        TodayCompletedCount /
                        TodayInspectionCount *
                        100);
        }
        catch (Exception exception)
        {
            ErrorMessage =
                "ダッシュボードを読み込めませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
        finally
        {
            IsLoading =
                false;
        }
    }


    // ============================================
    // Refresh
    // ============================================

    public void Refresh()
    {
        _ = LoadAsync();
    }


    // ============================================
    // Navigation Commands
    // ============================================

    [RelayCommand]
    private void OpenInspectionStatus()
    {
        InspectionStatusRequested?.Invoke();
    }

    [RelayCommand]
    private void OpenNotStarted()
    {
        NotStartedRequested?.Invoke();
    }

    [RelayCommand]
    private void OpenApprovalPending()
    {
        ApprovalPendingRequested?.Invoke();
    }

    [RelayCommand]
    private void OpenAbnormalList()
    {
        AbnormalListRequested?.Invoke();
    }
}