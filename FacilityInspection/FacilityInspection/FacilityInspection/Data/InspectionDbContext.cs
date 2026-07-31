using FacilityInspection.Domain.Equipments;
using FacilityInspection.Domain.Locations;
using FacilityInspection.Domain.Sites;
using Microsoft.EntityFrameworkCore;
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
