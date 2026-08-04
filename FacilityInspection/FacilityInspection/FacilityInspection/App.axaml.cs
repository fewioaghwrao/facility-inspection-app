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

        var dbContextFactory =
            new InspectionDbContextFactory(
                databasePath);

        using (var dbContext =
               dbContextFactory.CreateDbContext())
        {
            dbContext.Database.EnsureCreated();
        }

        var passwordHasher =
            new PasswordHasher<Operator>();

        var operatorSeedService =
            new OperatorSeedService(
                dbContextFactory,
                passwordHasher);

        operatorSeedService
            .SeedAsync()
            .GetAwaiter()
            .GetResult();

        IAuthenticationService authenticationService =
            new AuthenticationService(
                dbContextFactory,
                passwordHasher);

        var currentUserSession =
            new CurrentUserSession();

        return new MainViewModel(
            authenticationService,
            currentUserSession);
    }
}