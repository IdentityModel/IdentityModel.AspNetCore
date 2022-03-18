using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace IdentityModel.AspNetCore.AccessTokenManagement.Azure;

/// <summary>
///     Extension methods for TokenManagementBuilder to register Azure Key vault caching services.
/// </summary>
public static class KeyVaultTokenManagementBuilderExtensions
{
    /// <summary>
    ///     Adds secret persistance to Azure Key Vault.
    /// </summary>
    /// <param name="tokenManagementBuilder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static TokenManagementBuilder WithAzureKeyVault(
        this TokenManagementBuilder tokenManagementBuilder,
        Action<KeyVaultOptions> options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var keyVaultOptions = new KeyVaultOptions();
        options(keyVaultOptions);

        if (keyVaultOptions.Url is null || keyVaultOptions.Credential is null)
        {
            throw new ArgumentException($"Invalid or missing {nameof(keyVaultOptions.Url)} or {keyVaultOptions.Credential}.");
        }

        tokenManagementBuilder
            .Services
            .AddTransient<IClientAccessTokenCache, KeyVaultClientAccessTokenCache>();

        tokenManagementBuilder
            .Services.AddAzureClients(builder =>
            {
                builder
                  .AddSecretClient(keyVaultOptions.Url)
                  .WithCredential(keyVaultOptions.Credential);
            });

        return tokenManagementBuilder;
    }
}
