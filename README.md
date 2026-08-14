# Facility Inspection App

**Avalonia UI / C# / SQLite で構築した、設備点検・保守記録アプリです。**

工場設備の点検業務を対象に、**点検予定の作成 → 担当者による点検実施 → 結果・写真の記録 → 保全管理者による確認・承認／差し戻し → 操作履歴の保存**までを一連の流れとして扱います。

ローカルSQLiteを利用する構成とし、点検担当者と保全管理者で画面・操作を分離しています。

---

## Screenshots

### ログイン / 点検担当者

<table>
  <tr>
    <td width="50%"><img src="docs/images/screenshots/login.png" alt="ログイン画面"></td>
    <td width="50%"><img src="docs/images/screenshots/member-schedule.png" alt="点検担当者予定"></td>
  </tr>
  <tr>
    <td align="center">ログイン</td>
    <td align="center">点検担当者 - 点検予定</td>
  </tr>
  <tr>
    <td width="50%"><img src="docs/images/screenshots/member-inspection-list.png" alt="点検担当者点検一覧"></td>
    <td width="50%"><img src="docs/images/screenshots/logout-dialog.png" alt="ログアウト確認"></td>
  </tr>
  <tr>
    <td align="center">担当点検一覧</td>
    <td align="center">ログアウト確認</td>
  </tr>
</table>

### 保全管理者

<table>
  <tr>
    <td width="50%"><img src="docs/images/screenshots/admin-dashboard.png" alt="保全管理者ダッシュボード"></td>
    <td width="50%"><img src="docs/images/screenshots/inspection-status.png" alt="点検実施状況"></td>
  </tr>
  <tr>
    <td align="center">保全管理者ダッシュボード</td>
    <td align="center">点検実施状況</td>
  </tr>
  <tr>
    <td width="50%"><img src="docs/images/screenshots/approval-pending.png" alt="完了承認待ち"></td>
    <td width="50%"><img src="docs/images/screenshots/approval-return.png" alt="承認・差し戻し"></td>
  </tr>
  <tr>
    <td align="center">完了・承認待ち</td>
    <td align="center">承認・差し戻し</td>
  </tr>
</table>

<details>
<summary><strong>全スクリーンショットを表示</strong></summary>

### 共通

| 画面 | スクリーンショット |
|---|---|
| ログイン | <img src="docs/images/screenshots/login.png" alt="ログイン画面" width="560"> |
| ログアウト確認 | <img src="docs/images/screenshots/logout-dialog.png" alt="ログアウト確認" width="560"> |

### 点検担当者

| 画面 | スクリーンショット |
|---|---|
| 点検予定 | <img src="docs/images/screenshots/member-schedule.png" alt="点検担当者予定" width="560"> |
| 点検一覧 | <img src="docs/images/screenshots/member-inspection-list.png" alt="点検担当者点検一覧" width="560"> |

### 保全管理者 - 点検管理

| 画面 | スクリーンショット |
|---|---|
| ダッシュボード | <img src="docs/images/screenshots/admin-dashboard.png" alt="保全管理者ダッシュボード" width="560"> |
| 点検予定カレンダー | <img src="docs/images/screenshots/admin-schedule-calendar.png" alt="保全責任者点検予定カレンダー" width="560"> |
| 点検予定 新規登録 | <img src="docs/images/screenshots/schedule-create.png" alt="点検予定の新規登録" width="560"> |
| 点検実施状況 | <img src="docs/images/screenshots/inspection-status.png" alt="点検実施状況" width="560"> |
| 点検実施詳細 | <img src="docs/images/screenshots/inspection-detail.png" alt="点検実施詳細" width="560"> |
| 未実施一覧 | <img src="docs/images/screenshots/not-started-list.png" alt="未実施一覧" width="560"> |
| 異常一覧 | <img src="docs/images/screenshots/abnormal-list.png" alt="異常一覧" width="560"> |
| 完了・承認待ち | <img src="docs/images/screenshots/approval-pending.png" alt="完了承認待ち" width="560"> |
| 承認・差し戻し | <img src="docs/images/screenshots/approval-return.png" alt="承認差し戻し" width="560"> |

### 保全管理者 - マスタ / 運用管理

| 画面 | スクリーンショット |
|---|---|
| 設備登録 | <img src="docs/images/screenshots/equipment-register.png" alt="設備登録" width="560"> |
| 点検表テンプレート | <img src="docs/images/screenshots/inspection-template.png" alt="点検表テンプレート" width="560"> |
| 点検表テンプレート作成 | <img src="docs/images/screenshots/inspection-template-create.png" alt="点検表テンプレート作成" width="560"> |
| 担当者一覧・管理 | <img src="docs/images/screenshots/operator-management.png" alt="担当者一覧・管理" width="560"> |
| 担当者新規登録 | <img src="docs/images/screenshots/operator-create.png" alt="担当者新規登録" width="560"> |
| 操作履歴 | <img src="docs/images/screenshots/audit-log.png" alt="操作履歴" width="560"> |
| バックアップ / 復元 | <img src="docs/images/screenshots/backup-restore.png" alt="バックアップ復元" width="560"> |

