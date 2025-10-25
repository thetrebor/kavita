using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using API.Entities.Progress;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Services;
#nullable enable

public interface IClientDeviceService
{
    Task<ClientDevice> IdentifyOrRegisterDeviceAsync(int userId, ClientInfoData? clientInfo, string? clientDeviceId);
    Task<IEnumerable<ClientDevice>> GetUserDevicesAsync(int userId, bool includeInactive = false);
    Task<bool> RenameDeviceAsync(int userId, int deviceId, string newName);
    Task<bool> RemoveDeviceAsync(int userId, int deviceId);
    Task<bool> LogoutDeviceAsync(int userId, int deviceId);
    Task RecordDeviceActivityAsync(int deviceId, ClientInfoData clientInfo);
}

public class ClientDeviceService : IClientDeviceService
{
    private readonly DataContext _context;
    private readonly ILogger<ClientDeviceService> _logger;

    public ClientDeviceService(DataContext context, ILogger<ClientDeviceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ClientDevice> IdentifyOrRegisterDeviceAsync(
        int userId,
        ClientInfoData? clientInfo,
        string? clientDeviceId)
    {
        // STEP 1: Try exact match on ClientDeviceId (if provided)
        if (!string.IsNullOrEmpty(clientDeviceId))
        {
            var deviceByClientId = await _context.ClientDevice
                .Include(d => d.History.OrderByDescending(h => h.CapturedAtUtc).Take(1))
                .FirstOrDefaultAsync(d =>
                    d.AppUserId == userId &&
                    d.ClientDeviceId == clientDeviceId &&
                    d.IsActive);

            if (deviceByClientId != null)
            {
                await UpdateDeviceActivityAsync(deviceByClientId, clientInfo);
                return deviceByClientId;
            }
        }

        // STEP 2: Try fingerprint matching
        var fingerprint = GenerateDeviceFingerprint(clientInfo);

        var deviceByFingerprint = await _context.ClientDevice
            .Include(d => d.History.OrderByDescending(h => h.CapturedAtUtc).Take(1))
            .FirstOrDefaultAsync(d =>
                d.AppUserId == userId &&
                d.DeviceFingerprint == fingerprint &&
                d.IsActive);

        if (deviceByFingerprint != null)
        {
            // If client now provides DeviceId, update the record
            if (!string.IsNullOrEmpty(clientDeviceId) &&
                string.IsNullOrEmpty(deviceByFingerprint.ClientDeviceId))
            {
                deviceByFingerprint.ClientDeviceId = clientDeviceId;
            }

            await UpdateDeviceActivityAsync(deviceByFingerprint, clientInfo);
            return deviceByFingerprint;
        }

        // STEP 3: Fuzzy matching (optional, for edge cases)
        var fuzzyMatch = await TryFuzzyMatchAsync(userId, clientInfo, fingerprint);
        if (fuzzyMatch != null)
        {
            _logger.LogInformation(
                "Fuzzy matched device {DeviceId} for user {UserId}",
                fuzzyMatch.Id, userId);

            if (!string.IsNullOrEmpty(clientDeviceId))
            {
                fuzzyMatch.ClientDeviceId = clientDeviceId;
            }

            await UpdateDeviceActivityAsync(fuzzyMatch, clientInfo);
            return fuzzyMatch;
        }

        // STEP 4: Register new device
        return await RegisterNewDeviceAsync(userId, clientInfo, clientDeviceId, fingerprint);
    }

    public async Task<IEnumerable<ClientDevice>> GetUserDevicesAsync(int userId, bool includeInactive = false)
    {
        var query = _context.ClientDevice
            .Where(d => d.AppUserId == userId);

        if (!includeInactive)
        {
            query = query.Where(d => d.IsActive);
        }

        return await query
            .OrderByDescending(d => d.LastSeenUtc)
            .ToListAsync();
    }

    public async Task<bool> RenameDeviceAsync(int userId, int deviceId, string newName)
    {
        var device = await _context.ClientDevice
            .FirstOrDefaultAsync(d => d.Id == deviceId && d.AppUserId == userId);

        if (device == null)
        {
            return false;
        }

        device.FriendlyName = newName;
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} renamed device {DeviceId} to '{Name}'",
            userId, deviceId, newName);

