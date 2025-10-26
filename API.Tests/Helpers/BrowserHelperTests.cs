using API.Helpers;
using Xunit;

namespace API.Tests.Helpers;

public class BrowserHelperTests
{
    #region DetermineClientType Tests

    [Theory]
    [InlineData("", "Unknown")]
    [InlineData(null, "Unknown")]
    public void DetermineClientType_ReturnsUnknown_ForEmptyOrNullUserAgent(string userAgent, string expected)
    {
        var result = BrowserHelper.DetermineClientType(userAgent);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (Linux; Android 11; Pixel 5) KOReader/2023.10", "KOReader")]
    [InlineData("koreader/1.0", "KOReader")]
    [InlineData("KOREADER", "KOReader")]
    [InlineData("Mozilla/5.0 (Linux; U; Android 2.0; en-us;) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1 (Kobo Touch)", "KOReader")]
    public void DetermineClientType_ReturnsKOReader_ForKOReaderUserAgents(string userAgent, string expected)
    {
        var result = BrowserHelper.DetermineClientType(userAgent);
        Assert.Equal(expected, result);
    }


    [Theory]
    [InlineData("Panels/2.0", "Panels")]
    [InlineData("panels", "Panels")]
    public void DetermineClientType_ReturnsPanels_ForPanelsUserAgents(string userAgent, string expected)
    {
        var result = BrowserHelper.DetermineClientType(userAgent);
        Assert.Equal(expected, result);
    }


    [Theory]
    [InlineData("OPDS Catalog/1.0", "OPDS Client")]
    [InlineData("opds reader", "OPDS Client")]
    public void DetermineClientType_ReturnsOPDSClient_ForOPDSUserAgents(string userAgent, string expected)
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
        Assert.Equal("Web Browser", result);
    }

    [Theory]
    [InlineData("curl/7.68.0")]
    [InlineData("PostmanRuntime/7.32.3")]
    [InlineData("SomeCustomClient/1.0")]
    public void DetermineClientType_ReturnsUnknown_ForUnrecognizedUserAgents(string userAgent)
    {
        var result = BrowserHelper.DetermineClientType(userAgent);
        Assert.Equal("Unknown", result);
    }

    [Fact]
    public void DetermineClientType_PrioritizesSpecificClients_OverWebBrowser()
    {
        // KOReader running on Android with Chrome-like UA
        var userAgent = "Mozilla/5.0 (Linux; Android 11) AppleWebKit/537.36 Chrome/91.0 KOReader/2023.10";
        var result = BrowserHelper.DetermineClientType(userAgent);
        Assert.Equal("KOReader", result);
    }

    #endregion

    #region DetectPlatform Tests

