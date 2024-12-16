using Microsoft.AspNetCore.Mvc;

namespace Postomat.AuthorizationMicroservice.Controllers;

public class AuthorizationController : ControllerBase
{
    public IActionResult Index()
    {
        return Ok();
    }
}