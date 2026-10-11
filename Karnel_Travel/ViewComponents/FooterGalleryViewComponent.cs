using Karnel_Travel.Data;
using Karnel_Travel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karnel_Travel.ViewComponents
{
    // Website ke footer ki Gallery: sab se naye 6 tourist spots ki images database se dikhata hai
    public class FooterGalleryViewComponent : ViewComponent
    {
        private readonly Karnel_TravelContext _db;

        // Constructor: ASP.NET khud database context de deta hai (dependency injection)
        public FooterGalleryViewComponent(Karnel_TravelContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Sirf wo spots jin ki image upload hui hai, sab se naye pehle
            var spots = await _db.TouristSpots
                .AsNoTracking()
                .Where(s => s.ImagePath != null && s.ImagePath != "")
                .OrderByDescending(s => s.Id)
                .Take(6)
                .ToListAsync();

            return View(spots);
        }
    }
}