    [Theory]
    [InlineData("", "Unknown")]
    [InlineData(null, "Unknown")]
    public void DetectPlatform_ReturnsUnknown_ForEmptyOrNullUserAgent(string userAgent, string expected)
    {
        var result = BrowserHelper.DetectPlatform(userAgent);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36", "Windows")]
    [InlineData("Mozilla/5.0 (Windows NT 6.1; WOW64)", "Windows")]
    [InlineData("Mozilla/5.0 (win32)", "Windows")]
    [InlineData("Mozilla/5.0 (win64)", "Windows")]
    [InlineData("WINDOWS", "Windows")]
    public void DetectPlatform_ReturnsWindows_ForWindowsUserAgents(string userAgent, string expected)
    {
        var result = BrowserHelper.DetectPlatform(userAgent);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15", "macOS")]
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 11_6) AppleWebKit/537.36", "macOS")]
    [InlineData("macintosh", "macOS")]
    [InlineData("mac os", "macOS")]
    public void DetectPlatform_ReturnsMacOS_ForMacOSUserAgents(string userAgent, string expected)
    {
        var result = BrowserHelper.DetectPlatform(userAgent);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36", "Linux")]
    [InlineData("Mozilla/5.0 (X11; Ubuntu; Linux x86_64)", "Linux")]
    [InlineData("linux", "Linux")]
    public void DetectPlatform_ReturnsLinux_ForLinuxUserAgents(string userAgent, string expected)
    {
        var result = BrowserHelper.DetectPlatform(userAgent);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 17_2 like Mac OS X) AppleWebKit/605.1.15", "macOS")]
    [InlineData("Mozilla/5.0 (iPad; CPU OS 17_2 like Mac OS X) AppleWebKit/605.1.15", "macOS")]
    [InlineData("Mozilla/5.0 (iPod touch; CPU iPhone OS 12_0 like Mac OS X)", "macOS")]
    [InlineData("iphone", "iOS")]
    [InlineData("ipad", "iOS")]
    [InlineData("ipod", "iOS")]
    public void DetectPlatform_ReturnsIOS_ForIOSUserAgents(string userAgent, string expected)
    {
        var result = BrowserHelper.DetectPlatform(userAgent);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (Linux; Android 13; Pixel 7) AppleWebKit/537.36", "Android")]
    [InlineData("Mozilla/5.0 (Linux; Android 11; SM-G991B)", "Android")]
    [InlineData("android", "Android")]
    public void DetectPlatform_ReturnsAndroid_ForAndroidUserAgents(string userAgent, string expected)
    {
        var result = BrowserHelper.DetectPlatform(userAgent);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void DetectPlatform_ReturnsAndroid_NotLinux_ForAndroidUserAgents()
    {
        // Android UAs contain "Linux" but should be detected as Android
        var userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 5) AppleWebKit/537.36";
        var result = BrowserHelper.DetectPlatform(userAgent);
        Assert.Equal("Android", result);
    }

    [Theory]
    [InlineData("curl/7.68.0")]
    [InlineData("PostmanRuntime/7.32.3")]
    [InlineData("FreeBSD")]
    [InlineData("OpenBSD")]
    public void DetectPlatform_ReturnsUnknown_ForUnrecognizedPlatforms(string userAgent)
    {
        var result = BrowserHelper.DetectPlatform(userAgent);
        Assert.Equal("Unknown", result);
    }

    [Fact]
    public void DetectPlatform_IsCaseInsensitive()
    {
        Assert.Equal("Windows", BrowserHelper.DetectPlatform("WINDOWS NT 10.0"));
        Assert.Equal("Android", BrowserHelper.DetectPlatform("ANDROID 13"));
        Assert.Equal("macOS", BrowserHelper.DetectPlatform("MACINTOSH; INTEL MAC OS X"));
        Assert.Equal("iOS", BrowserHelper.DetectPlatform("IPHONE"));
        Assert.Equal("Linux", BrowserHelper.DetectPlatform("LINUX X86_64"));
    }

    #endregion

    #region Real-World User Agent Tests

    [Theory]
    [InlineData(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
        "Web Browser",
        "Windows")]
    [InlineData(
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
        "Web Browser",
        "macOS")]
    [InlineData(
        "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
        "Web Browser",
        "Linux")]
    [InlineData(
        "Mozilla/5.0 (iPhone; CPU iPhone OS 17_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.2 Mobile/15E148 Safari/604.1",
        "Web Browser",
        "macOS")]
    [InlineData(
        "Mozilla/5.0 (Linux; Android 13; Pixel 7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36",
        "Web Browser",
        "Android")]
    [InlineData(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:121.0) Gecko/20100101 Firefox/121.0",
        "Web Browser",
        "Windows")]
    [InlineData(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0",
        "Web Browser",
        "Windows")]
    public void RealWorld_UserAgents_AreDetectedCorrectly(string userAgent, string expectedClientType, string expectedPlatform)
    {
        var clientType = BrowserHelper.DetermineClientType(userAgent);
        var platform = BrowserHelper.DetectPlatform(userAgent);

        Assert.Equal(expectedClientType, clientType);
        Assert.Equal(expectedPlatform, platform);
    }

    #endregion
}
