using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace AspireInGithubActions.Tests;

public class WebTests
{
    [Test]
    [CancelAfter(120_000)]
    public async Task GetWebResourceRootReturnsOkStatusCode(CancellationToken ct)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            //Environment.SetEnvironmentVariable("AppHost__ContainerHostname", "host-gateway");
        }

        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AspireInGithubActions_AppHost>(ct);

        appHost.Services.AddLogging(x => x
            .SetMinimumLevel(LogLevel.Debug)
            .AddFilter("Aspire.", LogLevel.Debug)
            .AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug)
        );

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(ct);
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync(ct);

        // Act
        await resourceNotificationService.WaitForResourceHealthyAsync("apiservice", ct);
        var finalState = await resourceNotificationService.WaitForResourceAsync("sdk", KnownResourceStates.TerminalStates, ct);
        var resourceEvent = await resourceNotificationService.WaitForResourceAsync("sdk", evt => true, ct);
        // Assert
        Assert.That(resourceEvent.Snapshot.ExitCode, Is.EqualTo(0));
    }
}