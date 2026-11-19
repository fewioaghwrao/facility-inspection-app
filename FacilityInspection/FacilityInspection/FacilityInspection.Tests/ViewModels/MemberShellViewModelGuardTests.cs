using FacilityInspection.ViewModels;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

/// <summary>
/// 点検担当者シェルの、未保存変更がある画面からの離脱確認に関するテスト。
/// </summary>
public sealed class MemberShellViewModelGuardTests
{
    private static readonly Guid OperatorId =
        Guid.Parse("AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE");

    private static readonly Guid ScheduleId =
        Guid.Parse("11111111-2222-3333-4444-555555555555");

    private readonly FakeGuardViewModel _entry =
        new();

    private readonly StubViewModel _inspectionList =
        new();

    private Action<Guid>? _openInspection;

    private int _logoutCount;


    [Fact]
    public void OpenInspectionListCommand_入力画面に未保存変更あり_確認されるまで移動しない()
    {
        // Arrange
        var sut =
            CreateShellShowingEntry(
                hasUnsavedChanges: true);

        // Act
        sut.OpenInspectionListCommand.Execute(null);

        // Assert
        Assert.Same(
            _entry,
            sut.CurrentContent);

        Assert.Equal(
            1,
            _entry.RequestLeaveCount);

        // 確認ダイアログで移動を選ぶ
        _entry.ProceedPendingLeave();

        Assert.Same(
            _inspectionList,
            sut.CurrentContent);
    }


    [Fact]
    public void OpenDashboardCommand_入力画面に未保存変更あり_確認されるまで移動しない()
    {
        // Arrange
        var sut =
            CreateShellShowingEntry(
                hasUnsavedChanges: true);

        // Act
        sut.OpenDashboardCommand.Execute(null);

        // Assert
        Assert.Same(
            _entry,
            sut.CurrentContent);

        _entry.ProceedPendingLeave();

        Assert.NotSame(
            _entry,
            sut.CurrentContent);
    }


    [Fact]
    public void OpenLogoutDialogCommand_入力画面に未保存変更あり_確認後にログアウトする()
    {
        // Arrange
        var sut =
            CreateShellShowingEntry(
                hasUnsavedChanges: true);

        // Act
        sut.OpenLogoutDialogCommand.Execute(null);

        // Assert
        Assert.False(
            sut.IsLogoutDialogOpen);

        Assert.Equal(
            0,
            _logoutCount);

        _entry.ProceedPendingLeave();

        Assert.Equal(
            1,
            _logoutCount);
    }


    [Fact]
    public void OpenLogoutDialogCommand_未保存変更なし_通常のログアウト確認を表示する()
    {
        // Arrange
        var sut =
            CreateShellShowingEntry(
                hasUnsavedChanges: false);

        // Act
        sut.OpenLogoutDialogCommand.Execute(null);

        // Assert
        Assert.True(
            sut.IsLogoutDialogOpen);

        Assert.Equal(
            0,
            _entry.RequestLeaveCount);
    }


    [Fact]
    public void ActiveNavigationGuard_入力画面表示中_入力画面を返す()
    {
        var sut =
            CreateShellShowingEntry(
                hasUnsavedChanges: true);

        Assert.Same(
            _entry,
            sut.ActiveNavigationGuard);
    }


    // ============================================
    // Helper
    // ============================================

    private MemberShellViewModel CreateShellShowingEntry(
        bool hasUnsavedChanges)
    {
        _entry.HasUnsavedChanges =
            hasUnsavedChanges;

        var sut =
            new MemberShellViewModel(
                OperatorId,
                "テスト点検担当者",
                () => _logoutCount++,
                (_, openInspection) =>
                {
                    _openInspection = openInspection;
                    return new StubViewModel();
                },
                _ => _inspectionList,
                () => new StubViewModel(),
                (_, _, _) => _entry);

        // ダッシュボードから点検を開く
        _openInspection!(
            ScheduleId);

        Assert.Same(
            _entry,
            sut.CurrentContent);

        return sut;
    }


    private sealed class StubViewModel
        : ViewModelBase
    {
    }


    private sealed class FakeGuardViewModel
        : ViewModelBase,
          INavigationGuard
    {
        private Action? _pending;

        public bool HasUnsavedChanges { get; set; }

        public int RequestLeaveCount { get; private set; }

        public void RequestLeave(
            Action proceed)
        {
            RequestLeaveCount++;

            if (!HasUnsavedChanges)
            {
                proceed();
                return;
            }

            _pending = proceed;
        }

        public void ProceedPendingLeave()
        {
            var pending =
                _pending;

            _pending = null;

            pending?.Invoke();
        }
    }
}
