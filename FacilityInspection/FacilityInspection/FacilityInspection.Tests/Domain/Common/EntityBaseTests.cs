using FacilityInspection.Domain.Common;
using Xunit;

namespace FacilityInspection.Tests.Domain.Common;

public sealed class EntityBaseTests
{
    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithoutId_GeneratesNonEmptyId()
    {
        // Act
        var entity =
            new TestEntity();

        // Assert
        Assert.NotEqual(
            Guid.Empty,
            entity.Id);
    }


    [Fact]
    public void Constructor_WithId_UsesSpecifiedId()
    {
        // Arrange
        var id =
            Guid.NewGuid();

        // Act
        var entity =
            new TestEntity(
                id);

        // Assert
        Assert.Equal(
            id,
            entity.Id);
    }


    [Fact]
    public void Constructor_SetsCreatedAndUpdatedTimeToCurrentUtcTime()
    {
        // Arrange
        var before =
            DateTime.UtcNow;

        // Act
        var entity =
            new TestEntity();

        var after =
            DateTime.UtcNow;

        // Assert
        Assert.InRange(
            entity.CreatedAtUtc,
            before,
            after);

        Assert.InRange(
            entity.UpdatedAtUtc,
            before,
            after);

        Assert.Equal(
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc);
    }


    // ============================================
    // MarkUpdated
    // ============================================

    [Fact]
    public void MarkUpdated_UpdatesUpdatedAtUtcWithoutChangingCreatedAtUtc()
    {
        // Arrange
        var entity =
            new TestEntity();

        var originalCreatedAtUtc =
            entity.CreatedAtUtc;

        var originalUpdatedAtUtc =
            entity.UpdatedAtUtc;

        var beforeUpdate =
            DateTime.UtcNow;

        // Act
        entity.UpdateTimestamp();

        var afterUpdate =
            DateTime.UtcNow;

        // Assert
        Assert.Equal(
            originalCreatedAtUtc,
            entity.CreatedAtUtc);

        Assert.InRange(
            entity.UpdatedAtUtc,
            beforeUpdate,
            afterUpdate);

        Assert.True(
            entity.UpdatedAtUtc >=
            originalUpdatedAtUtc);
    }


    // ============================================
    // Test Entity
    // ============================================

    private sealed class TestEntity : EntityBase
    {
        public TestEntity()
            : base()
        {
        }

        public TestEntity(
            Guid id)
            : base(id)
        {
        }

        public void UpdateTimestamp()
        {
            MarkUpdated();
        }
    }
}