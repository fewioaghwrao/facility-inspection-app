using FacilityInspection.Domain.Equipments;
using FacilityInspection.Domain.Locations;
using FacilityInspection.Domain.Operators;
using FacilityInspection.Domain.Sites;
using Microsoft.EntityFrameworkCore;
using FacilityInspection.Domain.InspectionTemplates;
using System;

namespace FacilityInspection.Data;

public sealed class InspectionDbContext : DbContext
{
    private readonly string _databasePath;

    public InspectionDbContext(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        _databasePath = databasePath;
    }

    public DbSet<FactorySite> FactorySites => Set<FactorySite>();

    public DbSet<Location> Locations => Set<Location>();

    public DbSet<Equipment> Equipments => Set<Equipment>();

    public DbSet<Operator> Operators => Set<Operator>();

    public DbSet<InspectionTemplate> InspectionTemplates =>
    Set<InspectionTemplate>();

    public DbSet<InspectionTemplateItem> InspectionTemplateItems =>
        Set<InspectionTemplateItem>();

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(
            $"Data Source={_databasePath}");
    }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(InspectionDbContext).Assembly);
    }
}
