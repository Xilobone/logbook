using Microsoft.AspNetCore.DataProtection;

namespace Logbook.Data
{
using Microsoft.AspNetCore.DataProtection;

public static class EncryptionHelper
{
    private static IDataProtector _protector;

    // Call this once at startup
    public static void Init(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("SensitiveDataProtector");
    }

    public static string Encrypt(string value)
        => value == null ? null : _protector.Protect(value);

    public static string Decrypt(string value)
        => value == null ? null : _protector.Unprotect(value);
}

}