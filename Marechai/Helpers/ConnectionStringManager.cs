using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Marechai.Helpers;

/// <summary>
///     Manages connection string configuration with support for encrypted credentials.
///     Supports both plaintext (for development) and encrypted (for production) credentials.
/// </summary>
public static class ConnectionStringManager
{
    private const string DEFAULT_CONNECTION_KEY   = "DefaultConnection";
    private const string EncryptedConnectionKey = "DefaultConnectionEncrypted";

    /// <summary>
    ///     Gets the connection string from configuration, attempting to decrypt if necessary.
    /// </summary>
    /// <param name="configuration">The configuration object</param>
    /// <param name="credentialEncryptor">Optional credential encryptor for decryption</param>
    /// <returns>The connection string (plaintext or decrypted)</returns>
    public static string GetConnectionString(IConfiguration      configuration,
                                             CredentialEncryptor credentialEncryptor = null)
    {
        if(configuration == null) throw new ArgumentNullException(nameof(configuration));

        // First, try to get the encrypted connection string
        string encryptedConnection = configuration.GetConnectionString(EncryptedConnectionKey);

        if(!string.IsNullOrEmpty(encryptedConnection) && credentialEncryptor != null)
        {
            try
            {
                return credentialEncryptor.DecryptCredential(encryptedConnection);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"\u001b[31;1mWarning: Failed to decrypt connection string: {ex.Message}\u001b[0m");
            }
        }

        // Fall back to plaintext connection string
        string plaintextConnection = configuration.GetConnectionString(DEFAULT_CONNECTION_KEY);

        if(string.IsNullOrEmpty(plaintextConnection))
        {
            Console.WriteLine("\u001b[31;1mWarning: No connection string found in configuration\u001b[0m");
        }

        return plaintextConnection;
    }

    /// <summary>
    ///     Adds connection string management services to the DI container.
    /// </summary>
    /// <param name="services">The service collection</param>
    public static void AddConnectionStringManagement(IServiceCollection services)
    {
        services.AddDataProtection();
    }
}