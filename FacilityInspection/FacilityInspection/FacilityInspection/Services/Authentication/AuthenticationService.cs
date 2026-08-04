using FacilityInspection.Data;
using FacilityInspection.Domain.Operators;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FacilityInspection.Services.Authentication;

public sealed class AuthenticationService(
    IDbContextFactory<InspectionDbContext> dbContextFactory,
    IPasswordHasher<Operator> passwordHasher)
    : IAuthenticationService
{
    public async Task<AuthenticationResult> SignInAsync(
        string loginId,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedLoginId = loginId
            .Trim()
            .ToUpperInvariant();

        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var user = await dbContext.Operators
            .SingleOrDefaultAsync(
                x => x.NormalizedLoginId == normalizedLoginId,
                cancellationToken);

        if (user is null || !user.IsActive)
        {
            return AuthenticationResult.Failure(
                "ログインIDまたはパスワードが正しくありません。");
        }

        var verificationResult =
            passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password);

        if (verificationResult ==
            PasswordVerificationResult.Failed)
        {
            return AuthenticationResult.Failure(
                "ログインIDまたはパスワードが正しくありません。");
        }

        if (verificationResult ==
            PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash =
                passwordHasher.HashPassword(user, password);
        }

        user.RecordLogin(DateTimeOffset.Now);

        await dbContext.SaveChangesAsync(cancellationToken);

        var signedInUser = new SignedInOperator(
            user.Id,
            user.LoginId,
            user.DisplayName,
            user.Role);

        return AuthenticationResult.Success(signedInUser);
    }
}