using Azure.Core;
using Azure.Identity;
using GraphProcessor.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GraphProcessor.Tokens;

/// <summary>
/// Creates a new TokenCredential instance based on configuration.
/// Credentials info are selected from (from the highest to the lowest priority):
/// <list type="number">
///     <item>Console args</item>
///     <item>User Secrets (only if Environment = "Development" - check https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0)</item>
///     <item>Environment Variables</item>
///     <item>Other mechanisms, e.g. Managed Identity (check the DefaultAzureCredential class' doc: https://learn.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet)</item>
/// </list>
/// </summary>
public class TokenCredentialFactory(ILogger<TokenCredentialFactory> logger, IOptions<TokenCredentialFactoryOptions> options)
{
    public TokenCredential Create()
    {
        var optionsValue = options.Value;
        if (!string.IsNullOrWhiteSpace(optionsValue.TenantId) && !string.IsNullOrWhiteSpace(optionsValue.ClientId) && !string.IsNullOrWhiteSpace(optionsValue.Secret))
        {
            logger.LogDebug("Creating a ClientSecretCredential based on CLI args");
            return new ClientSecretCredential(optionsValue.TenantId, optionsValue.ClientId, optionsValue.Secret);
        }
        else
        {
            logger.LogDebug("Creating a DefaultAzureCredential");
            return new DefaultAzureCredential();
        }
    }
}
