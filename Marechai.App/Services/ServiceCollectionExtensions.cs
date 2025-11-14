using System;
using System.Collections.Generic;
using System.Net.Http;
using Marechai.App.Services;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Serialization.Json;
using Microsoft.Kiota.Serialization.Multipart;

namespace Uno.Extensions.Http.Kiota;

/// <summary>
///     Provides extension methods for registering Kiota clients within the <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers a Kiota client with the specified <paramref name="name" /> and endpoint options.
    /// </summary>
    /// <typeparam name="TClient">The Kiota client type to register.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to register the client with.</param>
    /// <param name="context">The <see cref="HostBuilderContext" /> providing the hosting context.</param>
    /// <param name="options">[Optional] The endpoint options for the client (loaded from appsettings if not specified).</param>
    /// <param name="name">[Optional] The name for locating endpoint information in appsettings.</param>
    /// <param name="configure">[Optional] A callback for configuring the endpoint.</param>
    /// <returns>The updated <see cref="IServiceCollection" /> with the registered Kiota client.</returns>
    public static IServiceCollection AddKiotaClientV2<TClient>(this IServiceCollection services,
                                                               HostBuilderContext context,
                                                               EndpointOptions? options = null, string? name = null,
                                                               Func<IHttpClientBuilder, EndpointOptions,
                                                                   IHttpClientBuilder>? configure = null)
        where TClient : class =>
        services.AddKiotaClientWithEndpointV2<TClient, EndpointOptions>(context, options, name, configure);

    /// <summary>
    ///     Registers a Kiota client with the specified <paramref name="name" /> and supports additional endpoint options.
    /// </summary>
    /// <typeparam name="TClient">The Kiota client type to register.</typeparam>
    /// <typeparam name="TEndpoint">The type of endpoint to register.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to register the client with.</param>
    /// <param name="context">The <see cref="HostBuilderContext" /> providing the hosting context.</param>
    /// <param name="options">[Optional] The endpoint options for the client (loaded from appsettings if not specified).</param>
    /// <param name="name">[Optional] The name for locating endpoint information in appsettings.</param>
    /// <param name="configure">[Optional] A callback for configuring the endpoint.</param>
    /// <returns>The updated <see cref="IServiceCollection" /> with the registered Kiota client.</returns>
    public static IServiceCollection AddKiotaClientWithEndpointV2<TClient, TEndpoint>(
        this IServiceCollection services, HostBuilderContext context, TEndpoint? options = null, string? name = null,
        Func<IHttpClientBuilder, TEndpoint?, IHttpClientBuilder>? configure = null)
        where TClient : class where TEndpoint : EndpointOptions, new()
    {
        services.AddKiotaHandlers();
        string clientName = name ?? typeof(TClient).FullName ?? "DefaultClient";

        return services.AddClientWithEndpoint<TClient, TEndpoint>(context,
                                                                  options,
                                                                  clientName,
                                                                  (s, c) => s.AddHttpClient<TClient>(clientName)
                                                                             .AddTypedClient((httpClient, sp) =>
                                                                              {
                                                                                  var authProvider =
                                                                                      new
                                                                                          AnonymousAuthenticationProvider();

                                                                                  var parseNodeFactory =
                                                                                      new JsonParseNodeFactory();

                                                                                  var serializationWriterFactory =
                                                                                      new
                                                                                          CompositeSerializationWriterFactory();

                                                                                  serializationWriterFactory
                                                                                     .AddFactory(new
                                                                                          JsonSerializationWriterFactory());

                                                                                  serializationWriterFactory
                                                                                     .AddFactory(new
                                                                                          MultipartSerializationWriterFactory());

                                                                                  var requestAdapter =
                                                                                      new
                                                                                          HttpClientRequestAdapter(authProvider,
                                                                                              parseNodeFactory,
                                                                                              serializationWriterFactory,
                                                                                              httpClient);

                                                                                  return (TClient)
                                                                                      Activator
                                                                                         .CreateInstance(typeof(
                                                                                                  TClient),
                                                                                              requestAdapter)!;
                                                                              })
                                                                             .AttachKiotaHandlers(),
                                                                  configure);
    }

    /// <summary>
    ///     Dynamically adds Kiota handlers to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to register the handlers with.</param>
    /// <returns>The updated <see cref="IServiceCollection" /> with the registered Kiota handlers.</returns>
    private static IServiceCollection AddKiotaHandlers(this IServiceCollection services)
    {
        IList<Type> kiotaHandlers = KiotaClientFactory.GetDefaultHandlerTypes();
        foreach(Type handler in kiotaHandlers) services.AddTransient(handler);

        return services;
    }

    /// <summary>
    ///     Attaches Kiota handlers to the <see cref="IHttpClientBuilder" />.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder" /> to attach the handlers to.</param>
    /// <returns>The updated <see cref="IHttpClientBuilder" /> with the attached Kiota handlers.</returns>
    private static IHttpClientBuilder AttachKiotaHandlers(this IHttpClientBuilder builder)
    {
        IList<Type> kiotaHandlers = KiotaClientFactory.GetDefaultHandlerTypes();

        foreach(Type handler in kiotaHandlers)
            builder.AddHttpMessageHandler(sp => (DelegatingHandler)sp.GetRequiredService(handler));

        return builder;
    }
}