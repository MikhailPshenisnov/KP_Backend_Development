using Microsoft.AspNetCore.Mvc;

namespace Postomat.API.Controllers;

public class DeliveryController : ControllerBase
{
    public IActionResult Index()
    {
        return Ok();
    }
}