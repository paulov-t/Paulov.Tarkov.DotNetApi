using Paulov.TarkovServices.Models;

namespace Paulov.TarkovServices.Services;

public interface IPasswordService
{
    void GenerateSalt(Span<byte> buffer);
    HashedPassword GenerateSaltedHash(string password);
    HashedPassword GenerateHashFromExistingSalt(string password, HashedPassword existingHash);
    HashedPassword HashPassword(string password, ReadOnlySpan<byte> salt);
}