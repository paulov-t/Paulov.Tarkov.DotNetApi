using Microsoft.AspNetCore.RequestDecompression;

namespace Paulov.Tarkov.WebServer.DOTNET.Providers
{
    public class ZLibDecompressionProvider : IDecompressionProvider
    {
        public Stream GetDecompressionStream(Stream stream)
        {
            // Write your code here to decompress
            return stream;
        }
    }
}
