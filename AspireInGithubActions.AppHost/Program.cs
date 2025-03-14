using System.Runtime.InteropServices;

var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.AspireInGithubActions_ApiService>("apiservice", "http");

var container = builder.AddContainer("sdk", "mcr.microsoft.com/dotnet/sdk")
    // work around https://github.com/dotnet/aspire/issues/7809#issuecomment-2688753611
    .WithEntrypoint("sh")
    .WithArgs("-c", ReferenceExpression.Create($"sleep 1;curl {apiService.GetEndpoint("http").Property(EndpointProperty.Url)}/weatherforecast -v"))
    .WaitFor(apiService);

if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    container.WithContainerRuntimeArgs("--add-host=host.docker.internal:host-gateway");
}

builder.Build().Run();
