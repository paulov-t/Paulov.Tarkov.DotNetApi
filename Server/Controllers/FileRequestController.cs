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
    public async Task<IActionResult> ServeFile(string path)
    {
        if (path.EndsWith(".jpg"))
        {
            HttpClient checkClient = new HttpClient();
            if ((await checkClient.GetAsync($"https://raw.githubusercontent.com/paulov-t/Paulov.Tarkov.Db/refs/heads/master/files/{path}")).StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new RedirectResult($"https://raw.githubusercontent.com/paulov-t/Paulov.Tarkov.Db/refs/heads/master/files/{path.Replace(".jpg", ".png")}");
            }
        }

        return new RedirectResult($"https://raw.githubusercontent.com/paulov-t/Paulov.Tarkov.Db/refs/heads/master/files/{path}");
    }
}