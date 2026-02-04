using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PortalFilmowy.Models;
using PortalFilmowy.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

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

    [Authorize(Roles = "Admin")]
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

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Stats()
    {
        var movies = _context.Movies;
        var reviews = _context.Reviews;

        var movies_count = movies.Count();
        var reviews_count = reviews.Count();

        ViewData["moviesCount"] = movies_count;
        ViewData["reviewsCount"] = reviews_count;

        // Szukanie najlepszego filmu
        // Pobieramy filmy, które mają przynajmniej jedną opinię
        var moviesWithReviews = await _context.Movies
            .Include(m => m.Reviews)
            .Where(m => m.Reviews.Any())
            .ToListAsync();

        // Sortujemy w pamięci (bezpieczniejsze dla EF Core)
        var bestMovie = moviesWithReviews
            .OrderByDescending(m => m.Reviews.Average(r => r.Rating))
            .FirstOrDefault();

        if (bestMovie != null)
        {
            double avg = bestMovie.Reviews.Average(r => r.Rating);
            ViewData["BestMovieTitle"] = bestMovie.Title;
            ViewData["BestMovieRatingVal"] = avg; // Dokładna wartość (np. 8.5)
            ViewData["BestMovieRatingInt"] = (int)Math.Round(avg); // Zaokrąglona do gwiazdek(np. 9)
        }
        else
        {
            // Zabezpieczenie na wypadek pustej bazy
            ViewData["BestMovieTitle"] = "Brak danych";
            ViewData["BestMovieRatingVal"] = 0.0;
            ViewData["BestMovieRatingInt"] = 0;
        }

        return View();
    }
}