</details>

---

## 目的

設備点検では、次のような情報を継続して管理する必要があります。

- どの設備を、いつ点検するか
- 誰が点検を担当するか
- どの点検表を使用するか
- 各点検項目の結果はどうだったか
- 異常があったか
- 写真などの証跡が残っているか
- 点検完了後に承認されたか、差し戻されたか
- 誰がどの操作を行ったか

本アプリでは、これらを個別の画面・データとして分断せず、**点検予定を起点とした一連の業務フロー**として管理することを目標としています。

---

## 主な機能

### 共通

- ローカルログイン
- ロールに応じた画面切り替え
- ログアウト確認

### 点検担当者

- 担当する点検予定の確認
- 点検予定カレンダー
- 担当点検一覧
- 点検チェックリスト入力
- 点検結果の記録
- 異常判定・コメント入力
- 点検写真の登録
- 点検完了

### 保全管理者

- 点検実施状況の確認
- 点検実施詳細の確認
- 異常一覧
- 未実施一覧
- 完了・承認待ち一覧
- 点検結果の承認
- 点検担当者への差し戻し
- 設備台帳管理
- 点検表テンプレート管理
- 点検予定管理
- 担当者管理
- 操作履歴の確認
- SQLiteデータベースのバックアップ / 復元

---

## 利用ロール

| ロール | 主な役割 |
|---|---|
| 点検担当者 | 自分に割り当てられた点検予定を確認し、チェックリスト・測定値・コメント・写真などの点検結果を登録する |
| 保全管理者 | 点検予定・設備・テンプレート・担当者を管理し、実施結果の確認、異常確認、承認・差し戻し、操作履歴確認を行う |

---

## 画面遷移

### 点検担当者

ログイン後、点検担当者向けサイドメニューから「点検予定」「点検一覧」へ移動し、対象の点検を選択して点検を実施します。

<p align="center">
  <img src="docs/images/member-screen-flow.png" alt="点検担当者 画面遷移図" width="900">
</p>

### 保全管理者

保全管理者は点検実施状況を中心に、異常・未実施・承認待ちの確認に加え、設備、テンプレート、予定、担当者、操作履歴、バックアップ / 復元を管理します。

<p align="center">
  <img src="docs/images/admin-screen-flow.png" alt="保全管理者 画面遷移図" width="1100">
</p>

---

## ER図

<p align="center">
  <img src="docs/images/er-diagram.png" alt="ER図" width="1000">
</p>

### 主なデータ関係

```text
FactorySite
    └─ Location
         └─ Equipment
              └─ InspectionSchedule
                   └─ Inspection
                        ├─ InspectionResult
                        └─ InspectionPhoto

InspectionTemplate
    └─ InspectionTemplateItem
         └─ InspectionResult

Operator
    ├─ InspectionSchedule
    ├─ Inspection
    └─ AuditLog
```

点検予定・実績・結果を分離し、予定に対して実際に行われた点検と、点検項目ごとの結果を履歴として保存する構成です。

---

## 主なエンティティ

| エンティティ | 役割 |
|---|---|
| `FactorySite` | 工場・拠点 |
| `Location` | 工場内の設置場所 |
| `Equipment` | 点検対象設備 |
| `Operator` | 点検担当者 / 保全管理者 |
| `InspectionTemplate` | 設備種別ごとの点検表テンプレート |
| `InspectionTemplateItem` | 点検表内の個別点検項目 |
| `InspectionSchedule` | 設備・点検表・担当者・予定日を紐づけた点検予定 |
| `Inspection` | 点検の実施状態・実施者・完了日時・レビュー状態 |
| `InspectionResult` | 点検項目ごとの入力値・異常判定・コメント |
| `InspectionPhoto` | 点検実績 / 点検項目に紐づく写真 |
| `AuditLog` | 作成・更新・点検・承認・差し戻し等の操作履歴 |

### データ設計上のポイント

- `InspectionSchedule` と `Inspection` を分離し、「予定」と「実施実績」を別管理
- 1つの点検予定に対する実施実績は最大1件となるように制約
- `InspectionResult` は `InspectionId + InspectionTemplateItemId` の組み合わせで重複登録を防止
- `InspectionResult` に項目名・入力種別・単位等を保持し、点検時点の結果情報を保存できる構成
- 点検写真は点検全体、または特定の点検結果に紐づけ可能
- `AuditLog` で操作者と操作対象を記録

---

## 技術スタック

| 分類 | 技術 |
|---|---|
| Language | C# / .NET |
| UI | Avalonia UI |
| Architecture | MVVM |
| MVVM Support | CommunityToolkit.Mvvm |
| ORM | Entity Framework Core |
| Database | SQLite |
| Data Access | Repository パターン |
| Diagram | draw.io / Mermaid |

---

## アーキテクチャ

