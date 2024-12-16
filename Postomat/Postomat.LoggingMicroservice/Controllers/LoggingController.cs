using Microsoft.AspNetCore.Mvc;

namespace Postomat.LoggingMicroservice.Controllers;

public class LoggingController : ControllerBase
{
    public IActionResult Index()
    {
        return Ok();
    }
}