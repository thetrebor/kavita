using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using API.DTOs.Misc;
using API.Services.Reading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace API.Middleware;


/// <summary>
/// Middleware that extracts client information from the HTTP request and makes it
/// available through IClientInfoAccessor for the duration of the request.
/// </summary>
public class ClientInfoMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ClientInfoMiddleware> _logger;

    public ClientInfoMiddleware(RequestDelegate next, ILogger<ClientInfoMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var clientInfo = ExtractClientInfo(context);
            ClientInfoAccessor.SetClientInfo(clientInfo);

            await _next(context);
        }
        finally
        {
            // Clear after request completes
            ClientInfoAccessor.SetClientInfo(null);
        }
    }

    private ClientInfoDto ExtractClientInfo(HttpContext context)
    {
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var kavitaClient = context.Request.Headers["X-Kavita-Client"].ToString();
        var ipAddress = GetClientIpAddress(context);
        var authType = DetermineAuthType(context);

        // If custom Kavita header exists, parse it for rich info
        if (!string.IsNullOrEmpty(kavitaClient))
        {
            var parsed = ParseKavitaClientHeader(kavitaClient, userAgent);
            parsed.IpAddress = ipAddress;
            parsed.AuthType = authType;
            parsed.CapturedAt = DateTime.UtcNow;
            return parsed;
        }

        // Fallback to basic UA parsing
        return new ClientInfoDto
        {
            UserAgent = userAgent,
            IpAddress = ipAddress,
            AuthType = authType,
            ClientType = DetermineClientType(userAgent),
            Platform = DetectPlatform(userAgent),
            CapturedAt = DateTime.UtcNow
        };
    }

    private ClientInfoDto ParseKavitaClientHeader(string header, string fallbackUa)
    {
        try
        {
            // Parse: "web-app/1.2.3 (Chrome/120.0; Windows; Desktop; 1920x1080; landscape)"
            var match = Regex.Match(header,
                @"web-app/([^\s]+) \(([^/]+)/([^;]+); ([^;]+); ([^;]+); (\d+)x(\d+)(?:; ([^\)]+))?\)");

            if (match.Success)
            {
                return new ClientInfoDto
                {
                    ClientType = "Web App",
                    AppVersion = match.Groups[1].Value,
                    Browser = match.Groups[2].Value,
                    BrowserVersion = match.Groups[3].Value,
                    Platform = match.Groups[4].Value,
                    DeviceType = match.Groups[5].Value,
                    ScreenWidth = int.Parse(match.Groups[6].Value),
                    ScreenHeight = int.Parse(match.Groups[7].Value),
                    Orientation = match.Groups.Count > 8 && match.Groups[8].Success
                        ? match.Groups[8].Value
                        : null,
                    UserAgent = fallbackUa
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse X-Kavita-Client header: {Header}", header);
        }

        // Fallback if parsing fails
        return new ClientInfoDto
        {
            UserAgent = fallbackUa,
            ClientType = "Web App"
        };
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Check for X-Forwarded-For header (proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP in the chain
            return forwardedFor.Split(',')[0].Trim();
        }

        // Check for X-Real-IP header
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fallback to direct connection IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private static AuthenticationType DetermineAuthType(HttpContext context)
    {
        // Check if user is authenticated
        if (!context.User?.Identity?.IsAuthenticated ?? true)
        {
            return AuthenticationType.Unknown;
        }

        // Check for API key claim (adjust based on your auth implementation)
        // Check for API key in query string
        if (context.Request.Query.TryGetValue("apiKey", out var apiKeyQuery) &&
            !string.IsNullOrEmpty(apiKeyQuery))
        {
            return AuthenticationType.ApiKey;
        }

        // Check for API key in URL path (e.g., /api/opds/{apiKey}/...)
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.Contains("/api/opds/", StringComparison.OrdinalIgnoreCase))
        {
            // Path format: /api/opds/{apiKey}/...
            // Split and check if there's a segment after /api/opds/
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var opdsIndex = Array.FindIndex(segments, s =>
                s.Equals("opds", StringComparison.OrdinalIgnoreCase));

            if (opdsIndex >= 0 && opdsIndex + 1 < segments.Length)
            {
                // There's a segment after /opds/ which should be the API key
                return AuthenticationType.ApiKey;
            }
        }


        // Check authorization header pattern
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrEmpty(authHeader))
        {
            // If it's a JWT, it will have 3 parts separated by dots
            var token = authHeader.Replace("Bearer ", string.Empty);
            if (token.Split('.').Length == 3)
            {
                return AuthenticationType.JWT;
            }
        }

        // Default to JWT if authenticated but can't determine
        return AuthenticationType.JWT;
    }

    private static string DetermineClientType(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            return "Unknown";
        }

        var ua = userAgent.ToLowerInvariant();

        // Known e-reader applications
        if (ua.Contains("koreader")) return "KOReader";
        if (ua.Contains("tachiyomi")) return "Tachiyomi";
        if (ua.Contains("calibre")) return "Calibre";
        if (ua.Contains("fbreader")) return "FBReader";
        if (ua.Contains("chunky")) return "Chunky";
        if (ua.Contains("panels")) return "Panels";

        // Mobile apps (if you have custom UA strings)
        if (ua.Contains("kavita-mobile")) return "Kavita Mobile";

        // OPDS clients
        if (ua.Contains("opds")) return "OPDS Client";

        // Web browsers
        if (ua.Contains("chrome") || ua.Contains("firefox") ||
            ua.Contains("safari") || ua.Contains("edge"))
        {
            return "Web Browser";
        }

        return "Unknown";
    }

    private static string DetectPlatform(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            return "Unknown";
        }

        var ua = userAgent.ToLowerInvariant();

        if (ua.Contains("windows") || ua.Contains("win32") || ua.Contains("win64"))
            return "Windows";
        if (ua.Contains("macintosh") || ua.Contains("mac os"))
            return "macOS";
        if (ua.Contains("linux") && !ua.Contains("android"))
            return "Linux";
        if (ua.Contains("iphone") || ua.Contains("ipad") || ua.Contains("ipod"))
            return "iOS";
        if (ua.Contains("android"))
            return "Android";

        return "Unknown";
    }
}

