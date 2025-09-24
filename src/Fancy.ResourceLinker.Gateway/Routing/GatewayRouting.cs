﻿using Fancy.ResourceLinker.Gateway.Routing.Auth;
using Fancy.ResourceLinker.Gateway.Routing.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace Fancy.ResourceLinker.Gateway.Routing;

/// <summary>
/// Class with helper methods to set up the routing feature.
/// </summary>
internal static class GatewayRouting
{
    /// <summary>
    /// Adds the gateway routing services to the ioc container.
    /// </summary>
    /// <param name="services">The services.</param>
    internal static void AddGatewayRouting(IServiceCollection services, GatewayRoutingSettings settings)
    {
        // Register all other needed services
        services.AddHttpForwarder();
        services.AddSingleton(settings); 
        services.AddTransient<GatewayRouter>();
        services.AddReverseProxy().AddGatewayRoutes(settings);

        // Set up routing authentication subsystem
        services.AddSingleton<RouteAuthenticationManager>();
        services.AddKeyedTransient<IRouteAuthenticationStrategy, NoAuthenticationAuthStrategy>(NoAuthenticationAuthStrategy.NAME);
        services.AddKeyedTransient<IRouteAuthenticationStrategy, EnsureAuthenticatedAuthStrategy>(EnsureAuthenticatedAuthStrategy.NAME);
        services.AddKeyedTransient<IRouteAuthenticationStrategy, TokenPassThroughAuthStrategy>(TokenPassThroughAuthStrategy.NAME);
        services.AddKeyedTransient<IRouteAuthenticationStrategy, AzureOnBehalfOfAuthStrategy>(AzureOnBehalfOfAuthStrategy.NAME);
        services.AddKeyedTransient<IRouteAuthenticationStrategy, ClientCredentialsAuthStrategy>(ClientCredentialsAuthStrategy.NAME);
        services.AddKeyedTransient<IRouteAuthenticationStrategy, Auth0ClientCredentialsAuthStrategy>(Auth0ClientCredentialsAuthStrategy.NAME);
    }

    /// <summary>
    /// Adds the gateway routing middleware to the middleware pipeline by using the yarp proxy.
    /// </summary>
    /// <param name="webApp">The web application.</param>
    internal static void UseGatewayRouting(WebApplication webApp)
    {
        webApp.MapReverseProxy(pipeline =>
        {
            pipeline.UseGatewayPipeline();
        });
    }

    /// <summary>
    /// Adds the gateway routes to the yarp proxy as in memory configuration.
    /// </summary>
    /// <param name="reverseProxyBuilder">The reverse proxy builder.</param>
    /// <param name="settings">The settings.</param>
    private static void AddGatewayRoutes(this IReverseProxyBuilder reverseProxyBuilder, GatewayRoutingSettings settings)
    {
        List<RouteConfig> routes = new List<RouteConfig>();
        List<ClusterConfig> clusters = new List<ClusterConfig>();

        // Add for each microservcie a route and a cluster
        foreach (KeyValuePair<string, RouteSettings> routeSettings in settings.Routes)
        {
            // Only for routes with a path match an automatic gateway route is created
            if (string.IsNullOrEmpty(routeSettings.Value.PathMatch)) continue;

            if (routeSettings.Value.BaseUrl == null) throw new InvalidOperationException($"A 'BaseUrl' must be set for route '{routeSettings.Key}'");
            routes.Add(new RouteConfig
            {
                RouteId = routeSettings.Key,
                ClusterId = routeSettings.Key,
                Match = new RouteMatch { Path = routeSettings.Value.PathMatch },
                Metadata = new Dictionary<string, string> { { "RouteName", routeSettings.Key } },
                Transforms = new List<Dictionary<string, string>>
                {
                    // Turn off default forwarded headers to be able to override it with the origin of the
                    // ResourceProxy from the configuration
                    new Dictionary<string, string>{ { "X-Forwarded", "Off" } }
                }
,            });

            clusters.Add(new ClusterConfig
            {
                ClusterId = routeSettings.Key,
                Destinations = new Dictionary<string, DestinationConfig> { { "default", new DestinationConfig { Address = routeSettings.Value.BaseUrl.AbsoluteUri } } },
            });
        }

        reverseProxyBuilder.LoadFromMemory(routes, clusters)
                           .AddTransforms(builder => 
                                builder.AddRequestTransform(context => { context.ProxyRequest.Headers.SetForwardedHeaders(settings.ResourceProxy); return ValueTask.CompletedTask; }));
    }
}
