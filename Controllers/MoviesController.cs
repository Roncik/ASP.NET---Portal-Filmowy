using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using PortalFilmowy.Data;
using PortalFilmowy.Models;

namespace PortalFilmowy.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            var movies = _context.Movies.Include(m => m.Genre);
            return View(await movies.ToListAsync());
        }

        // --- WŁASNA METODA LINQ (Search) --- [cite: 1]
        public async Task<IActionResult> Search(string searchString, int? genreId, string sortOrder)
        {
            // Ładowanie listy gatunków do Dropdowna
            ViewBag.Genres = new SelectList(_context.Genres, "Id", "Name");

            var movies = _context.Movies.Include(m => m.Genre).AsQueryable();

            // 1. Filtrowanie
            if (!string.IsNullOrEmpty(searchString))
            {
                movies = movies.Where(s => s.Title.Contains(searchString) || s.Director.Contains(searchString));
            }
            if (genreId.HasValue)
            {
                movies = movies.Where(x => x.GenreId == genreId);
            }

            // 2. Sortowanie
            movies = sortOrder switch
            {
                "date_desc" => movies.OrderByDescending(s => s.ReleaseDate),
                "title" => movies.OrderBy(s => s.Title),
                _ => movies.OrderByDescending(s => s.Id), // Domyślnie najnowsze dodane
            };

            return View(await movies.ToListAsync());
        }

        // GET: Movies/Create (Tylko dla admina) [cite: 1]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Movie movie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", movie.GenreId);
            return View(movie);
        }

        // ... Tutaj dodaj Edit, Delete, Details (najlepiej wygeneruj przez Scaffolding) ...

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies
                .Include(m => m.Genre)
                .Include(m => m.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            // SPRAWDZENIE: Czy zalogowany użytkownik ma już opinię dla tego filmu?
            if (User.Identity.IsAuthenticated)
            {
                var myReview = movie.Reviews.FirstOrDefault(r => r.UserName == User.Identity.Name);
                ViewBag.UserReview = myReview; // Przekazujemy istniejącą opinię (lub null) do widoku
            }

            return View(movie);
        }

        // POST: Movies/AddReview
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview([Bind("MovieId, Rating, Comment")] Review review)
        {
            review.UserName = User.Identity.Name;

            // Sprawdź, czy użytkownik już ocenił ten film
            bool alreadyExists = await _context.Reviews.AnyAsync(
                r => r.MovieId == review.MovieId && r.UserName == review.UserName);

            if (alreadyExists)
            {
                TempData["Error"] = "Możesz dodać tylko jedną opinię do filmu.";
                return RedirectToAction(nameof(Details), new { id = review.MovieId });
            }

            if (review.Rating < 1 || review.Rating > 10)
            {
                TempData["Error"] = "Ocena musi być między 1 a 10.";
            }
            else
            {
                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Dodano opinię!";
            }

            return RedirectToAction(nameof(Details), new { id = review.MovieId });
        }

        // GET: Movies/EditReview
        [Authorize]
        public async Task<IActionResult> EditReview(int? id)
        {
            if (id == null) return NotFound();

            // Pobierz opinię i upewnij się, że należy do zalogowanego użytkownika
            var review = await _context.Reviews.FindAsync(id);

            if (review == null || review.UserName != User.Identity.Name)
            {
                return Forbid(); // Brak dostępu
            }

            return View(review);
        }

        // POST: Movies/EditReview
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReview(int id, [Bind("Id, MovieId, Rating, Comment")] Review review)
        {
            if (id != review.Id) return NotFound();

            // Ponowne sprawdzenie własności (bezpieczeństwo)
            var originalReview = await _context.Reviews.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (originalReview == null || originalReview.UserName != User.Identity.Name)
            {
                return Forbid();
            }

            // Ustawiamy UserName ręcznie, bo nie przesyłamy go w formularzu (dla bezpieczeństwa)
            review.UserName = User.Identity.Name;

            if (ModelState.IsValid)
            {
                _context.Update(review);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Zaktualizowano opinię.";
                return RedirectToAction(nameof(Details), new { id = review.MovieId });
            }
            return View(review);
        }

        // POST: Movies/DeleteReview
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);

            // Sprawdź czy opinia istnieje i czy należy do użytkownika (lub czy to Admin)
            if (review != null && (review.UserName == User.Identity.Name || User.IsInRole("Admin")))
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Usunięto opinię.";
            }
            else
            {
                TempData["Error"] = "Nie możesz usunąć tej opinii.";
            }

            // Powrót do szczegółów filmu
            return RedirectToAction(nameof(Details), new { id = review?.MovieId });
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> Create([Bind("Id,Title,ReleaseDate,Director,GenreId")] Movie movie)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(movie);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", movie.GenreId);
        //    return View(movie);
        //}

        // GET: Movies/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", movie.GenreId);
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ReleaseDate,Director,GenreId")] Movie movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", movie.GenreId);
            return View(movie);
        }

        // GET: Movies/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}