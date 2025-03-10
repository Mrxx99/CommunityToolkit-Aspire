﻿using System.Globalization;

using CommunityToolkit.Aspire.Hosting.Java.Utils;
using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

/// <summary>
/// Provides extension methods for adding Java applications to an <see cref="IDistributedApplicationBuilder"/>.
/// </summary>
public static class JavaAppHostingExtension
{
    /// <summary>
    /// Adds a Java application to the application model. Executes the containerized Java app.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/> to add the resource to.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="options">The <see cref="JavaAppContainerResourceOptions"/> to configure the Java application.</param>"
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<JavaAppContainerResource> AddJavaApp(this IDistributedApplicationBuilder builder, string name, JavaAppContainerResourceOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ContainerImageName, nameof(options.ContainerImageName));

        var resource = new JavaAppContainerResource(name);

        var rb = builder.AddResource(resource)
          .WithAnnotation(new ContainerImageAnnotation { Image = options.ContainerImageName, Tag = options.ContainerImageTag, Registry = options.ContainerRegistry })
          .WithHttpEndpoint(port: options.Port, targetPort: options.TargetPort, name: JavaAppContainerResource.HttpEndpointName)
          .WithJavaDefaults(options);

        if (options.Args is { Length: > 0 })
        {
            rb.WithArgs(options.Args);
        }

        return rb;
    }

    /// <summary>
    /// Adds a Spring application to the application model. Executes the containerized Spring app.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/> to add the resource to.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="options">The <see cref="JavaAppContainerResourceOptions"/> to configure the Java application.</param>"
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<JavaAppContainerResource> AddSpringApp(this IDistributedApplicationBuilder builder, string name, JavaAppContainerResourceOptions options) =>
        builder.AddJavaApp(name, options);

    /// <summary>
    /// Adds a Java application to the application model. Executes the executable Java app.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/> to add the resource to.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="workingDirectory">The working directory to use for the command. If null, the working directory of the current process is used.</param>
    /// <param name="options">The <see cref="JavaAppExecutableResourceOptions"/> to configure the Java application.</param>"
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<JavaAppExecutableResource> AddJavaApp(this IDistributedApplicationBuilder builder, string name, string workingDirectory, JavaAppExecutableResourceOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectory, nameof(workingDirectory));

#pragma warning disable CS8601 // Possible null reference assignment.
        string[] allArgs = options.Args is { Length: > 0 }
            ? ["-jar", options.ApplicationName, .. options.Args]
            : ["-jar", options.ApplicationName];
#pragma warning restore CS8601 // Possible null reference assignment.

        workingDirectory = PathNormalizer.NormalizePathForCurrentPlatform(Path.Combine(builder.AppHostDirectory, workingDirectory));
        var resource = new JavaAppExecutableResource(name, "java", workingDirectory);

        return builder.AddResource(resource)
                      .WithJavaDefaults(options)
                      .WithHttpEndpoint(port: options.Port, name: JavaAppContainerResource.HttpEndpointName, isProxied: false)
                      .WithArgs(allArgs);
    }

    /// <summary>
    /// Adds a Spring application to the application model. Executes the executable Spring app.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/> to add the resource to.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="workingDirectory">The working directory to use for the command. If null, the working directory of the current process is used.</param>
    /// <param name="options">The <see cref="JavaAppExecutableResourceOptions"/> to configure the Java application.</param>"
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<JavaAppExecutableResource> AddSpringApp(this IDistributedApplicationBuilder builder, string name, string workingDirectory, JavaAppExecutableResourceOptions options) =>
        builder.AddJavaApp(name, workingDirectory, options);

    private static IResourceBuilder<JavaAppContainerResource> WithJavaDefaults(
        this IResourceBuilder<JavaAppContainerResource> builder,
        JavaAppContainerResourceOptions options) =>
        builder.WithOtlpExporter()
               .WithEnvironment("JAVA_TOOL_OPTIONS", $"-javaagent:{options.OtelAgentPath?.TrimEnd('/')}/opentelemetry-javaagent.jar")
               ;

    private static IResourceBuilder<JavaAppExecutableResource> WithJavaDefaults(
        this IResourceBuilder<JavaAppExecutableResource> builder,
        JavaAppExecutableResourceOptions options) =>
        builder.WithOtlpExporter()
               .WithEnvironment("JAVA_TOOL_OPTIONS", $"-javaagent:{options.OtelAgentPath?.TrimEnd('/')}/opentelemetry-javaagent.jar")
               .WithEnvironment("SERVER_PORT", options.Port.ToString(CultureInfo.InvariantCulture))
               ;
}
