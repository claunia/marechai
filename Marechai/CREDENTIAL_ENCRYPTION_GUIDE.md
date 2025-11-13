# Credential Encryption Configuration Guide

## Overview

Your Marechai application now supports secure local credential encryption using ASP.NET Core's Data Protection API (
DPAPI). This provides encryption of sensitive credentials without requiring cloud services.

## Using Encrypted Credentials

### Option 1: Continue with Plaintext Credentials (Development)

Your existing `appsettings.json` configuration will continue to work as-is:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=zeus.claunia.com;port=3306;user=marechai;password=marechaipass;database=marechai;TreatTinyAsBoolean=false"
  }
}
```

### Option 2: Use Encrypted Credentials (Production)

For production deployments, encrypt your connection string:

1. **Create an encryption tool** - Run a utility to encrypt your connection string:

```csharp
var protectionProvider = DataProtectionProvider.Create("Marechai");
var protector = protectionProvider.CreateProtector("Marechai.CredentialEncryption");
string encrypted = protector.Protect(plaintextConnectionString);
string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(encrypted));
```

2. **Store encrypted value** - Add to your `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnectionEncrypted": "YOUR_BASE64_ENCRYPTED_VALUE_HERE"
  }
}
```

3. **Application will automatically**:
  - Try to decrypt `DefaultConnectionEncrypted` first
  - Fall back to plaintext `DefaultConnection` if encrypted version not found
  - Log warnings if decryption fails

## Key Features

✅ **Local Encryption** - Uses Windows Data Protection API (DPAPI) or Linux DPAPI equivalent
✅ **No Cloud Dependency** - All encryption happens locally with no external services
✅ **Backward Compatible** - Existing plaintext connection strings continue to work
✅ **Automatic Fallback** - Seamlessly falls back from encrypted to plaintext
✅ **Scoped Encryption** - Uses application-specific encryption key ("Marechai.CredentialEncryption")

## Security Notes

⚠️ **DPAPI Scope**:

- Windows DPAPI: Per-machine or per-user scope (depends on configuration)
- Linux DPAPI: Key management may require additional configuration

⚠️ **Best Practices**:

- Store encrypted values in environment-specific appsettings files
- Never commit plaintext credentials to version control
- Use encrypted credentials in production deployments
- Restrict file permissions on appsettings files

## Migration Steps

1. **For Development**: No action needed - continue using plaintext credentials
2. **For Production**:
  - Encrypt your connection string using the helper methods
  - Update appsettings.Production.json with encrypted value
  - Deploy and verify decryption works
  - Remove plaintext credentials from production environment

## Helper Classes

### CredentialEncryptor

Located in `Marechai/Helpers/CredentialEncryptor.cs`

- `EncryptCredential(plaintext)` - Encrypts a credential
- `DecryptCredential(encryptedBase64)` - Decrypts a credential
- `IsEncrypted(credential)` - Checks if a credential is encrypted

### ConnectionStringManager

Located in `Marechai/Helpers/ConnectionStringManager.cs`

- `GetConnectionString(configuration, encryptor?)` - Gets connection string with auto-fallback
- `AddConnectionStringManagement(services)` - Registers DI services

## Configuration in appsettings.json

Your application supports both connection string formats in any appsettings file:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "plaintext-connection-string-here",
    "DefaultConnectionEncrypted": "base64-encrypted-value-here"
  }
}
```

The application will:

1. Check for encrypted value first
2. Use plaintext as fallback
3. Log warnings if something goes wrong

---

**Note**: All credential encryption is local-only using DPAPI. No Azure or cloud services are required.
