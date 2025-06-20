using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers;

/// <summary>
/// Handles all client requests for files from the server
/// </summary>
[Route("files/{*path}")]
public class FileRequestController() : Controller
{
    private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    [HttpGet]
    public IActionResult ServeFile(string path)
    {
        return new RedirectResult($"https://raw.githubusercontent.com/paulov-t/Paulov.Tarkov.Db/refs/heads/master/files/{path}");
    }
}