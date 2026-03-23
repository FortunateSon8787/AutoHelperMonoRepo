using System.Security.Cryptography;
using AutoHelper.Application.Common.Interfaces;

namespace AutoHelper.Infrastructure.Security;

/// <summary>
/// PBKDF2-based password hasher using SHA-256 with 100 000 iterations.
/// The stored format is "base64(salt).base64(hash)".
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;       // 128-bit salt
    private const int HashSize = 32;       // 256-bit hash
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        var parts = passwordHash.Split('.');
        if (parts.Length != 2) return false;

        byte[] salt;
        byte[] expectedHash;

        try
        {
            salt = Convert.FromBase64String(parts[0]);
            expectedHash = Convert.FromBase64String(parts[1]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        // Constant-time comparison to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
