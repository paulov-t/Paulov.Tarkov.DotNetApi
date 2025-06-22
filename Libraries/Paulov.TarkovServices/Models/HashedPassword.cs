using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Paulov.TarkovServices.Models;

public readonly struct HashedPassword :  IEquatable<HashedPassword>
{
    public const byte CurrentPasswordVersion = 1;
    [JsonProperty("hash")]
    public readonly byte[] PasswordHash;
    [JsonProperty("salt")]
    public readonly byte[] Salt;
    [JsonProperty("algorithm")]
    public readonly HashAlgorithmName HashAlgorithm;
    [JsonProperty("iterations")]
    public readonly uint Iterations;
    [JsonProperty("version")]
    public readonly byte PasswordVersion = CurrentPasswordVersion; //Future-proofing
     
    public HashedPassword(byte[] passwordHash, byte[] salt, HashAlgorithmName hashAlgorithm, uint iterations, byte pwdVer = CurrentPasswordVersion) : this()
    {
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Salt = salt ?? throw new ArgumentNullException(nameof(salt));
        HashAlgorithm = hashAlgorithm;
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(iterations, nameof(iterations));
        Iterations = iterations;
        PasswordVersion = pwdVer;
    }

    public override string ToString()
    {
        return Convert.ToHexString(PasswordHash);
    }

    public bool Equals(HashedPassword other)
    {
        //SequenceEqual does the heavy lifting here. It will load the values into vectors if needed
        return PasswordHash.SequenceEqual(other.PasswordHash) && HashAlgorithm.Equals(other.HashAlgorithm) && Iterations == other.Iterations && PasswordVersion == other.PasswordVersion;
    }

    public override bool Equals(object obj)
    {
        return obj is HashedPassword other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PasswordHash, HashAlgorithm, Iterations, PasswordVersion);
    }

    public static bool operator ==(HashedPassword left, HashedPassword right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(HashedPassword left, HashedPassword right)
    {
        return !(left == right);
    }
}