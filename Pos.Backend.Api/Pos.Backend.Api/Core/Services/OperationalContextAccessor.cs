using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Pos.Backend.Api.Core.Models;
using Pos.Backend.Api.Core.Security;
using Pos.Backend.Api.Infrastructure.Data;

namespace Pos.Backend.Api.Core.Services;

public class OperationalContextAccessor : IOperationalContextAccessor
{
    private const string ContextItemKey = "__OperationalContext";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly PosDbContext _dbContext;

    public OperationalContextAccessor(IHttpContextAccessor httpContextAccessor, PosDbContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    public async Task<OperationalContext> GetRequiredContextAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new OperationalContextException("INVALID_CLAIMS", StatusCodes.Status401Unauthorized);

        if (httpContext.Items.TryGetValue(ContextItemKey, out var cached)
            && cached is OperationalContext cachedContext)
        {
            return cachedContext;
        }

        var principal = httpContext.User;
        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        var username = principal.FindFirstValue(AppClaims.Username);
        var companyIdValue = principal.FindFirstValue(AppClaims.CompanyId);
        var establishmentIdValue = principal.FindFirstValue(AppClaims.EstablishmentId);
        var emissionPointIdValue = principal.FindFirstValue(AppClaims.EmissionPointId);

        if (string.IsNullOrWhiteSpace(userIdValue)
            || string.IsNullOrWhiteSpace(username)
            || string.IsNullOrWhiteSpace(companyIdValue)
            || string.IsNullOrWhiteSpace(establishmentIdValue)
            || string.IsNullOrWhiteSpace(emissionPointIdValue)
            || !int.TryParse(userIdValue, out var userId)
            || !int.TryParse(companyIdValue, out var companyId)
            || !int.TryParse(establishmentIdValue, out var establishmentId)
            || !int.TryParse(emissionPointIdValue, out var emissionPointId))
        {
            throw new OperationalContextException("INVALID_CLAIMS", StatusCodes.Status401Unauthorized);
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.IsActive,
                u.CompanyId,
                u.EstablishmentId,
                u.EmissionPointId
            })
            .FirstOrDefaultAsync();

        if (user is null || !user.IsActive)
        {
            throw new OperationalContextException("CONTEXT_MISMATCH", StatusCodes.Status403Forbidden);
        }

        if (!string.Equals(user.Username, username, StringComparison.Ordinal))
        {
            throw new OperationalContextException("CONTEXT_MISMATCH", StatusCodes.Status403Forbidden);
        }

        var company = await _dbContext.Companies
            .AsNoTracking()
            .Where(c => c.Id == companyId)
            .Select(c => new { c.Id, c.IsActive })
            .FirstOrDefaultAsync();

        if (company is null || !company.IsActive)
        {
            throw new OperationalContextException("COMPANY_INACTIVE_OR_NOT_FOUND", StatusCodes.Status401Unauthorized);
        }

        var establishment = await _dbContext.Establishments
            .AsNoTracking()
            .Where(e => e.Id == establishmentId)
            .Select(e => new { e.Id, e.IsActive, e.CompanyId })
            .FirstOrDefaultAsync();

        if (establishment is null || !establishment.IsActive)
        {
            throw new OperationalContextException("ESTABLISHMENT_INACTIVE_OR_NOT_FOUND", StatusCodes.Status401Unauthorized);
        }

        if (establishment.CompanyId != companyId)
        {
            throw new OperationalContextException("CONTEXT_MISMATCH", StatusCodes.Status403Forbidden);
        }

        var emissionPoint = await _dbContext.EmissionPoints
            .AsNoTracking()
            .Where(ep => ep.Id == emissionPointId)
            .Select(ep => new { ep.Id, ep.IsActive, ep.EstablishmentId })
            .FirstOrDefaultAsync();

        if (emissionPoint is null || !emissionPoint.IsActive)
        {
            throw new OperationalContextException("EMISSION_POINT_INACTIVE_OR_NOT_FOUND", StatusCodes.Status401Unauthorized);
        }

        if (emissionPoint.EstablishmentId != establishmentId
            || user.CompanyId != companyId
            || user.EstablishmentId != establishmentId
            || user.EmissionPointId != emissionPointId)
        {
            throw new OperationalContextException("CONTEXT_MISMATCH", StatusCodes.Status403Forbidden);
        }

        var operationalContext = new OperationalContext
        {
            UserId = userId,
            Username = username,
            CompanyId = companyId,
            EstablishmentId = establishmentId,
            EmissionPointId = emissionPointId
        };

        httpContext.Items[ContextItemKey] = operationalContext;
        return operationalContext;
    }
}
