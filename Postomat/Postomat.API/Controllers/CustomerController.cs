using Microsoft.AspNetCore.Mvc;

namespace Postomat.API.Controllers;

public class CustomerController : ControllerBase
{
    public IActionResult Index()
    {
        return Ok();
    }
}