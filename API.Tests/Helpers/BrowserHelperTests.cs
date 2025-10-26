using API.Constants;
using API.Helpers;
using Xunit;

namespace API.Tests.Helpers;

public class BrowserHelperTests
{
    #region DetermineClientType Tests

    [Theory]
    [InlineData("", ClientDeviceTypeNames.Unknown)]
    [InlineData(null, ClientDeviceTypeNames.Unknown)]
    public void DetermineClientType_ReturnsUnknown_ForEmptyOrNullUserAgent(string userAgent, string expected)
    {
        var result = BrowserHelper.DetermineClientType(userAgent);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (Linux; Android 11; Pixel 5) KOReader/2023.10", ClientDeviceTypeNames.KOReader)]
    [InlineData("koreader/1.0", ClientDeviceTypeNames.KOReader)]
    [InlineData("KOREADER", ClientDeviceTypeNames.KOReader)]
    [InlineData("Mozilla/5.0 (Linux; U; Android 2.0; en-us;) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1 (Kobo Touch)", ClientDeviceTypeNames.KOReader)]
    public void DetermineClientType_ReturnsKOReader_ForKOReaderUserAgents(string userAgent, string expected)
    {
        var result = BrowserHelper.DetermineClientType(userAgent);
        Assert.Equal(expected, result);
    }


    [Theory]
    [InlineData("Panels/2.0", ClientDeviceTypeNames.Panels)]
    [InlineData("panels", ClientDeviceTypeNames.Panels)]
    public void DetermineClientType_ReturnsPanels_ForPanelsUserAgents(string userAgent, string expected)
    {
        var result = BrowserHelper.DetermineClientType(userAgent);
        Assert.Equal(expected, result);
    }


    [Theory]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36")]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:121.0) Gecko/20100101 Firefox/121.0")]
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.2 Safari/605.1.15")]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0")]
    public void DetermineClientType_ReturnsWebBrowser_ForBrowserUserAgents(string userAgent)
    {
        var result = BrowserHelper.DetermineClientType(userAgent);
        Assert.Equal(ClientDeviceTypeNames.WebBrowser, result);
    }

    [Theory]
    [InlineData("curl/7.68.0")]
    [InlineData("PostmanRuntime/7.32.3")]
    [InlineData("SomeCustomClient/1.0")]
    public void DetermineClientType_ReturnsUnknown_ForUnrecognizedUserAgents(string userAgent)
    {
        var result = BrowserHelper.DetermineClientType(userAgent);
        Assert.Equal(ClientDeviceTypeNames.Unknown, result);
    }

    [Fact]
    public void DetermineClientType_PrioritizesSpecificClients_OverWebBrowser()
    {
        // KOReader running on Android with Chrome-like UA
        var userAgent = "Mozilla/5.0 (Linux; Android 11) AppleWebKit/537.36 Chrome/91.0 KOReader/2023.10";
        var result = BrowserHelper.DetermineClientType(userAgent);
        Assert.Equal(ClientDeviceTypeNames.KOReader, result);
    }

    #endregion

    #region DetectPlatform Tests

    [Theory]
    [InlineData("", ClientDevicePlatformNames.Unknown)]
    [InlineData(null, ClientDevicePlatformNames.Unknown)]
    public void DetectPlatform_ReturnsUnknown_ForEmptyOrNullUserAgent(string userAgent, string expected)
    {
        var result = BrowserHelper.DetectPlatform(userAgent);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36", ClientDevicePlatformNames.Windows)]
    [InlineData("Mozilla/5.0 (Windows NT 6.1; WOW64)", ClientDevicePlatformNames.Windows)]
    [InlineData("Mozilla/5.0 (win32)", ClientDevicePlatformNames.Windows)]
    [InlineData("Mozilla/5.0 (win64)", ClientDevicePlatformNames.Windows)]
    [InlineData("WINDOWS", ClientDevicePlatformNames.Windows)]
    public void DetectPlatform_ReturnsWindows_ForWindowsUserAgents(string userAgent, string expected)
    {
        var result = BrowserHelper.DetectPlatform(userAgent);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15", ClientDevicePlatformNames.MacOs)]
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 11_6) AppleWebKit/537.36", ClientDevicePlatformNames.MacOs)]
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/26.0 Safari/605.1.15", ClientDevicePlatformNames.MacOs)]
    [InlineData("macintosh", ClientDevicePlatformNames.MacOs)]
    public void DetectPlatform_ReturnsMacOS_ForMacOSUserAgents(string userAgent, string expected)
    {
        var result = BrowserHelper.DetectPlatform(userAgent);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36", ClientDevicePlatformNames.Linux)]
    [InlineData("Mozilla/5.0 (X11; Ubuntu; Linux x86_64)", ClientDevicePlatformNames.Linux)]
    [InlineData("linux", ClientDevicePlatformNames.Linux)]
    public void DetectPlatform_ReturnsLinux_ForLinuxUserAgents(string userAgent, string expected)
    {
        var result = BrowserHelper.DetectPlatform(userAgent);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 17_2 like Mac OS X) AppleWebKit/605.1.15", ClientDevicePlatformNames.IOs)]
    [InlineData("Mozilla/5.0 (iPad; CPU OS 17_2 like Mac OS X) AppleWebKit/605.1.15", ClientDevicePlatformNames.IOs)]
    [InlineData("Mozilla/5.0 (iPod touch; CPU iPhone OS 12_0 like Mac OS X)", ClientDevicePlatformNames.IOs)]
    [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 18_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/26.0 Mobile/15E148 Safari/604.1", ClientDevicePlatformNames.IOs)]
    [InlineData("Mozilla/5.0 (iPad; CPU OS 18_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/26.0 Mobile/15E148 Safari/604.1", ClientDevicePlatformNames.IOs)]
    [InlineData("iphone", ClientDevicePlatformNames.IOs)]
    [InlineData("ipad", ClientDevicePlatformNames.IOs)]
    [InlineData("ipod", ClientDevicePlatformNames.IOs)]
    [InlineData("mac os", ClientDevicePlatformNames.IOs)]
    public void DetectPlatform_ReturnsIOS_ForIOSUserAgents(string userAgent, string expected)
    {
        var result = BrowserHelper.DetectPlatform(userAgent);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (Linux; Android 13; Pixel 7) AppleWebKit/537.36", ClientDevicePlatformNames.Android)]
    [InlineData("Mozilla/5.0 (Linux; Android 11; SM-G991B)", ClientDevicePlatformNames.Android)]
    [InlineData("android", ClientDevicePlatformNames.Android)]
    public void DetectPlatform_ReturnsAndroid_ForAndroidUserAgents(string userAgent, string expected)
    {
        var result = BrowserHelper.DetectPlatform(userAgent);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void DetectPlatform_ReturnsAndroid_NotLinux_ForAndroidUserAgents()
    {
        // Android UAs contain "Linux" but should be detected as Android
        const string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 5) AppleWebKit/537.36";
        var result = BrowserHelper.DetectPlatform(userAgent);
        Assert.Equal(ClientDevicePlatformNames.Android, result);
    }

    [Theory]
    [InlineData("curl/7.68.0")]
    [InlineData("PostmanRuntime/7.32.3")]
    [InlineData("FreeBSD")]
    [InlineData("OpenBSD")]
    public void DetectPlatform_ReturnsUnknown_ForUnrecognizedPlatforms(string userAgent)
    {
        var result = BrowserHelper.DetectPlatform(userAgent);
        Assert.Equal(ClientDevicePlatformNames.Unknown, result);
    }

    [Fact]
    public void DetectPlatform_IsCaseInsensitive()
    {
        Assert.Equal(ClientDevicePlatformNames.Windows, BrowserHelper.DetectPlatform("WINDOWS NT 10.0"));
        Assert.Equal(ClientDevicePlatformNames.Android, BrowserHelper.DetectPlatform("ANDROID 13"));
        Assert.Equal(ClientDevicePlatformNames.MacOs, BrowserHelper.DetectPlatform("MACINTOSH; INTEL MAC OS X"));
        Assert.Equal(ClientDevicePlatformNames.IOs, BrowserHelper.DetectPlatform("IPHONE"));
        Assert.Equal(ClientDevicePlatformNames.Linux, BrowserHelper.DetectPlatform("LINUX X86_64"));
    }

    #endregion

    #region Real-World User Agent Tests

    [Theory]
    [InlineData(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
        ClientDeviceTypeNames.WebBrowser,
        ClientDevicePlatformNames.Windows)]
    [InlineData(
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
        ClientDeviceTypeNames.WebBrowser,
        ClientDevicePlatformNames.MacOs)]
    [InlineData(
        "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
        ClientDeviceTypeNames.WebBrowser,
        ClientDevicePlatformNames.Linux)]
    [InlineData(
        "Mozilla/5.0 (iPhone; CPU iPhone OS 17_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.2 Mobile/15E148 Safari/604.1",
        ClientDeviceTypeNames.WebBrowser,
        ClientDevicePlatformNames.IOs)]
    [InlineData(
        "Mozilla/5.0 (Linux; Android 13; Pixel 7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36",
        ClientDeviceTypeNames.WebBrowser,
        ClientDevicePlatformNames.Android)]
    [InlineData(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:121.0) Gecko/20100101 Firefox/121.0",
        ClientDeviceTypeNames.WebBrowser,
        ClientDevicePlatformNames.Windows)]
    [InlineData(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0",
        ClientDeviceTypeNames.WebBrowser,
        ClientDevicePlatformNames.Windows)]

    [InlineData(
        "Mozilla/5.0 (X11; Linux x86_64; Librera) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.2997.-1000651005 Safari/537.36",
        ClientDeviceTypeNames.Librera,
        ClientDevicePlatformNames.Linux)]
    public void RealWorld_UserAgents_AreDetectedCorrectly(string userAgent, string expectedClientType, string expectedPlatform)
    {
        var clientType = BrowserHelper.DetermineClientType(userAgent);
        var platform = BrowserHelper.DetectPlatform(userAgent);

        Assert.Equal(expectedClientType, clientType);
        Assert.Equal(expectedPlatform, platform);
    }

    #endregion
}
