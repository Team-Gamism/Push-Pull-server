using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace PushAndPull.Global.Config;

public static class ForwardedHeadersConfig
{
    public static IServiceCollection AddForwardedHeaders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var knownProxyIp = configuration["ReverseProxy:KnownProxyIp"];

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            if (!string.IsNullOrWhiteSpace(knownProxyIp))
            {
                if (!IPAddress.TryParse(knownProxyIp, out var proxyIp))
                    throw new InvalidOperationException($"ReverseProxy:KnownProxyIp is not a valid IP address: {knownProxyIp}");

                options.KnownProxies.Add(proxyIp);
            }
        });

        return services;
    }
}
