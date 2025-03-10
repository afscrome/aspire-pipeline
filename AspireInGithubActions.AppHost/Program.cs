using System.Runtime.InteropServices;

var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.AspireInGithubActions_ApiService>("apiservice", "http")
    .WithEndpoint("http", x => {
        x.IsProxied = false;
    });

var container = builder.AddContainer("sdk", "mcr.microsoft.com/dotnet/sdk")
    // work around https://github.com/dotnet/aspire/issues/7809#issuecomment-2688753611
    .WithEntrypoint("sh")
    .WithArgs("-c", ReferenceExpression.Create($"sleep 1;curl {apiService.GetEndpoint("http").Property(EndpointProperty.Url)}/weatherforecast -v"))
    .WaitFor(apiService);

container.WithContainerRuntimeArgs("--add-host=foobar:host-gateway");
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    container.WithContainerRuntimeArgs("--add-host=host.docker.internal:host-gateway");
}

builder.Build().Run();
