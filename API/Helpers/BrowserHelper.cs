using API.Constants;

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
            return ClientDeviceTypeNames.Unknown;
        }

        var ua = userAgent.ToLowerInvariant();

        if (ua.Contains("koreader") || ua.Contains("kobo touch")) return ClientDeviceTypeNames.KOReader;
        if (ua.Contains("panels")) return ClientDeviceTypeNames.Panels;

        // OPDS clients
        if (ua.Contains("librera")) return ClientDeviceTypeNames.Librera;

        // Web browsers
        if (ua.Contains("chrome") || ua.Contains("firefox") ||
            ua.Contains("safari") || ua.Contains("edge"))
        {
            return ClientDeviceTypeNames.WebBrowser;
        }

        return ClientDeviceTypeNames.Unknown;
    }

    public static string DetectPlatform(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            return ClientDevicePlatformNames.Unknown;
        }

        var ua = userAgent.ToLowerInvariant();

        if (ua.Contains("windows") || ua.Contains("win32") || ua.Contains("win64"))
            return ClientDevicePlatformNames.Windows;
        if (ua.Contains("macintosh"))
            return ClientDevicePlatformNames.MacOs;
        if (ua.Contains("linux") && !ua.Contains("android"))
            return ClientDevicePlatformNames.Linux;
        if (ua.Contains("iphone") || ua.Contains("ipad") || ua.Contains("ipod") || ua.Contains("mac os"))
            return ClientDevicePlatformNames.IOs;
        if (ua.Contains("android"))
            return ClientDevicePlatformNames.Android;

        return ClientDevicePlatformNames.Unknown;
    }
}