        return true;
    }

    public async Task<bool> RemoveDeviceAsync(int userId, int deviceId)
    {
        var device = await _context.ClientDevice
            .FirstOrDefaultAsync(d => d.Id == deviceId && d.AppUserId == userId);

        if (device == null)
        {
            return false;
        }

        device.IsActive = false;
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} removed device {DeviceId}", userId, deviceId);

        return true;
    }

    public async Task<bool> LogoutDeviceAsync(int userId, int deviceId)
    {
        // This would integrate with your JWT token management
        // For now, just mark as inactive (user would need to re-authenticate)
        var device = await _context.ClientDevice
            .FirstOrDefaultAsync(d => d.Id == deviceId && d.AppUserId == userId);

        if (device == null)
        {
            return false;
        }

        // TODO: Integrate with JWT revocation list or token versioning
        // For example, increment a token version number on the user record
        // and validate tokens against this version

        _logger.LogInformation("User {UserId} logged out device {DeviceId}", userId, deviceId);

        return true;
    }

    public async Task RecordDeviceActivityAsync(int deviceId, ClientInfoData clientInfo)
    {
        var device = await _context.ClientDevice
            .Include(d => d.History)
            .FirstOrDefaultAsync(d => d.Id == deviceId);

        if (device == null)
        {
            _logger.LogWarning("Attempted to record activity for non-existent device {DeviceId}", deviceId);
            return;
        }

        await UpdateDeviceActivityAsync(device, clientInfo);
    }

    // ========================================================================
    // PRIVATE HELPER METHODS
    // ========================================================================

    /// <summary>
    /// Generates a stable fingerprint hash from key ClientInfo attributes.
    /// Used for device matching when ClientDeviceId is not available.
    /// </summary>
    private static string GenerateDeviceFingerprint(ClientInfoData clientInfo)
    {
        var components = new List<string>
        {
            clientInfo.ClientType.ToLowerInvariant(),
            clientInfo.Platform?.ToLowerInvariant() ?? "unknown",
            clientInfo.DeviceType?.ToLowerInvariant() ?? "unknown"
        };

        // For web browsers, include browser + major version only
        if (clientInfo.ClientType == "Web Browser" && !string.IsNullOrEmpty(clientInfo.Browser))
        {
            components.Add(clientInfo.Browser.ToLowerInvariant());

            // Extract major version only (e.g., "120.0.5481.97" -> "120")
            if (!string.IsNullOrEmpty(clientInfo.BrowserVersion))
            {
                var majorVersion = clientInfo.BrowserVersion.Split('.')[0];
                components.Add(majorVersion);
            }
        }

        var fingerprint = string.Join("|", components);

        // Use SHA256 hash for consistent length and privacy
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(fingerprint));
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Attempts to find a matching device using fuzzy matching logic.
    /// Handles cases like browser version upgrades or minor UserAgent changes.
    /// </summary>
    private async Task<ClientDevice?> TryFuzzyMatchAsync(
        int userId,
        ClientInfoData clientInfo,
        string newFingerprint)
    {
        // Get recent devices (seen in last 30 days) with similar characteristics
        var recentDevices = await _context.ClientDevice
            .Where(d =>
                d.AppUserId == userId &&
                d.IsActive &&
                d.LastSeenUtc > DateTime.UtcNow.AddDays(-30))
            .ToListAsync();

        foreach (var device in recentDevices)
        {
            var similarity = CalculateSimilarity(device.CurrentClientInfo, clientInfo);

            // If 80%+ similar, consider it a match
            if (similarity >= 0.8)
            {
                _logger.LogInformation(
                    "Fuzzy match found with {Similarity:P0} similarity for device {DeviceId}",
                    similarity, device.Id);

                // Update the fingerprint to the new one
                device.DeviceFingerprint = newFingerprint;
                return device;
            }
        }

        return null;
    }

    /// <summary>
    /// Calculates similarity score between two ClientInfoData objects.
    /// Returns value between 0.0 (no match) and 1.0 (perfect match).
    /// </summary>
    private static double CalculateSimilarity(ClientInfoData existing, ClientInfoData current)
    {
        var matchCount = 0;
        var totalChecks = 0;

        // Core attributes (weighted heavily)
        if (CompareStrings(existing.ClientType, current.ClientType)) matchCount += 3;
        totalChecks += 3;

        if (CompareStrings(existing.Platform, current.Platform)) matchCount += 3;
        totalChecks += 3;

        if (CompareStrings(existing.DeviceType, current.DeviceType)) matchCount += 2;
        totalChecks += 2;

        // Browser info (less critical, versions change)
        if (CompareStrings(existing.Browser, current.Browser)) matchCount += 2;
        totalChecks += 2;

        // Browser version - only check major version, allow one version difference
        if (!string.IsNullOrEmpty(existing.BrowserVersion) &&
            !string.IsNullOrEmpty(current.BrowserVersion))
        {
            var existingMajor = int.TryParse(existing.BrowserVersion.Split('.')[0], out var em) ? em : 0;
            var currentMajor = int.TryParse(current.BrowserVersion.Split('.')[0], out var cm) ? cm : 0;

            // Allow one major version difference
            if (Math.Abs(existingMajor - currentMajor) <= 1)
            {
                matchCount += 1;
            }
            totalChecks += 1;
        }

        return totalChecks > 0 ? (double)matchCount / totalChecks : 0.0;
    }

    private static bool CompareStrings(string? a, string? b)
    {
        if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b)) return true;
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Registers a brand new device in the database.
    /// </summary>
    private async Task<ClientDevice> RegisterNewDeviceAsync(
        int userId,
        ClientInfoData clientInfo,
        string? clientDeviceId,
        string fingerprint)
    {
        var friendlyName = GenerateFriendlyName(clientInfo);

        var newDevice = new ClientDevice
        {
            AppUserId = userId,
            ClientDeviceId = clientDeviceId,
            DeviceFingerprint = fingerprint,
            FriendlyName = friendlyName,
            CurrentClientInfo = clientInfo,
            FirstSeenUtc = DateTime.UtcNow,
            LastSeenUtc = DateTime.UtcNow,
            IsActive = true,
            History = new List<ClientDeviceHistory>
            {
                new ClientDeviceHistory
                {
                    ClientInfo = clientInfo,
                    CapturedAtUtc = DateTime.UtcNow
                }
            }
        };

        _context.ClientDevice.Add(newDevice);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Registered new device {DeviceId} '{Name}' for user {UserId}",
            newDevice.Id, friendlyName, userId);

        return newDevice;
    }

    /// <summary>
    /// Generates a user-friendly device name from ClientInfo.
    /// Examples: "Chrome on Windows", "Safari on iOS", "KOReader on Android"
    /// </summary>
    private static string GenerateFriendlyName(ClientInfoData clientInfo)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(clientInfo.Browser))
        {
            parts.Add(clientInfo.Browser);
        }
        else if (!string.IsNullOrEmpty(clientInfo.ClientType) &&
                 clientInfo.ClientType != "Web Browser")
        {
            parts.Add(clientInfo.ClientType);
        }
        else
        {
            parts.Add("Unknown Client");
        }

        if (!string.IsNullOrEmpty(clientInfo.Platform))
        {
            parts.Add("on");
            parts.Add(clientInfo.Platform);
        }

        return string.Join(" ", parts);
    }

    /// <summary>
    /// Updates device's last seen timestamp and records ClientInfo changes in history.
    /// </summary>
    private async Task UpdateDeviceActivityAsync(ClientDevice device, ClientInfoData newClientInfo)
    {
        device.LastSeenUtc = DateTime.UtcNow;

        // Check if ClientInfo has meaningfully changed
        if (HasMeaningfulChanges(device.CurrentClientInfo, newClientInfo))
        {
            device.CurrentClientInfo = newClientInfo;

            // Record in history
            device.History.Add(new ClientDeviceHistory
            {
                ClientInfo = newClientInfo,
                CapturedAtUtc = DateTime.UtcNow
            });

            _logger.LogDebug(
                "Recorded ClientInfo change for device {DeviceId}",
                device.Id);
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Determines if ClientInfo has changed in a meaningful way that warrants history recording.
    /// Ignores volatile attributes like IP address, screen dimensions, timestamp.
    /// </summary>
    private static bool HasMeaningfulChanges(ClientInfoData existing, ClientInfoData current)
    {
        return existing.ClientType != current.ClientType ||
               existing.Platform != current.Platform ||
               existing.DeviceType != current.DeviceType ||
               existing.Browser != current.Browser ||
               GetMajorVersion(existing.BrowserVersion) != GetMajorVersion(current.BrowserVersion) ||
               existing.AppVersion != current.AppVersion;
    }

    private static string GetMajorVersion(string? version)
    {
        if (string.IsNullOrEmpty(version)) return string.Empty;
        return version.Split('.')[0];
    }
}
