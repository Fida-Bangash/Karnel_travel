using Karnel_Travel.Data;
using Karnel_Travel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karnel_Travel.ViewComponents
{
    // Footer gallery: shows images of the 6 newest tourist spots from the database.
    public class FooterGalleryViewComponent : ViewComponent
    {
        // Database context.
        private readonly Karnel_TravelContext _db;

        // Constructor: the database context is injected by ASP.NET.
        public FooterGalleryViewComponent(Karnel_TravelContext db)
        {
            _db = db;
        }

        // Loads the gallery data and renders the component view.
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Only spots that have an image, newest first (read-only query).
            var spots = await _db.TouristSpots
                .AsNoTracking()
                .Where(s => s.ImagePath != null && s.ImagePath != "")
                .OrderByDescending(s => s.Id)
                .Take(6)
                .ToListAsync();

            // Pass the spots to the component view.
            return View(spots);
        }
    }
}
