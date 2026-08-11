using FacilityInspection.Domain.AuditLogs;
using FacilityInspection.Domain.Equipments;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.Domain.InspectionTemplates;
using FacilityInspection.Domain.Locations;
using FacilityInspection.Domain.Operators;
using FacilityInspection.Domain.Sites;
using Microsoft.EntityFrameworkCore;
using System;

namespace FacilityInspection.Data;

public sealed class InspectionDbContext
    : DbContext
{
    private readonly string
        _databasePath;


    // ============================================
    // Constructor
    // ============================================

    public InspectionDbContext(
        string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            databasePath);

        _databasePath =
            databasePath;
    }


    // ============================================
    // Master
    // ============================================

    public DbSet<FactorySite> FactorySites =>
        Set<FactorySite>();

    public DbSet<Location> Locations =>
        Set<Location>();

    public DbSet<Equipment> Equipments =>
        Set<Equipment>();

    public DbSet<Operator> Operators =>
        Set<Operator>();


    // ============================================
    // Inspection Template
    // ============================================

    public DbSet<InspectionTemplate>
        InspectionTemplates =>
            Set<InspectionTemplate>();

    public DbSet<InspectionTemplateItem>
        InspectionTemplateItems =>
            Set<InspectionTemplateItem>();


    // ============================================
    // Inspection
    // ============================================

    public DbSet<InspectionSchedule>
        InspectionSchedules =>
            Set<InspectionSchedule>();

    public DbSet<Inspection> Inspections =>
        Set<Inspection>();

    public DbSet<InspectionResult>
        InspectionResults =>
            Set<InspectionResult>();

    public DbSet<InspectionPhoto>
        InspectionPhotos =>
            Set<InspectionPhoto>();


    // ============================================
    // Audit Log
    // ============================================

    public DbSet<AuditLog>
        AuditLogs =>
            Set<AuditLog>();


    // ============================================
    // Configuring
    // ============================================

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(
            $"Data Source={_databasePath}");
    }


    // ============================================
    // Model
    // ============================================

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        /*
         * Data/Configurations にある
         * IEntityTypeConfiguration<T>
         * 実装を自動的に読み込む。
         *
         * AuditLogConfiguration も
         * ここで自動適用される。
         */
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(InspectionDbContext).Assembly);
    }
}