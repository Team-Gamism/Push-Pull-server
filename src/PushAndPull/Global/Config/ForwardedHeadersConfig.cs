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
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor;

            if (!string.IsNullOrWhiteSpace(knownProxyIp))
                options.KnownProxies.Add(IPAddress.Parse(knownProxyIp));
        });

        return services;
    }
}
