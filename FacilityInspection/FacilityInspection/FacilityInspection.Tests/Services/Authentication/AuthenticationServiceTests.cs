using FacilityInspection.Data;
using FacilityInspection.Domain.Operators;
using FacilityInspection.Services.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FacilityInspection.Tests.Services.Authentication;

public sealed class AuthenticationServiceTests
    : IDisposable
{
    private const string ValidPassword =
        "Demo1234!";

    private const string ErrorMessage =
        "ログインIDまたはパスワードが正しくありません。";

    private readonly string _databasePath;

    private readonly InspectionDbContextFactory
        _dbContextFactory;


    public AuthenticationServiceTests()
    {
        _databasePath =
            Path.Combine(
                Path.GetTempPath(),
                $"facility-inspection-auth-test-" +
                $"{Guid.NewGuid():N}.db");

        _dbContextFactory =
            new InspectionDbContextFactory(
                _databasePath);
    }


    // ============================================
    // 正常ログイン
    // ============================================

    [Fact]
    public async Task SignInAsync_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var passwordHasher =
            new PasswordHasher<Operator>();

        var user =
            await CreateAndSaveOperatorAsync(
                passwordHasher,
                loginId:
                    "inspector",
                displayName:
                    "点検担当者1",
                role:
                    OperatorRole.Inspector,
                password:
                    ValidPassword);

        var service =
            CreateService(
                passwordHasher);

        // Act
        var result =
            await service.SignInAsync(
                "inspector",
                ValidPassword);

        // Assert
        Assert.True(
            result.Succeeded);

        Assert.NotNull(
            result.User);

        Assert.Equal(
            string.Empty,
            result.ErrorMessage);

        Assert.Equal(
            user.Id,
            result.User.Id);

        Assert.Equal(
            user.LoginId,
            result.User.LoginId);

        Assert.Equal(
            user.DisplayName,
            result.User.DisplayName);

        Assert.Equal(
            user.Role,
            result.User.Role);
    }


    // ============================================
    // LoginId 正規化
    // ============================================

    [Fact]
    public async Task SignInAsync_WithLowercaseLoginId_ReturnsSuccess()
    {
        // Arrange
        var passwordHasher =
            new PasswordHasher<Operator>();

        await CreateAndSaveOperatorAsync(
            passwordHasher,
            loginId:
                "INSPECTOR",
            displayName:
                "点検担当者1",
            role:
                OperatorRole.Inspector,
            password:
                ValidPassword);

        var service =
            CreateService(
                passwordHasher);

        // Act
        var result =
            await service.SignInAsync(
                "inspector",
                ValidPassword);

        // Assert
        Assert.True(
            result.Succeeded);
    }


    [Fact]
    public async Task SignInAsync_WithSpacesAroundLoginId_ReturnsSuccess()
    {
        // Arrange
        var passwordHasher =
            new PasswordHasher<Operator>();

        await CreateAndSaveOperatorAsync(
            passwordHasher,
            loginId:
                "inspector",
            displayName:
                "点検担当者1",
            role:
                OperatorRole.Inspector,
            password:
                ValidPassword);

        var service =
            CreateService(
                passwordHasher);

        // Act
        var result =
            await service.SignInAsync(
                "  inspector  ",
                ValidPassword);

        // Assert
        Assert.True(
            result.Succeeded);
    }


    [Fact]
    public async Task SignInAsync_WithLowercaseAndSpacesLoginId_ReturnsSuccess()
    {
        // Arrange
        var passwordHasher =
            new PasswordHasher<Operator>();

        await CreateAndSaveOperatorAsync(
            passwordHasher,
            loginId:
                "INSPECTOR",
            displayName:
                "点検担当者1",
            role:
                OperatorRole.Inspector,
            password:
                ValidPassword);

        var service =
            CreateService(
                passwordHasher);

        // Act
        var result =
            await service.SignInAsync(
                "  inspector  ",
                ValidPassword);

        // Assert
        Assert.True(
            result.Succeeded);
    }


    // ============================================
    // 存在しないユーザー
    // ============================================

    [Fact]
    public async Task SignInAsync_WithUnknownLoginId_ReturnsFailure()
    {
        // Arrange
        await InitializeDatabaseAsync();

        var passwordHasher =
            new PasswordHasher<Operator>();

        var service =
            CreateService(
                passwordHasher);

        // Act
        var result =
            await service.SignInAsync(
                "unknown-user",
                ValidPassword);

        // Assert
        Assert.False(
            result.Succeeded);

        Assert.Null(
            result.User);

        Assert.Equal(
            ErrorMessage,
            result.ErrorMessage);
    }


    // ============================================
    // パスワード不一致
    // ============================================

    [Fact]
    public async Task SignInAsync_WithWrongPassword_ReturnsFailure()
    {
        // Arrange
        var passwordHasher =
            new PasswordHasher<Operator>();

        await CreateAndSaveOperatorAsync(
            passwordHasher,
            loginId:
                "inspector",
            displayName:
                "点検担当者1",
            role:
                OperatorRole.Inspector,
            password:
                ValidPassword);

        var service =
            CreateService(
                passwordHasher);

        // Act
        var result =
            await service.SignInAsync(
                "inspector",
                "WrongPassword!");

        // Assert
        Assert.False(
            result.Succeeded);

        Assert.Null(
            result.User);

        Assert.Equal(
            ErrorMessage,
            result.ErrorMessage);
    }


    // ============================================
    // 無効ユーザー
    // ============================================

    [Fact]
    public async Task SignInAsync_WithInactiveOperator_ReturnsFailure()
    {
        // Arrange
        var passwordHasher =
            new PasswordHasher<Operator>();

        await CreateAndSaveOperatorAsync(
            passwordHasher,
            loginId:
                "inspector",
            displayName:
                "点検担当者1",
            role:
                OperatorRole.Inspector,
            password:
                ValidPassword,
            isActive:
                false);

        var service =
            CreateService(
                passwordHasher);

        // Act
        var result =
            await service.SignInAsync(
                "inspector",
                ValidPassword);

        // Assert
        Assert.False(
            result.Succeeded);

        Assert.Null(
            result.User);

        Assert.Equal(
            ErrorMessage,
            result.ErrorMessage);
    }


    // ============================================
    // Role
    // ============================================

    [Fact]
    public async Task SignInAsync_WithInspector_ReturnsInspectorRole()
    {
        // Arrange
        var passwordHasher =
            new PasswordHasher<Operator>();

        await CreateAndSaveOperatorAsync(
            passwordHasher,
            loginId:
                "inspector",
            displayName:
                "点検担当者1",
            role:
                OperatorRole.Inspector,
            password:
                ValidPassword);

        var service =
            CreateService(
                passwordHasher);

        // Act
        var result =
            await service.SignInAsync(
                "inspector",
                ValidPassword);

        // Assert
        Assert.True(
            result.Succeeded);

        Assert.NotNull(
            result.User);

        Assert.Equal(
            OperatorRole.Inspector,
            result.User.Role);
    }


    [Fact]
    public async Task SignInAsync_WithMaintenanceManager_ReturnsMaintenanceManagerRole()
    {
        // Arrange
        var passwordHasher =
            new PasswordHasher<Operator>();

        await CreateAndSaveOperatorAsync(
            passwordHasher,
            loginId:
                "manager",
            displayName:
                "保全責任者",
            role:
                OperatorRole.MaintenanceManager,
            password:
                ValidPassword);

        var service =
            CreateService(
                passwordHasher);

        // Act
        var result =
            await service.SignInAsync(
                "manager",
                ValidPassword);

        // Assert
        Assert.True(
            result.Succeeded);

        Assert.NotNull(
            result.User);

        Assert.Equal(
            OperatorRole.MaintenanceManager,
            result.User.Role);
    }


    // ============================================
    // LastLoginAt
    // ============================================

    [Fact]
    public async Task SignInAsync_WhenSucceeded_UpdatesLastLoginAt()
    {
        // Arrange
        var passwordHasher =
            new PasswordHasher<Operator>();

        var user =
            await CreateAndSaveOperatorAsync(
                passwordHasher,
                loginId:
                    "inspector",
                displayName:
                    "点検担当者1",
                role:
                    OperatorRole.Inspector,
                password:
                    ValidPassword);

        var service =
            CreateService(
                passwordHasher);

        var before =
            DateTimeOffset.Now;

        // Act
        var result =
            await service.SignInAsync(
                "inspector",
                ValidPassword);

        var after =
            DateTimeOffset.Now;

        // Assert
        Assert.True(
            result.Succeeded);

        await using var dbContext =
            await _dbContextFactory
                .CreateDbContextAsync();

        var savedUser =
            await dbContext.Operators
                .SingleAsync(
                    x => x.Id == user.Id);

        Assert.NotNull(
            savedUser.LastLoginAt);

        Assert.InRange(
            savedUser.LastLoginAt!.Value,
            before,
            after);
    }


    [Fact]
    public async Task SignInAsync_WithWrongPassword_DoesNotUpdateLastLoginAt()
    {
        // Arrange
        var passwordHasher =
            new PasswordHasher<Operator>();

        var user =
            await CreateAndSaveOperatorAsync(
                passwordHasher,
                loginId:
                    "inspector",
                displayName:
                    "点検担当者1",
                role:
                    OperatorRole.Inspector,
                password:
                    ValidPassword);

        var service =
            CreateService(
                passwordHasher);

        // Act
        var result =
            await service.SignInAsync(
                "inspector",
                "WrongPassword!");

        // Assert
        Assert.False(
            result.Succeeded);

        await using var dbContext =
            await _dbContextFactory
                .CreateDbContextAsync();

        var savedUser =
            await dbContext.Operators
                .SingleAsync(
                    x => x.Id == user.Id);

        Assert.Null(
            savedUser.LastLoginAt);
    }


    [Fact]
    public async Task SignInAsync_WithInactiveOperator_DoesNotUpdateLastLoginAt()
    {
        // Arrange
        var passwordHasher =
            new PasswordHasher<Operator>();

        var user =
            await CreateAndSaveOperatorAsync(
                passwordHasher,
                loginId:
                    "inspector",
                displayName:
                    "点検担当者1",
                role:
                    OperatorRole.Inspector,
                password:
                    ValidPassword,
                isActive:
                    false);

        var service =
            CreateService(
                passwordHasher);

        // Act
        var result =
            await service.SignInAsync(
                "inspector",
                ValidPassword);

        // Assert
        Assert.False(
            result.Succeeded);

        await using var dbContext =
            await _dbContextFactory
                .CreateDbContextAsync();

        var savedUser =
            await dbContext.Operators
                .SingleAsync(
                    x => x.Id == user.Id);

        Assert.Null(
            savedUser.LastLoginAt);
    }


    // ============================================
    // SuccessRehashNeeded
    // ============================================

    [Fact]
    public async Task SignInAsync_WhenRehashNeeded_UpdatesPasswordHash()
    {
        // Arrange
        await InitializeDatabaseAsync();

        var user =
            new Operator
            {
                LoginId =
                    "inspector",

                NormalizedLoginId =
                    "INSPECTOR",

                DisplayName =
                    "点検担当者1",

                PasswordHash =
                    "old-password-hash",

                Role =
                    OperatorRole.Inspector,

                IsActive =
                    true
            };

        await using (var dbContext =
            await _dbContextFactory
                .CreateDbContextAsync())
        {
            dbContext.Operators.Add(
                user);

            await dbContext.SaveChangesAsync();
        }

        var passwordHasher =
            new RehashNeededPasswordHasher(
                "new-password-hash");

        var service =
            CreateService(
                passwordHasher);

        // Act
        var result =
            await service.SignInAsync(
                "inspector",
                ValidPassword);

        // Assert
        Assert.True(
            result.Succeeded);

        Assert.True(
            passwordHasher.HashPasswordCalled);

        await using var verifyContext =
            await _dbContextFactory
                .CreateDbContextAsync();

        var savedUser =
            await verifyContext.Operators
                .SingleAsync(
                    x => x.Id == user.Id);

        Assert.Equal(
            "new-password-hash",
            savedUser.PasswordHash);
    }


    // ============================================
    // Failed PasswordHasher
    // ============================================

    [Fact]
    public async Task SignInAsync_WhenPasswordVerificationFails_ReturnsFailure()
    {
        // Arrange
        await InitializeDatabaseAsync();

        var user =
            new Operator
            {
                LoginId =
                    "inspector",

                NormalizedLoginId =
                    "INSPECTOR",

                DisplayName =
                    "点検担当者1",

                PasswordHash =
                    "stored-hash",

                Role =
                    OperatorRole.Inspector,

                IsActive =
                    true
            };

        await using (var dbContext =
            await _dbContextFactory
                .CreateDbContextAsync())
        {
            dbContext.Operators.Add(
                user);

            await dbContext.SaveChangesAsync();
        }

        var passwordHasher =
            new FailedPasswordHasher();

        var service =
            CreateService(
                passwordHasher);

        // Act
        var result =
            await service.SignInAsync(
                "inspector",
                ValidPassword);

        // Assert
        Assert.False(
            result.Succeeded);

        Assert.Null(
            result.User);

        Assert.Equal(
            ErrorMessage,
            result.ErrorMessage);
    }


    // ============================================
    // Helpers
    // ============================================

    private AuthenticationService CreateService(
        IPasswordHasher<Operator> passwordHasher)
    {
        return new AuthenticationService(
            _dbContextFactory,
            passwordHasher);
    }


    private async Task InitializeDatabaseAsync()
    {
        await using var dbContext =
            await _dbContextFactory
                .CreateDbContextAsync();

        await dbContext.Database
            .EnsureCreatedAsync();
    }


    private async Task<Operator>
        CreateAndSaveOperatorAsync(
            IPasswordHasher<Operator> passwordHasher,
            string loginId,
            string displayName,
            OperatorRole role,
            string password,
            bool isActive = true)
    {
        await InitializeDatabaseAsync();

        var user =
            new Operator
            {
                LoginId =
                    loginId.Trim(),

                NormalizedLoginId =
                    loginId
                        .Trim()
                        .ToUpperInvariant(),

                DisplayName =
                    displayName,

                Role =
                    role,

                IsActive =
                    isActive
            };

        user.PasswordHash =
            passwordHasher.HashPassword(
                user,
                password);

        await using var dbContext =
            await _dbContextFactory
                .CreateDbContextAsync();

        dbContext.Operators.Add(
            user);

        await dbContext.SaveChangesAsync();

        return user;
    }


    // ============================================
    // Test PasswordHasher
    // ============================================

    private sealed class
        RehashNeededPasswordHasher
        : IPasswordHasher<Operator>
    {
        private readonly string
            _newPasswordHash;


        public RehashNeededPasswordHasher(
            string newPasswordHash)
        {
            _newPasswordHash =
                newPasswordHash;
        }


        public bool HashPasswordCalled
        {
            get;
            private set;
        }


        public string HashPassword(
            Operator user,
            string password)
        {
            HashPasswordCalled =
                true;

            return _newPasswordHash;
        }


        public PasswordVerificationResult
            VerifyHashedPassword(
                Operator user,
                string hashedPassword,
                string providedPassword)
        {
            return
                PasswordVerificationResult
                    .SuccessRehashNeeded;
        }
    }


    private sealed class
        FailedPasswordHasher
        : IPasswordHasher<Operator>
    {
        public string HashPassword(
            Operator user,
            string password)
        {
            return "unused";
        }


        public PasswordVerificationResult
            VerifyHashedPassword(
                Operator user,
                string hashedPassword,
                string providedPassword)
        {
            return
                PasswordVerificationResult
                    .Failed;
        }
    }


    // ============================================
    // Cleanup
    // ============================================

    public void Dispose()
    {
        GC.SuppressFinalize(
            this);

        try
        {
            if (File.Exists(
                _databasePath))
            {
                File.Delete(
                    _databasePath);
            }

            var walPath =
                _databasePath +
                "-wal";

            if (File.Exists(
                walPath))
            {
                File.Delete(
                    walPath);
            }

            var shmPath =
                _databasePath +
                "-shm";

            if (File.Exists(
                shmPath))
            {
                File.Delete(
                    shmPath);
            }
        }
        catch
        {
            // テスト終了時の一時ファイル削除失敗は
            // テスト結果には影響させない。
        }
    }
}
