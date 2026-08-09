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
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }
        else if (ApplicationLifetime is
            ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new MainView
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static MainViewModel CreateMainViewModel()
    {
        var databaseDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "FacilityInspection");

        Directory.CreateDirectory(
            databaseDirectory);

        var databasePath = Path.Combine(
            databaseDirectory,
            "facility-inspection.db");

        var sampleDatabasePath = Path.Combine(
            AppContext.BaseDirectory,
            "sample",
            "database",
            "facility-inspection-sample.db");

        // 実行用DBが存在しない場合だけ、
        // リポジトリに含めたサンプルDBからコピーする
     /*   if (!File.Exists(databasePath) &&
            File.Exists(sampleDatabasePath))
        {
            File.Copy(
                sampleDatabasePath,
                databasePath);
        }*/

        var dbContextFactory =
            new InspectionDbContextFactory(
                databasePath);

        InitializeDatabase(
            dbContextFactory);

        var passwordHasher =
            new PasswordHasher<Operator>();

        SeedOperators(
            dbContextFactory,
            passwordHasher);

        SeedLocations(
    dbContextFactory);

        SeedEquipments(
    dbContextFactory);

        SeedInspectionTemplates(
            dbContextFactory);

        SeedInspectionSchedules(
    dbContextFactory);

        SeedInspections(
    dbContextFactory);

        IAuthenticationService authenticationService =
            new AuthenticationService(
                dbContextFactory,
                passwordHasher);

        var currentUserSession =
            new CurrentUserSession();

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


        return new MainViewModel(
            authenticationService,
            currentUserSession,
            inspectionTemplateRepository,
            operatorRepository,
            scheduleRepository,
            inspectionRepository);
    }

    private static void InitializeDatabase(
        InspectionDbContextFactory
            dbContextFactory)
    {
        using var dbContext =
            dbContextFactory.CreateDbContext();

        dbContext.Database.EnsureCreated();
    }

    private static void SeedOperators(
        InspectionDbContextFactory
            dbContextFactory,
        PasswordHasher<Operator>
            passwordHasher)
    {
        var operatorSeedService =
            new OperatorSeedService(
                dbContextFactory,
                passwordHasher);

        operatorSeedService
            .SeedAsync()
            .GetAwaiter()
            .GetResult();
    }

    private static void SeedInspectionTemplates(
        InspectionDbContextFactory
            dbContextFactory)
    {
        using var dbContext =
            dbContextFactory.CreateDbContext();

        var inspectionTemplateSeedService =
            new InspectionTemplateSeedService(
                dbContext);

        inspectionTemplateSeedService
            .SeedAsync()
            .GetAwaiter()
            .GetResult();
    }

    private static void SeedEquipments(
    InspectionDbContextFactory dbContextFactory)
    {
        var equipmentSeedService =
            new EquipmentSeedService(
                dbContextFactory);

        equipmentSeedService
            .SeedAsync()
            .GetAwaiter()
            .GetResult();
    }

    private static void SeedLocations(
    InspectionDbContextFactory dbContextFactory)
    {
        var locationSeedService =
            new LocationSeedService(
                dbContextFactory);

        locationSeedService
            .SeedAsync()
            .GetAwaiter()
            .GetResult();
    }

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

    private static void SeedInspections(
    InspectionDbContextFactory dbContextFactory)
    {
        var inspectionSeedService =
            new InspectionSeedService(
                dbContextFactory);

        inspectionSeedService
            .SeedAsync()
            .GetAwaiter()
            .GetResult();
    }
}
