using System;
using System.Threading.Tasks;
using API.Services;
using API.Services.Reading;
using API.Services.Store;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace API.Middleware;

/// <summary>
/// Middleware that identifies and tracks device activity for authenticated requests.
/// Runs after authentication middleware and ClientInfoMiddleware.
/// Can be skipped on specific endpoints using [SkipDeviceTracking] attribute.
/// </summary>
public class DeviceTrackingMiddleware(RequestDelegate next, ILogger<DeviceTrackingMiddleware> logger)
{
    public async Task InvokeAsync(
        HttpContext context,
        IClientDeviceService clientDeviceService,
        IClientInfoAccessor clientInfoAccessor,
        IUserContext userContext)
    {
        try
        {
            // Only track for authenticated users
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                // Check if endpoint has [SkipDeviceTracking] attribute
                var endpoint = context.GetEndpoint();
                var skipTracking = endpoint?.Metadata.GetMetadata<SkipDeviceTrackingAttribute>() != null;

                if (!skipTracking)
                {
                    var userId = userContext.GetUserId();
                    var clientInfo = clientInfoAccessor.Current;
                    var clientDeviceId = clientInfoAccessor.CurrentDeviceId;

                    if (userId.HasValue && clientInfo != null)
                    {
                        // Identify/register device and store in context for downstream use
                        var device = await clientDeviceService.IdentifyOrRegisterDeviceAsync(
                            userId.Value,
                            clientInfo,
                            clientDeviceId);

                        // Store device in HttpContext.Items for downstream access
                        context.Items["CurrentDevice"] = device;

                        logger.LogTrace(
                            "Identified device {DeviceId} ({DeviceName}) for user {UserId}",
                            device.Id, device.FriendlyName, userId);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Don't break the request pipeline if device tracking fails
            logger.LogError(ex, "Failed to track device activity");
        }

        await next(context);
    }
}

/// <summary>
/// Attribute to skip device tracking on specific endpoints.
/// Use for high-frequency endpoints where device tracking adds unnecessary overhead.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SkipDeviceTrackingAttribute : Attribute;
