using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CryptoServer.Middleware
{
    public class IpRestrictionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<IpRestrictionMiddleware> _logger;
        private readonly string[] _allowedIps;

        public IpRestrictionMiddleware(RequestDelegate next, ILogger<IpRestrictionMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _allowedIps = configuration.GetSection("AllowedIps").Get<string[]>() ?? Array.Empty<string>();
            
            _logger.LogInformation("Allowed IPs: {AllowedIps}", string.Join(", ", _allowedIps));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            _logger.LogInformation("Request from IP: {ClientIp}", clientIp);

            if (!IsIpAllowed(clientIp))
            {
                _logger.LogWarning("Unauthorized access attempt from IP: {ClientIp}", clientIp);
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Unauthorized access" });
                return;
            }

            await _next(context);
        }

        private bool IsIpAllowed(string? clientIp)
        {
            if (string.IsNullOrEmpty(clientIp))
                return false;

            return _allowedIps.Any(ip => 
                ip == clientIp || 
                (ip.Contains("/") && IsIpInRange(clientIp, ip)));
        }

        private bool IsIpInRange(string clientIp, string ipRange)
        {
            try
            {
                var parts = ipRange.Split('/');
                if (parts.Length != 2)
                    return false;

                var ipAddress = IPAddress.Parse(clientIp);
                var networkAddress = IPAddress.Parse(parts[0]);
                var bitsLength = int.Parse(parts[1]);

                if (ipAddress.AddressFamily != networkAddress.AddressFamily)
                    return false;

                var ipBytes = ipAddress.GetAddressBytes();
                var networkBytes = networkAddress.GetAddressBytes();

                if (ipBytes.Length != networkBytes.Length)
                    return false;

                var bytesLength = bitsLength / 8;
                var remainingBits = bitsLength % 8;

                for (int i = 0; i < bytesLength; i++)
                {
                    if (ipBytes[i] != networkBytes[i])
                        return false;
                }

                if (remainingBits > 0)
                {
                    var mask = (byte)(0xFF << (8 - remainingBits));
                    return (ipBytes[bytesLength] & mask) == (networkBytes[bytesLength] & mask);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 