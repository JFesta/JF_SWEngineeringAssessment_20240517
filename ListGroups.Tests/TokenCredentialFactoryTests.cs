using Azure.Identity;
using ListGroups.Configs;
using ListGroups.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ListGroups.Tests;

public class TokenCredentialFactoryTests
{
    private ILogger<TokenCredentialFactory> _logger;

    public TokenCredentialFactoryTests()
    {
        _logger = new NullLogger<TokenCredentialFactory>();
    }

    [Fact]
    public void FullOptionsCreatesClientSecretCredentials()
    {
        var factory = new TokenCredentialFactory(_logger, Options.Create(new TokenCredentialFactoryOptions()
        {
            TenantId = Guid.NewGuid().ToString(),
            ClientId = Guid.NewGuid().ToString(),
            Secret = "SomeSecret"
        }));
        var result = factory.Create();
        Assert.NotNull(result);
        Assert.IsType<ClientSecretCredential>(result);
    }

    [Fact]
    public void PartialOptionsCreatesDefaultAzureCredentials()
    {
        var factory = new TokenCredentialFactory(_logger, Options.Create(new TokenCredentialFactoryOptions()
        {
            TenantId = Guid.NewGuid().ToString(),
            ClientId = null,
            Secret = null
        }));
        var result = factory.Create();
        Assert.NotNull(result);
        Assert.IsType<DefaultAzureCredential>(result);
    }

    [Fact]
    public void DefaultOptionsCreatesDefaultAzureCredentials()
    {
        var factory = new TokenCredentialFactory(_logger, Options.Create(new TokenCredentialFactoryOptions()));
        var result = factory.Create();
        Assert.NotNull(result);
        Assert.IsType<DefaultAzureCredential>(result);
    }
}