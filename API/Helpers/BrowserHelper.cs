using API.Constants;
using API.Entities.Enums;

namespace API.Helpers;

/// <summary>
/// Handles all things around Parsing Headers
/// </summary>
public static class BrowserHelper
{
    public static ClientDeviceType DetermineClientType(string userAgent, string? endpoint = null)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            return ClientDeviceType.Unknown;
        }

        var ua = userAgent.ToLowerInvariant();

        // ua contains "web-app" keyword, it's Kavita web app
        if (ua.Contains("web-app")) return ClientDeviceType.WebApp;
        if (ua.Contains("koreader") || ua.Contains("kobo touch")) return ClientDeviceType.KoReader;
        if (ua.Contains("panels")) return ClientDeviceType.Panels;

        // Web browsers
        if (ua.Contains("chrome") || ua.Contains("firefox") ||
            ua.Contains("safari") || ua.Contains("edge"))
        {
            return ClientDeviceType.WebBrowser;
        }

        var ret = ClientDeviceType.Unknown;

        // OPDS clients
        if (ua.Contains("librera"))
        {
            ret = ClientDeviceType.Librera;
        }

        // If this is an opds url and it's not a custom server, then return as generic
        if (!string.IsNullOrEmpty(endpoint) && endpoint.Contains("/opds/") && ret == ClientDeviceType.Unknown)
        {
            ret = ClientDeviceType.OpdsClient;
        }

        return ret;
    }

    public static ClientDevicePlatform DetectPlatform(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            return ClientDevicePlatform.Unknown;
        }

        var ua = userAgent.ToLowerInvariant();

        if (ua.Contains("windows") || ua.Contains("win32") || ua.Contains("win64"))
            return ClientDevicePlatform.Windows;
        if (ua.Contains("macintosh"))
            return ClientDevicePlatform.MacOs;
        if (ua.Contains("linux") && !ua.Contains("android"))
            return ClientDevicePlatform.Linux;
        if (ua.Contains("iphone") || ua.Contains("ipad") || ua.Contains("ipod") || ua.Contains("mac os"))
            return ClientDevicePlatform.Ios;
        if (ua.Contains("android"))
            return ClientDevicePlatform.Android;

        return ClientDevicePlatform.Unknown;
    }
}
