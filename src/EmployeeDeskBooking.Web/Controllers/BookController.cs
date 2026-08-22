using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeDeskBooking.Web.Controllers;

[Authorize(Roles = "Employee")]
public class BookController : Controller
{
    [HttpGet]
    public IActionResult Index(DateOnly? date) =>
        RedirectToAction("Availability", "Desks", new { date });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CheckAvailability(DateOnly selectedDate) =>
        RedirectToAction("Availability", "Desks", new { date = selectedDate });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult BookDesk(Guid deskId, DateOnly selectedDate) =>
        RedirectToAction("Book", "Desks", new { deskId, date = selectedDate });
}
