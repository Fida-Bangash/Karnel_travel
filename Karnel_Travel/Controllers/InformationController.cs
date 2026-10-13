using Karnel_Travel.Data;
using Karnel_Travel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karnel_Travel.Controllers;

// Public information pages: spots, travel, hotels, restaurants and resorts.
public class InformationController(Karnel_TravelContext db) : Controller
{
    // Information landing page: newest listings and valid packages.
    public async Task<IActionResult> Index()
    {
        // Newest 3 hotels.
        ViewBag.NewHotels = await db.Hotels
            .OrderByDescending(h => h.Id)
            .Take(3)
            .ToListAsync();

        // Newest 3 resorts.
        ViewBag.NewResorts = await db.Resorts
            .OrderByDescending(r => r.Id)
            .Take(3)
            .ToListAsync();

        // Newest 3 restaurants.
        ViewBag.NewRestaurants = await db.Restaurants
            .OrderByDescending(r => r.Id)
            .Take(3)
            .ToListAsync();

        // Only packages that have not expired.
        ViewBag.Packages = await db.TourPackages
            .Where(p => p.ValidTill == null || p.ValidTill >= DateTime.Today)
            .ToListAsync();

        return View();
    }

    // Lists all tourist spots.
    public async Task<IActionResult> Spots()
    {
        return View(await db.TouristSpots.ToListAsync());
    }

    // Lists travel and transport options.
    public async Task<IActionResult> Travel()
    {
        return View(await db.TravelInfos.ToListAsync());
    }

    // Lists all hotels.
    public async Task<IActionResult> Hotels()
    {
        return View(await db.Hotels.ToListAsync());
    }

    // Lists all restaurants.
    public async Task<IActionResult> Restaurants()
    {
        return View(await db.Restaurants.ToListAsync());
    }

    // Lists all resorts.
    public async Task<IActionResult> Resorts()
    {
        return View(await db.Resorts.ToListAsync());
    }
}