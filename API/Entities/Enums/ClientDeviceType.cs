namespace API.Entities.Enums;

public enum ClientDeviceType
{
    Unknown = 0,
    WebBrowser = 1,
    /// <summary>
    /// WebApp is Kavita's explicit web UI, otherwise a generic browser will be WebBrowser
    /// </summary>
    WebApp = 2,
    KoReader = 3,
    Panels = 4,
    Librera = 5,
    /// <summary>
    /// Generic fallback to anything consuming opds/ url
    /// </summary>
    OpdsClient = 6
}
