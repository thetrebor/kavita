using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using API.Data;
using API.DTOs.Progress;
using API.Entities;
using API.Entities.Progress;
using API.Entities.User;
using API.Extensions.QueryExtensions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Services;
#nullable enable

public static class ClientDeviceTypes
{
    public const string WebBrowser = "Web Browser";
}

public interface IClientDeviceService
{
    Task<ClientDevice> IdentifyOrRegisterDeviceAsync(int userId, ClientInfoData clientInfo, string? clientDeviceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClientDevice>> GetUserDevicesAsync(int userId, bool includeInactive = false);
    Task<IEnumerable<ClientDeviceDto>> GetUserDeviceDtosAsync(int userId, bool includeInactive = false);
    Task<IEnumerable<ClientDeviceDto>> GetAllUserDeviceDtos(bool includeInactive = false);
    Task<bool> RenameDeviceAsync(int userId, int deviceId, string newName);
    Task<bool> RemoveDeviceAsync(int userId, int deviceId);
    Task<bool> LogoutDeviceAsync(int userId, int deviceId);
}

public class ClientDeviceService(DataContext context, IMapper mapper, ILogger<ClientDeviceService> logger)
    : IClientDeviceService
{
    /// <summary>
    /// Look for devices up to 30 days ago for matching
    /// </summary>
    private const int DeviceLookupWindowDays = -30;

    public async Task<ClientDevice> IdentifyOrRegisterDeviceAsync(
        int userId,
        ClientInfoData clientInfo,
        string? clientDeviceId,
        CancellationToken cancellationToken = default)
    {
        // STEP 1: Try exact match on ClientDeviceId (if provided)
        if (!string.IsNullOrEmpty(clientDeviceId))
        {
            var deviceByClientId = await context.ClientDevice
                .Include(d => d.History.OrderByDescending(h => h.CapturedAtUtc).Take(1))
                .FirstOrDefaultAsync(d =>
                    d.AppUserId == userId &&
                    d.ClientDeviceId == clientDeviceId &&
                    d.IsActive, cancellationToken: cancellationToken);

            if (deviceByClientId != null)
            {
                await UpdateDeviceActivityAsync(deviceByClientId, clientInfo);
                return deviceByClientId;
            }
        }

        // STEP 2: Try fingerprint matching
        var fingerprint = GenerateDeviceFingerprint(clientInfo);

        var deviceByFingerprint = await context.ClientDevice
            .Include(d => d.History.OrderByDescending(h => h.CapturedAtUtc).Take(1)) // Do I really need to include here?
            .FirstOrDefaultAsync(d =>
                d.AppUserId == userId &&
                d.DeviceFingerprint == fingerprint &&
                d.IsActive, cancellationToken: cancellationToken);

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
            logger.LogDebug("Fuzzy matched device {DeviceId} for user {UserId}", fuzzyMatch.Id, userId);

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
        return await context.ClientDevice
            .Where(d => d.AppUserId == userId)
            .WhereIf(!includeInactive, d => d.IsActive)
            .OrderByDescending(d => d.LastSeenUtc)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClientDeviceDto>> GetUserDeviceDtosAsync(int userId, bool includeInactive = false)
    {
        return await context.ClientDevice
            .Where(d => d.AppUserId == userId)
            .WhereIf(!includeInactive, d => d.IsActive)
            .OrderByDescending(d => d.LastSeenUtc)
            .ProjectTo<ClientDeviceDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClientDeviceDto>> GetAllUserDeviceDtos(bool includeInactive = false)
    {
        return await context.ClientDevice
            .WhereIf(!includeInactive, d => d.IsActive)
            .OrderByDescending(d => d.LastSeenUtc)
            .ProjectTo<ClientDeviceDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<bool> RenameDeviceAsync(int userId, int deviceId, string newName)
    {
        var device = await context.ClientDevice
            .FirstOrDefaultAsync(d => d.Id == deviceId && d.AppUserId == userId);

        if (device == null)
        {
            return false;
        }

        device.FriendlyName = newName;
        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} renamed device {DeviceId} to '{Name}'",
            userId, deviceId, newName);

        return true;
    }

    public async Task<bool> RemoveDeviceAsync(int userId, int deviceId)
    {
        var device = await context.ClientDevice
            .FirstOrDefaultAsync(d => d.Id == deviceId && d.AppUserId == userId);

        if (device == null)
        {
            return false;
        }

        device.IsActive = false;
        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} removed device {DeviceId}", userId, deviceId);

        return true;
    }

    public async Task<bool> LogoutDeviceAsync(int userId, int deviceId)
    {
        // This would integrate with your JWT token management
        // For now, just mark as inactive (user would need to re-authenticate)
        var device = await context.ClientDevice
            .FirstOrDefaultAsync(d => d.Id == deviceId && d.AppUserId == userId);

        if (device == null)
        {
            return false;
        }

        // TODO: Integrate with JWT revocation list or token versioning
        // For example, increment a token version number on the user record
        // and validate tokens against this version

        logger.LogInformation("User {UserId} logged out device {DeviceId}", userId, deviceId);

        return true;
    }


    /// <summary>
    /// Generates a stable fingerprint hash from key ClientInfo attributes.
    /// Used for device matching when ClientDeviceId is not available.
    /// </summary>
    private static string GenerateDeviceFingerprint(ClientInfoData clientInfo)
    {
        var components = new List<string>
        {
            clientInfo.ClientType.ToLowerInvariant(),
            clientInfo.Platform?.ToLowerInvariant() ?? string.Empty,
            clientInfo.DeviceType?.ToLowerInvariant() ?? string.Empty
        };

        // For web browsers, include browser + major version only
        if (clientInfo.ClientType == ClientDeviceTypes.WebBrowser && !string.IsNullOrEmpty(clientInfo.Browser))
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
        var recentDevices = await context.ClientDevice
            .Where(d =>
                d.AppUserId == userId &&
                d.IsActive &&
                d.LastSeenUtc > DateTime.UtcNow.AddDays(DeviceLookupWindowDays))
            .ToListAsync();

        foreach (var device in recentDevices)
        {
            var similarity = CalculateSimilarity(device.CurrentClientInfo, clientInfo);

            // If 80%+ similar, consider it a match
            if (similarity >= 0.8)
            {
                logger.LogInformation(
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
        // Checks are weighted
        var matchCount = 0;
        var totalChecks = 0;

        // Core attributes (weighted heavily)
        if (CompareStrings(existing.ClientType, current.ClientType))
        {
            matchCount += 3;
        }
        totalChecks += 3;

        if (CompareStrings(existing.Platform, current.Platform))
        {
            matchCount += 3;
        }
        totalChecks += 3;

        if (CompareStrings(existing.DeviceType, current.DeviceType))
        {
            matchCount += 2;
        }
        totalChecks += 2;

        // Browser info (less critical, versions change)
        if (CompareStrings(existing.Browser, current.Browser))
        {
            matchCount += 2;
        }
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

        return (double) matchCount / totalChecks;
    }

    private static bool CompareStrings(string? a, string? b)
    {
        if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b)) return true;
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Registers a brand-new device in the database.
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
                new()
                {
                    ClientInfo = clientInfo,
                    CapturedAtUtc = DateTime.UtcNow
                }
            }
        };

        context.ClientDevice.Add(newDevice);
        await context.SaveChangesAsync();

        logger.LogInformation(
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
                 clientInfo.ClientType != ClientDeviceTypes.WebBrowser)
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
    private async Task UpdateDeviceActivityAsync(ClientDevice device, ClientInfoData? newClientInfo)
    {
        device.LastSeenUtc = DateTime.UtcNow;

        if (HasMeaningfulChanges(device.CurrentClientInfo, newClientInfo))
        {
            device.CurrentClientInfo = newClientInfo;
            device.History.Add(new ClientDeviceHistory
            {
                ClientInfo = newClientInfo,
                CapturedAtUtc = DateTime.UtcNow
            });
        }

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Someone else updated the device, that's fine
            // Their LastSeenUtc is equally valid
            logger.LogDebug(ex, "Concurrency conflict updating device {DeviceId}, ignoring", device.Id);

            // Detach to prevent tracking issues
            context.Entry(device).State = EntityState.Detached;
        }
    }

    /// <summary>
    /// Determines if ClientInfo has changed in a meaningful way that warrants history recording.
    /// Ignores volatile attributes like IP address, screen dimensions, timestamp.
    /// </summary>
    private static bool HasMeaningfulChanges(ClientInfoData? existing, ClientInfoData? current)
    {
        if (existing == null && current == null) return false;
        if (existing != null && current == null) return true;

        return existing?.ClientType != current?.ClientType ||
               existing?.Platform != current?.Platform ||
               existing?.DeviceType != current?.DeviceType ||
               existing?.Browser != current?.Browser ||
               GetMajorVersion(existing?.BrowserVersion) != GetMajorVersion(current?.BrowserVersion) ||
               existing?.AppVersion != current?.AppVersion;
    }

    private static string GetMajorVersion(string? version)
    {
        if (string.IsNullOrEmpty(version)) return string.Empty;
        return version.Split('.')[0];
    }
}
