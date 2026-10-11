using Karnel_Travel.Data;
using Karnel_Travel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karnel_Travel.Controllers;

public class SearchController(Karnel_TravelContext db) : Controller
{
    // Simple Search
    public async Task<IActionResult> Index(string? keyword)
    {
        var vm = new SearchViewModel
        {
            Keyword = keyword
        };

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            vm.Spots = await db.TouristSpots
                .Where(s =>
                    s.Name.Contains(keyword) ||
                    s.Category.Contains(keyword) ||
                    s.Location.Contains(keyword))
                .ToListAsync();

            vm.Hotels = await db.Hotels
                .Where(h =>
                    h.Name.Contains(keyword) ||
                    h.Location.Contains(keyword))
                .ToListAsync();

            vm.Restaurants = await db.Restaurants
                .Where(r =>
                    r.Name.Contains(keyword) ||
                    r.Location.Contains(keyword) ||
                    (r.Cuisine ?? "").Contains(keyword))
                .ToListAsync();

            vm.Resorts = await db.Resorts
                .Where(r =>
                    r.Name.Contains(keyword) ||
                    r.Location.Contains(keyword))
                .ToListAsync();
        }

        return View(vm);
    }

    // Advanced Search: location, price, quality(rating), quantity, availability
    [HttpGet]
    public async Task<IActionResult> Advanced(SearchViewModel f)
    {
        // ---------- Hotels ----------
        var hotels = db.Hotels.AsQueryable();
        if (!string.IsNullOrWhiteSpace(f.Keyword)) hotels = hotels.Where(h => h.Name.Contains(f.Keyword));
        if (!string.IsNullOrWhiteSpace(f.Location)) hotels = hotels.Where(h => h.Location.Contains(f.Location));
        if (f.MaxPrice.HasValue) hotels = hotels.Where(h => h.Price <= f.MaxPrice.Value);
        if (f.MinRating.HasValue) hotels = hotels.Where(h => h.Rating >= f.MinRating.Value);
        if (f.MinQuantity.HasValue) hotels = hotels.Where(h => h.Quantity >= f.MinQuantity.Value);
        if (f.AvailableOnly) hotels = hotels.Where(h => h.IsAvailable && h.Quantity > 0);
        f.Hotels = await hotels.ToListAsync();

        // ---------- Restaurants ----------
        var restaurants = db.Restaurants.AsQueryable();
        if (!string.IsNullOrWhiteSpace(f.Keyword))
            restaurants = restaurants.Where(r => r.Name.Contains(f.Keyword) || (r.Cuisine ?? "").Contains(f.Keyword));
        if (!string.IsNullOrWhiteSpace(f.Location)) restaurants = restaurants.Where(r => r.Location.Contains(f.Location));
        if (f.MaxPrice.HasValue) restaurants = restaurants.Where(r => r.Price <= f.MaxPrice.Value);
        if (f.MinRating.HasValue) restaurants = restaurants.Where(r => r.Rating >= f.MinRating.Value);
        if (f.MinQuantity.HasValue) restaurants = restaurants.Where(r => r.Quantity >= f.MinQuantity.Value);
        if (f.AvailableOnly) restaurants = restaurants.Where(r => r.IsAvailable && r.Quantity > 0);
        f.Restaurants = await restaurants.ToListAsync();

        // ---------- Resorts ----------
        var resorts = db.Resorts.AsQueryable();
        if (!string.IsNullOrWhiteSpace(f.Keyword)) resorts = resorts.Where(r => r.Name.Contains(f.Keyword));
        if (!string.IsNullOrWhiteSpace(f.Location)) resorts = resorts.Where(r => r.Location.Contains(f.Location));
        if (f.MaxPrice.HasValue) resorts = resorts.Where(r => r.Price <= f.MaxPrice.Value);
        if (f.MinRating.HasValue) resorts = resorts.Where(r => r.Rating >= f.MinRating.Value);
        if (f.MinQuantity.HasValue) resorts = resorts.Where(r => r.Quantity >= f.MinQuantity.Value);
        if (f.AvailableOnly) resorts = resorts.Where(r => r.IsAvailable && r.Quantity > 0);
        f.Resorts = await resorts.ToListAsync();

        // ---------- Tourist Spots ----------
        // Spots me price/rating nahi hota, isliye agar user ne price/rating/quantity/availability
        // ka filter lagaya hai to spots dikhane ka matlab nahi
        bool bookableFilter = f.MaxPrice.HasValue || f.MinRating.HasValue || f.MinQuantity.HasValue || f.AvailableOnly;
        if (!bookableFilter)
        {
            var spots = db.TouristSpots.AsQueryable();
            if (!string.IsNullOrWhiteSpace(f.Keyword))
                spots = spots.Where(s => s.Name.Contains(f.Keyword) || s.Category.Contains(f.Keyword));
            if (!string.IsNullOrWhiteSpace(f.Location))
                spots = spots.Where(s => s.Location.Contains(f.Location));
            f.Spots = await spots.ToListAsync();
        }

        return View(f);
    }
}