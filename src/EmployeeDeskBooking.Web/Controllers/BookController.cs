using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeDeskBooking.Web.Controllers;

[Authorize]
public class BookController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();
}
