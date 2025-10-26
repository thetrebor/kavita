namespace API.Helpers;

/// <summary>
/// Handles all things around Parsing Headers
/// </summary>
public static class BrowserHelper
{
    public static string DetermineClientType(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            return "Unknown";
        }

        var ua = userAgent.ToLowerInvariant();

        if (ua.Contains("koreader") || ua.Contains("kobo touch")) return "KOReader";
        if (ua.Contains("panels")) return "Panels";

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

    public static string DetectPlatform(string userAgent)
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
