using System.Diagnostics;
using CoffeeShop.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Privacy() => View();

    [Authorize(Roles = "Admin")]
    public IActionResult AdminOnly() => Content("หน้านี้สำหรับ Admin เท่านั้น");

    [Authorize(Roles = "Admin,Manager,Cashier")]
    public IActionResult StaffOnly() => Content($"พนักงาน: {User.Identity?.Name}");

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
