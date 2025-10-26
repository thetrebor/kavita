using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using API.Constants;
using API.Entities.Progress;
using API.Services.Reading;
using API.Services.Store;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace API.Middleware;


/// <summary>
/// Middleware that extracts client information from the HTTP request and makes it
/// available through IClientInfoAccessor for the duration of the request.
/// </summary>
public partial class ClientInfoMiddleware(RequestDelegate next, ILogger<ClientInfoMiddleware> logger)
{
    /// <summary>
    /// Web App name (will be localized for UI)
    /// </summary>
    private const string WebAppName = "web-app";
    public async Task InvokeAsync(HttpContext context, IUserContext userContext)
    {
        var clientInfo = ExtractClientInfo(context, userContext);
        var clientDeviceId = context.Request.Headers[Headers.DeviceId].ToString();

        ClientInfoAccessor.SetClientInfo(clientInfo);
        ClientInfoAccessor.SetClientDeviceId(clientDeviceId);

        await next(context);
    }

    private ClientInfoData ExtractClientInfo(HttpContext context, IUserContext userContext)
    {
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var kavitaClient = context.Request.Headers[Headers.KavitaClient].ToString();
        var ipAddress = GetClientIpAddress(context);
        var authType = userContext.GetAuthenticationType();

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
        return new ClientInfoData
        {
            UserAgent = userAgent,
            IpAddress = ipAddress,
            AuthType = authType,
            ClientType = DetermineClientType(userAgent),
            Platform = DetectPlatform(userAgent),
            CapturedAt = DateTime.UtcNow
        };
    }

    private ClientInfoData ParseKavitaClientHeader(string header, string fallbackUa)
    {
        try
        {
            // Parse: "web-app/1.2.3 (Chrome/120.0; Windows; Desktop; 1920x1080; landscape)"
            var match = UserAgentRegex().Match(header);

            if (match.Success)
            {
                return new ClientInfoData
                {
                    ClientType = WebAppName,
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
            logger.LogWarning(ex, "Failed to parse X-Kavita-Client header: {Header}", header);
        }

        // Fallback if parsing fails
        return new ClientInfoData
        {
            UserAgent = fallbackUa,
            ClientType = WebAppName
        };
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Check for X-Forwarded-For header (proxy/load balancer)
        var forwardedFor = context.Request.Headers[Headers.ForwardedFor].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP in the chain
            return forwardedFor.Split(',')[0].Trim();
        }

        // Check for X-Real-IP header
        var realIp = context.Request.Headers[Headers.RealIp].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fallback to direct connection IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }


    // TODO: Move this into a Helper and add unit tests
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

    [GeneratedRegex(@"web-app/([^\s]+) \(([^/]+)/([^;]+); ([^;]+); ([^;]+); (\d+)x(\d+)(?:; ([^\)]+))?\)")]
    private static partial Regex UserAgentRegex();
}

