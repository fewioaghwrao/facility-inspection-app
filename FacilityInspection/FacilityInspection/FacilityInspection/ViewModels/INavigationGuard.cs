using System;

namespace FacilityInspection.ViewModels;

/// <summary>
/// 保存していない入力内容を持つ画面が実装する。
/// シェルやウィンドウは、画面を切り替える前にこの画面へ離脱可否を問い合わせる。
/// </summary>
public interface INavigationGuard
{
    /// <summary>
    /// 保存していない変更があるかどうか。
    /// </summary>
    bool HasUnsavedChanges { get; }

    /// <summary>
    /// 画面を離れてよいか確認する。
    /// 変更が無い場合、または利用者が移動を選んだ場合に
    /// <paramref name="proceed"/> を呼び出す。
    /// 利用者がキャンセルした場合は呼び出さない。
    /// </summary>
    void RequestLeave(
        Action proceed);
}
