using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PortalFilmowy.Models;
using PortalFilmowy.Data;
using Microsoft.EntityFrameworkCore;

namespace PortalFilmowy.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Stats()
    {
        var movies = _context.Movies;
        var reviews = _context.Reviews;

        var movies_count = movies.Count();
        var reviews_count = reviews.Count();

        ViewData["moviesCount"] = movies_count;
        ViewData["reviewsCount"] = reviews_count;

        return View();
    }
}
