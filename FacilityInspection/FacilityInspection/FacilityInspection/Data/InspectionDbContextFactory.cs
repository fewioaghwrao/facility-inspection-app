using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FacilityInspection.Data;

public sealed class InspectionDbContextFactory
    : IDbContextFactory<InspectionDbContext>
{
    private readonly string _databasePath;

    public InspectionDbContextFactory(
        string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            databasePath);

        _databasePath = databasePath;
    }

    public InspectionDbContext CreateDbContext()
    {
        return new InspectionDbContext(
            _databasePath);
    }

    public Task<InspectionDbContext>
        CreateDbContextAsync(
            CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            CreateDbContext());
    }
}