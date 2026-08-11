using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FacilityInspection.Data;
using FacilityInspection.Data.Seeds;
using FacilityInspection.Domain.Operators;
using FacilityInspection.Services.Authentication;
using FacilityInspection.ViewModels;
using FacilityInspection.Views;
using Microsoft.AspNetCore.Identity;
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
            dbContextFactory);


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
            auditLogRepository);
    }


    // ============================================
    // DB初期化
    // ============================================

    private static void InitializeDatabase(
        InspectionDbContextFactory dbContextFactory)
    {
        using var dbContext =
            dbContextFactory.CreateDbContext();

        dbContext.Database.EnsureCreated();
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