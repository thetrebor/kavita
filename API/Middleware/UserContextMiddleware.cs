using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.Entities.Progress;
using API.Services;
using API.Services.Store;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace API.Middleware;

/// <summary>
/// Middleware that resolves user identity from various authentication methods
/// (JWT, API Key, OIDC) and provides a unified IUserContext for downstream components.
/// Must run after UseAuthentication() and UseAuthorization().
/// </summary>
public class UserContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserContextMiddleware> _logger;

    public UserContextMiddleware(RequestDelegate next, ILogger<UserContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        UserContext userContext,  // Scoped service
        IUnitOfWork unitOfWork)
    {
        try
        {
            // Clear any previous context (shouldn't be necessary, but defensive)
            userContext.Clear();

            // Check if endpoint allows anonymous access
            var endpoint = context.GetEndpoint();
            var allowAnonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null;

            // ALWAYS attempt to resolve user identity, regardless of [AllowAnonymous]
            var (userId, username, authType) = await ResolveUserIdentityAsync(context, unitOfWork);

            if (userId.HasValue)
            {
                // Successfully resolved user
                userContext.SetUserContext(userId.Value, username!, authType);

                _logger.LogTrace(
                    "Resolved user context: UserId={UserId}, Username={Username}, AuthType={AuthType}",
                    userId, username, authType);
            }
            else if (!allowAnonymous)
            {
                // No user resolved on a protected endpoint - this is a problem
                // Authorization middleware will handle returning 401/403
                _logger.LogWarning(
                    "Could not resolve user identity for protected endpoint: {Path}",
                    context.Request.Path);
            }
            else
            {
                // No user resolved but endpoint allows anonymous - this is fine
                _logger.LogTrace(
                    "No user identity resolved for anonymous endpoint: {Path}",
                    context.Request.Path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving user context");
            // Don't break the pipeline, let authorization handle it
        }

        await _next(context);
    }

    private async Task<(int? userId, string? username, AuthenticationType authType)> ResolveUserIdentityAsync(
        HttpContext context,
        IUnitOfWork unitOfWork)
    {
        // Priority 1: Check for API Key (query string or path parameter)
        var apiKeyResult = await TryResolveFromApiKeyAsync(context, unitOfWork);
        if (apiKeyResult.userId.HasValue)
        {
            return apiKeyResult;
        }

        // Priority 2: Check for JWT or OIDC claims
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            return ResolveFromClaims(context);
        }

        return (null, null, AuthenticationType.Unknown);
    }

    private async Task<(int? userId, string? username, AuthenticationType authType)> TryResolveFromApiKeyAsync(
        HttpContext context,
        IUnitOfWork unitOfWork)
    {
        string? apiKey = null;

        // Check query string
        if (context.Request.Query.TryGetValue("apiKey", out var apiKeyQuery))
        {
            apiKey = apiKeyQuery.ToString();
        }

        // Check path for OPDS endpoints: /api/opds/{apiKey}/...
        if (string.IsNullOrEmpty(apiKey))
        {
            var path = context.Request.Path.Value ?? string.Empty;
            if (path.Contains("/api/opds/", StringComparison.OrdinalIgnoreCase))
            {
                var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var opdsIndex = Array.FindIndex(segments, s =>
                    s.Equals("opds", StringComparison.OrdinalIgnoreCase));

                if (opdsIndex >= 0 && opdsIndex + 1 < segments.Length)
                {
                    apiKey = segments[opdsIndex + 1];
                }
            }
        }

        if (string.IsNullOrEmpty(apiKey))
        {
            return (null, null, AuthenticationType.Unknown);
        }

        try
        {
            var userId = await unitOfWork.UserRepository.GetUserIdByApiKeyAsync(apiKey);
            if (userId > 0)
            {
                // Get username for the API key user
                var user = await unitOfWork.UserRepository.GetUserByIdAsync(userId);
                return (userId, user?.UserName, AuthenticationType.ApiKey);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve user from API key");
        }

        return (null, null, AuthenticationType.Unknown);
    }

    private (int? userId, string? username, AuthenticationType authType) ResolveFromClaims(HttpContext context)
    {
        var claims = context.User!;

        // Check if OIDC authentication
        if (context.Request.Cookies.ContainsKey(OidcService.CookieName))
        {
            var userId = TryGetUserIdFromClaim(claims, ClaimTypes.NameIdentifier);
            var username = claims.FindFirst(JwtRegisteredClaimNames.Name)?.Value;

            return (userId, username, AuthenticationType.OIDC);
        }

        // JWT authentication
        var jwtUserId = TryGetUserIdFromClaim(claims, ClaimTypes.NameIdentifier);
        var jwtUsername = claims.FindFirst(JwtRegisteredClaimNames.Name)?.Value;

        return (jwtUserId, jwtUsername, AuthenticationType.JWT);
    }

    private int? TryGetUserIdFromClaim(ClaimsPrincipal claims, string claimType)
    {
        var claim = claims.FindFirst(claimType);
        if (claim != null && int.TryParse(claim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }
}
