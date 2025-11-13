using System;
using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace Marechai.Helpers;

/// <summary>
///     Handles encryption and decryption of credentials using ASP.NET Core Data Protection API (DPAPI).
///     This provides local, secure credential storage without requiring external services.
/// </summary>
public class CredentialEncryptor
{
    private const    string         PurposeString = "Marechai.CredentialEncryption";
    private readonly IDataProtector _protector;

    public CredentialEncryptor(IDataProtectionProvider protectionProvider)
    {
        if(protectionProvider == null) throw new ArgumentNullException(nameof(protectionProvider));

        _protector = protectionProvider.CreateProtector(PurposeString);
    }

    /// <summary>
    ///     Encrypts a plaintext credential string using the Data Protection API.
    /// </summary>
    /// <param name="plaintext">The plaintext credential to encrypt</param>
    /// <returns>Base64-encoded encrypted credential</returns>
    public string EncryptCredential(string plaintext)
    {
        if(string.IsNullOrEmpty(plaintext))
            throw new ArgumentException("Credential cannot be null or empty", nameof(plaintext));

        try
        {
            string encryptedString = _protector.Protect(plaintext);

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(encryptedString));
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException("Failed to encrypt credential", ex);
        }
    }

    /// <summary>
    ///     Decrypts a Base64-encoded encrypted credential string.
    /// </summary>
    /// <param name="encryptedBase64">The Base64-encoded encrypted credential</param>
    /// <returns>The decrypted plaintext credential</returns>
    public string DecryptCredential(string encryptedBase64)
    {
        if(string.IsNullOrEmpty(encryptedBase64))
            throw new ArgumentException("Encrypted credential cannot be null or empty", nameof(encryptedBase64));

        try
        {
            byte[] encryptedBytes  = Convert.FromBase64String(encryptedBase64);
            string encryptedString = Encoding.UTF8.GetString(encryptedBytes);

            return _protector.Unprotect(encryptedString);
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException("Failed to decrypt credential", ex);
        }
    }

    /// <summary>
    ///     Determines if a credential appears to be encrypted (Base64-encoded).
    /// </summary>
    /// <param name="credential">The credential to check</param>
    /// <returns>True if the credential appears to be encrypted, false otherwise</returns>
    public static bool IsEncrypted(string credential)
    {
        if(string.IsNullOrEmpty(credential)) return false;

        try
        {
            // Try to decode as Base64 - if it succeeds and doesn't look like a connection string, it's likely encrypted
            byte[] data    = Convert.FromBase64String(credential);
            string decoded = Encoding.UTF8.GetString(data);

            // If decoded string doesn't contain typical connection string characters, it's likely encrypted
            return !decoded.Contains("=") && !decoded.Contains(";");
        }
        catch
        {
            return false;
        }
    }
}