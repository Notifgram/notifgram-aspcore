using Microsoft.AspNetCore.Mvc;
namespace notifgram.Web.Api;

[Route("api/[controller]")]
[ApiController]
public class FilesController : BaseApiController
{
  private readonly string _filesDirectory = "";

  [HttpGet("{fileName}")]
  public IActionResult GetFile(string fileName)
  {
    // Ensure the file exists
    var filePath = Path.Combine(_filesDirectory, fileName);
    if (!System.IO.File.Exists(filePath))
    {
      return NotFound();
    }

    // Serve the file
    var fileBytes = System.IO.File.ReadAllBytes(filePath);
    return File(fileBytes, "application/octet-stream", fileName);
  }

}