```text
┌─────────────────────────────┐
│          Avalonia UI        │
│             Views           │
└──────────────┬──────────────┘
               │ Binding / Command
┌──────────────▼──────────────┐
│          ViewModels         │
│   CommunityToolkit.Mvvm     │
└──────────────┬──────────────┘
               │
       ┌───────┴────────┐
       │                │
┌──────▼───────┐  ┌─────▼─────────────┐
│ Repositories │  │     Services      │
│              │  │ Authentication    │
│              │  │ Backup / Photo    │
└──────┬───────┘  └───────────────────┘
       │
┌──────▼──────────────────────┐
│      InspectionDbContext    │
│      Entity Framework Core  │
└──────────────┬──────────────┘
               │
┌──────────────▼──────────────┐
│            SQLite           │
└─────────────────────────────┘
```

UIロジックはViewModelへ分離し、データアクセスはRepository、認証・バックアップなどはServiceへ分離しています。

---

## プロジェクト構成

主要な構成は次のとおりです。

```text
FacilityInspection/
├─ FacilityInspection.slnx
├─ Directory.Packages.props
│
├─ FacilityInspection.Desktop/
│  ├─ Program.cs
│  └─ FacilityInspection.Desktop.csproj
│
└─ FacilityInspection/
   ├─ Data/
   │  ├─ Configurations/       # EF Core Entity Configuration
   │  ├─ Seeds/                # 初期データ
   │  ├─ InspectionDbContext.cs
   │  ├─ InspectionRepository.cs
   │  ├─ ScheduleRepository.cs
   │  └─ ...
   │
   ├─ Domain/
   │  ├─ AuditLogs/
   │  ├─ Equipments/
   │  ├─ Inspections/
   │  ├─ InspectionTemplates/
   │  ├─ Locations/
   │  ├─ Operators/
   │  └─ Sites/
   │
   ├─ Services/
   │  ├─ Authentication/
   │  └─ Backup/
   │
   ├─ ViewModels/
   └─ Views/

sample/
├─ database/
│  └─ facility-inspection-sample.db
└─ images/
   └─ 点検結果用サンプル画像
```

---

## データベース

SQLiteを使用しています。

`InspectionDbContext` はデータベースファイルのパスをコンストラクタで受け取り、Entity Framework Core のSQLite Providerを利用して接続します。

また、各エンティティのマッピングは `Data/Configurations` に分離し、`IEntityTypeConfiguration<T>` の実装を `ApplyConfigurationsFromAssembly` でまとめて適用しています。

リポジトリには動作確認用のサンプルDBを含めています。

```text
sample/database/facility-inspection-sample.db
```

---

## 点検データの流れ

```text
点検予定を作成
    ↓
設備・点検表テンプレート・担当者を割り当て
    ↓
点検担当者が予定を確認
    ↓
点検開始
    ↓
チェックリスト / 測定値 / コメントを入力
    ↓
必要に応じて写真を登録
    ↓
点検完了
    ↓
保全管理者が結果を確認
    ├─ 承認
    └─ 差し戻し
```

この流れにより、「いつ・誰が・どの設備を・どの点検表で点検し、どの結果になったか」を追跡できるようにしています。

---

## 操作履歴

`AuditLog` により、アプリ内の主要操作を履歴として保持します。

対象例:

- データ作成 / 更新 / 削除
- 点検開始 / 点検完了
- 承認
- 差し戻し
- ログイン / ログアウト
- バックアップ / 復元

操作者 (`OperatorId`) に加えて、操作種別・対象種別・対象ID・変更前後の値・理由などを保持できる構造です。

---

## バックアップ / 復元

ローカルSQLiteを利用するため、保全管理者向けにデータベースのバックアップ / 復元画面を用意しています。

<p align="center">
  <img src="docs/images/screenshots/backup-restore.png" alt="バックアップ / 復元" width="760">
</p>

---

## 実行方法

### 前提

- .NET SDK
- Windows / Desktop環境

### Restore

```bash
dotnet restore FacilityInspection/FacilityInspection.slnx
```

### Run

```bash
dotnet run --project FacilityInspection/FacilityInspection/FacilityInspection.Desktop/FacilityInspection.Desktop.csproj
```

> 実行に必要な.NET SDKのバージョンは、プロジェクト設定に合わせてください。

---

## ポートフォリオとしてのポイント

このプロジェクトでは、単純なCRUDだけでなく、設備点検業務を想定した以下の設計・実装を行っています。

- Avalonia UI + MVVMによるデスクトップUI構成
- 点検担当者 / 保全管理者のロール別ナビゲーション
- Factory / Location / Equipment の設備階層管理
- 点検予定と点検実績を分離したデータモデル
- 点検表テンプレートと実施結果の履歴管理
- チェック・数値・テキスト等の点検入力
- 異常判定、コメント、写真の記録
- 完了後の承認 / 差し戻しフロー
- 操作履歴によるトレーサビリティ
- SQLiteによるローカルデータ保存
- EF Core ConfigurationによるDBマッピング分離
- Repository / Service / ViewModel の責務分離
- データベースのバックアップ / 復元

---

## 補足

本リポジトリは、設備点検・保守業務を題材にしたポートフォリオプロジェクトです。

現在のREADMEはDesktop版の構成を対象としており、画面・ER図・画面遷移図は実装内容をもとに整理しています。
