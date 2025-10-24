using System.Threading;
using API.DTOs.Misc;

namespace API.Services.Reading;
#nullable enable

/// <summary>
/// Provides access to client information for the current request.
/// This service captures details about the client making the request including
/// browser info, device type, authentication method, etc.
/// </summary>
public interface IClientInfoAccessor
{
    /// <summary>
    /// Gets the client information for the current request.
    /// Returns null if called outside an HTTP request context (e.g., background jobs).
    /// </summary>
    ClientInfoDto? Current { get; }
}

/// <summary>
/// Thread-safe accessor for client information using AsyncLocal storage.
/// Client info is set by middleware at the start of each request and automatically
/// cleared when the request completes.
/// </summary>
public class ClientInfoAccessor : IClientInfoAccessor
{
    private static readonly AsyncLocal<ClientInfoDto?> ClientInfo = new();

    public ClientInfoDto? Current => ClientInfo.Value;

    /// <summary>
    /// Sets the client info for the current async context.
    /// Should only be called by middleware.
    /// </summary>
    internal static void SetClientInfo(ClientInfoDto? info)
    {
        ClientInfo.Value = info;
    }
}
