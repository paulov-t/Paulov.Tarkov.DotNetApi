using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers;

/// <summary>
/// Handles all client requests for files from the server
/// </summary>
/// <param name="zipArchive">The <see cref="ZipArchive"/> to be injected</param>
[Route("files/{*path}")]
public class FileRequestController([FromKeyedServices("fileAssets")] ZipArchive zipArchive) : Controller
{
    private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();
    /// <summary>
    /// Serves the requested file from the <see cref="ZipArchive"/> if a <see cref="ZipArchiveEntry"/> exists
    /// </summary>
    /// <param name="path">The <see cref="string"/> path to look for the request file</param>
    /// <returns>
    /// A <see cref="Task{IActionResult}"/> that yields a <see cref="FileStreamResult"/> if the file is found,
    /// or a <see cref="NotFoundResult"/> if not.
    /// </returns>
    [HttpGet]
    public Task<IActionResult> ServeFile(string path)
    {
        //TODO: Look for ways to not need this. Without this lock, if two threads access the archive it throws exceptions
        lock (zipArchive)
        {
            ZipArchiveEntry archiveEntry = zipArchive.GetEntry(path);
            if(archiveEntry == null) return Task.FromResult<IActionResult>(NotFound());

            if (!_contentTypeProvider.TryGetContentType(archiveEntry.Name, out string contentType))
            {
                contentType = "application/octet-stream";
            }
        
            return Task.FromResult<IActionResult>(File(archiveEntry.Open(), contentType, archiveEntry.Name));   
        }
    }
}