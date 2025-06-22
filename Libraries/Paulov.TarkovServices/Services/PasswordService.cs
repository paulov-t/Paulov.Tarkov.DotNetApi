using System.Security.Cryptography;
using System.Text;
using Paulov.TarkovServices.Models;

namespace Paulov.TarkovServices.Services;

public class PasswordService : IDisposable, IPasswordService
{
    private const int Pbkdf2_Iterations = 2 ^ 15;
    private const int Pbkdf2_OutputBlockSize = 64;
    //TODO: Make this configurable from the app configuration
    private static readonly HashAlgorithmName Pbkdf2_HashAlgorithm = HashAlgorithmName.SHA512;
    
    private readonly RandomNumberGenerator _secureRNG = RandomNumberGenerator.Create(); 
    public PasswordService()
    {
        
    }

    public void GenerateSalt(Span<byte> buffer)
    {
        _secureRNG.GetBytes(buffer);
    }

    public HashedPassword GenerateSaltedHash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password, nameof(password));
        //It is recommended to make the salt the same size as the output hash length
        Span<byte> salt = stackalloc byte[Pbkdf2_OutputBlockSize];
        _secureRNG.GetBytes(salt);
        
        return HashPassword(password, salt);
    }

    public HashedPassword GenerateHashFromExistingSalt(string password, HashedPassword existingHash)
    {
        ArgumentException.ThrowIfNullOrEmpty(password, nameof(password));
        if(existingHash.PasswordVersion != HashedPassword.CurrentPasswordVersion) throw new InvalidOperationException("Password version mismatch.");
        return HashPassword(password, existingHash.Salt);
    }

    public HashedPassword HashPassword(string password, ReadOnlySpan<byte> salt)
    {
        //I'm not allowing unicode in passwords.
        //If someone tries to use an emoji in their password, they'll discover what their keyboard tastes like
        Span<byte> passwordAsBytes = stackalloc byte[Encoding.ASCII.GetByteCount(password)];
        Encoding.ASCII.GetBytes(password.AsSpan(), passwordAsBytes);
        
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            passwordAsBytes, 
            salt,
            Pbkdf2_Iterations,
            Pbkdf2_HashAlgorithm,
            Pbkdf2_OutputBlockSize);

        return new HashedPassword(hash, salt.ToArray(), Pbkdf2_HashAlgorithm, Pbkdf2_Iterations);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _secureRNG?.Dispose();
    }
}