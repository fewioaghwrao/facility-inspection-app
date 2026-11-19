using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace FacilityInspection.Data;

/// <summary>
/// dotnet ef（マイグレーション作成など）専用のファクトリー。
/// アプリ実行時には使用しない。
/// </summary>
/// <remarks>
/// InspectionDbContext は DB パスを受け取るコンストラクターしか持たないため、
/// 設計時ツールがインスタンスを生成できるようにする。
/// migrations add はモデルから差分を作るだけなので、
/// ここで指定するDBファイルには接続しない。
/// </remarks>
public sealed class DesignTimeInspectionDbContextFactory
    : IDesignTimeDbContextFactory<InspectionDbContext>
{
    public InspectionDbContext CreateDbContext(
        string[] args)
    {
        var databasePath =
            Path.Combine(
                Path.GetTempPath(),
                "facility-inspection-design-time.db");

        return new InspectionDbContext(
            databasePath);
    }
}
