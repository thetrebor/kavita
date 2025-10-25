using System;
using System.Collections.Generic;
using API.Entities.Progress;

namespace API.Entities;
#nullable enable

public class ClientDevice
{
    public int Id { get; set; }
    public int AppUserId { get; set; }

    /// <summary>
    /// Client-provided device identifier (from X-Device-Id header)
    /// Null for clients that don't send it (OPDS readers, etc.)
    /// </summary>
    public string? ClientDeviceId { get; set; }

    /// <summary>
    /// Server-computed fingerprint for device matching
    /// Hash of: ClientType + Platform + DeviceType + (Browser+BrowserVersion for web)
    /// </summary>
    public string DeviceFingerprint { get; set; } = string.Empty;

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

    /// <summary>
    /// Soft delete - removed devices stay in DB for history
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// For future: bind reading profile to device
    /// </summary>
    //public int? ReadingProfileId { get; set; }

    // Navigation properties
    public virtual AppUser AppUser { get; set; } = null!;
    public ICollection<ClientDeviceHistory> History { get; set; } = [];
}
