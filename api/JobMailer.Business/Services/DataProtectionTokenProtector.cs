using System.Security.Cryptography;
using JobMailer.Business.Abstractions;
using Microsoft.AspNetCore.DataProtection;

namespace JobMailer.Business.Services;

public sealed class DataProtectionTokenProtector : ITokenProtector
{
    private readonly IDataProtector _protector;

    public DataProtectionTokenProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("JobMailer.OAuthTokens.v1");
    }

    public string Protect(string plainText) => _protector.Protect(plainText);

    public string? Unprotect(string? protectedText)
    {
        if (string.IsNullOrEmpty(protectedText))
        {
            return null;
        }

        try
        {
            return _protector.Unprotect(protectedText);
        }
        catch (CryptographicException)
        {
            // Key ring changed or data was tampered with; force a fresh Google sign-in.
            return null;
        }
    }
}
