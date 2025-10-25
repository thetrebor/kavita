using System;
using API.Entities.Progress;

namespace API.DTOs.Progress;

public sealed record ClientDeviceDto
{
    /// <summary>
    /// User-friendly name, defaults to generated name like "Chrome on Windows"
    /// </summary>
    public string FriendlyName { get; set; } = string.Empty;

    /// <summary>
    /// Most recent stable ClientInfoData (excluding IP/timestamp changes)
    /// </summary>
    public ClientInfoData CurrentClientInfo { get; set; } = new();

    public DateTime FirstSeenUtc { get; set; }
    public DateTime LastSeenUtc { get; set; }
}
