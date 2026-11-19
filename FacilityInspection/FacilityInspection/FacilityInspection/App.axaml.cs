using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FacilityInspection.Data;
using FacilityInspection.Data.Seeds;
using FacilityInspection.Domain.Operators;
using FacilityInspection.Services.Authentication;
using FacilityInspection.Services.Backup;
using FacilityInspection.ViewModels;
using FacilityInspection.Views;
using Microsoft.AspNetCore.Identity;
using FacilityInspection.Services.Backup;
using System;
using System.IO;

namespace FacilityInspection;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var mainViewModel =
            CreateMainViewModel();

        if (ApplicationLifetime is
            IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow =
                new MainWindow
                {
                    DataContext = mainViewModel
                };
        }
        else if (ApplicationLifetime is
            ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView =
                new MainView
                {
                    DataContext = mainViewModel
                };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static MainViewModel CreateMainViewModel()
    {
        // ========================================
        // SQLite
        // ========================================

        var databaseDirectory =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder
                        .LocalApplicationData),
                "FacilityInspection");

        Directory.CreateDirectory(
            databaseDirectory);

        var databasePath =
            Path.Combine(
                databaseDirectory,
                "facility-inspection.db");



        var dbContextFactory =
            new InspectionDbContextFactory(
                databasePath);


        // ========================================
        // DB初期化
        // ========================================

        InitializeDatabase(
            databasePath);


        // ========================================
        // PasswordHasher
        // ========================================

        var passwordHasher =
            new PasswordHasher<Operator>();


        // ========================================
        // Seed
        //
        // 依存関係のある順番で登録する
        // ========================================

        // Operator
        SeedOperators(
            dbContextFactory,
            passwordHasher);

        // FactorySite / Location
        SeedLocations(
            dbContextFactory);

        // Equipment
        SeedEquipments(
            dbContextFactory);

        // InspectionTemplate
        SeedInspectionTemplates(
            dbContextFactory);

        // InspectionSchedule
        SeedInspectionSchedules(
            dbContextFactory);

        // Inspection / Result / Photo
        SeedInspections(
            dbContextFactory);

        // AuditLog
        SeedAuditLogs(
            dbContextFactory);


        // ========================================
        // 写真整理
        //
        // 異常終了などでDBに登録されないまま残った写真を
        // ゴミ箱フォルダーへ移動する。
        // ========================================

        CleanupOrphanPhotos(
            dbContextFactory);


        // ========================================
        // Service
        // ========================================

        IAuthenticationService authenticationService =
            new AuthenticationService(
                dbContextFactory,
                passwordHasher);

        var currentUserSession =
            new CurrentUserSession();


        // ========================================
        // Repository
        // ========================================

        var inspectionTemplateRepository =
            new InspectionTemplateRepository(
                dbContextFactory);

        var operatorRepository =
            new OperatorRepository(
                dbContextFactory,
                passwordHasher);

        var scheduleRepository =
            new ScheduleRepository(
                dbContextFactory);

        var inspectionRepository =
            new InspectionRepository(
                dbContextFactory);

        var auditLogRepository =
    new AuditLogRepository(
        dbContextFactory);

        /*
         * 復元したバックアップが旧スキーマの場合に備え、
         * 復元後にもマイグレーションを適用する。
         */
        var databaseBackupService =
            new DatabaseBackupService(
                databasePath,
                DatabaseInitializer.Initialize);

        var backupFilePickerService =
            new BackupFilePickerService();

        // ========================================
        // MainViewModel
        // ========================================

        return new MainViewModel(
            authenticationService,
            currentUserSession,
            inspectionTemplateRepository,
            operatorRepository,
            scheduleRepository,
            inspectionRepository,
            auditLogRepository,
            databaseBackupService,
            backupFilePickerService);
    }


    // ============================================
    // DB初期化
    //
    // マイグレーションを適用してスキーマを最新化する。
    // EnsureCreatedで作成された旧DBも
    // DatabaseInitializer側で移行する。
    // ============================================

    private static void InitializeDatabase(
        string databasePath)
    {
        DatabaseInitializer.Initialize(
            databasePath);
    }


    // ============================================
    // 写真整理
    // ============================================

    private static void CleanupOrphanPhotos(
        InspectionDbContextFactory dbContextFactory)
    {
        try
        {
            var cleaner =
                new InspectionPhotoCleaner(
                    dbContextFactory,
                    InspectionPhotoStorage.RootDirectoryPath);

            cleaner
                .CleanupAsync(
                    minimumOrphanAge:
                        TimeSpan.FromHours(1),
                    trashRetention:
                        TimeSpan.FromDays(30))
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception)
        {
            /*
             * 写真整理に失敗しても業務は継続できるため、
             * 起動を止めない。
             */
        }
    }


    // ============================================
    // Operator Seed
    // ============================================

    private static void SeedOperators(
        InspectionDbContextFactory dbContextFactory,
        PasswordHasher<Operator> passwordHasher)
    {
        var seedService =
            new OperatorSeedService(
                dbContextFactory,
                passwordHasher);

        seedService
            .SeedAsync()
            .GetAwaiter()
            .GetResult();
    }


    // ============================================
    // Location Seed
    // ============================================

    private static void SeedLocations(
        InspectionDbContextFactory dbContextFactory)
    {
        var seedService =
            new LocationSeedService(
                dbContextFactory);

        seedService
            .SeedAsync()
            .GetAwaiter()
            .GetResult();
    }


    // ============================================
    // Equipment Seed
    // ============================================

    private static void SeedEquipments(
        InspectionDbContextFactory dbContextFactory)
    {
        var seedService =
            new EquipmentSeedService(
                dbContextFactory);

        seedService
            .SeedAsync()
            .GetAwaiter()
            .GetResult();
    }


    // ============================================
    // InspectionTemplate Seed
    // ============================================

    private static void SeedInspectionTemplates(
        InspectionDbContextFactory dbContextFactory)
    {
        using var dbContext =
            dbContextFactory.CreateDbContext();

        var seedService =
            new InspectionTemplateSeedService(
                dbContext);

        seedService
            .SeedAsync()
            .GetAwaiter()
            .GetResult();
    }


    // ============================================
    // InspectionSchedule Seed
    // ============================================

    private static void SeedInspectionSchedules(
        InspectionDbContextFactory dbContextFactory)
    {
        var seedService =
            new InspectionScheduleSeedService(
                dbContextFactory);

        seedService
            .SeedAsync()
            .GetAwaiter()
            .GetResult();
    }


    // ============================================
    // Inspection / Result / Photo Seed
    // ============================================

    private static void SeedInspections(
        InspectionDbContextFactory dbContextFactory)
    {
        var seedService =
            new InspectionSeedService(
                dbContextFactory);

        seedService
            .SeedAsync()
            .GetAwaiter()
            .GetResult();
    }

    // ============================================
    // AuditLog Seed
    // ============================================

    private static void SeedAuditLogs(
        InspectionDbContextFactory dbContextFactory)
    {
        var seedService =
            new AuditLogSeedService(
                dbContextFactory);

        seedService
            .SeedAsync()
            .GetAwaiter()
            .GetResult();
    }
}