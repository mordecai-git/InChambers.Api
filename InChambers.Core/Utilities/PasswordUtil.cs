﻿using System.Security.Cryptography;

namespace InChambers.Core.Utilities;

internal static class PasswordUtil
{
    private const int SaltSize = 32; // 256 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 10000;

    public static string HashPassword(this string password)
    {
        // Generate a random salt
        byte[] salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Create the hash using PBKDF2 with SHA256
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(HashSize);

        // Combine the salt and hash and convert to Base64
        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    public static bool VerifyPassword(this string hashedPassword, string password)
    {
        // Convert the Base64 string back to bytes
        byte[] hashBytes = Convert.FromBase64String(hashedPassword);

        // Extract the salt from the hash
        byte[] salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        // Compute the hash of the entered password
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
        {
            byte[] hash = pbkdf2.GetBytes(HashSize);

            // Compare the computed hash with the stored hash
            for (int i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                    return false;
            }

            return true;
        }
    }
}