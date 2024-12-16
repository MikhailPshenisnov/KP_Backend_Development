using Microsoft.AspNetCore.Mvc;

namespace Postomat.API.Controllers;

public class AdminController : ControllerBase
{
    public IActionResult Index()
    {
        return Ok();
    }
}