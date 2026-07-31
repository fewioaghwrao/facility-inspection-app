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
        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.ToTable("Equipments");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();
        });
    }
}