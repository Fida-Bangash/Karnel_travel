using Karnel_Travel.Data;
using Karnel_Travel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karnel_Travel.Controllers;

// Information section: Spots, Travel, Hotels, Restaurants, Resorts ke public pages
public class InformationController(Karnel_TravelContext db) : Controller
{
    // Main Information page: 5 cards + naye listings + valid packages
    public async Task<IActionResult> Index()
    {
        // Sab se naye 3 hotels
        ViewBag.NewHotels = await db.Hotels
            .OrderByDescending(h => h.Id)
            .Take(3)
            .ToListAsync();

        // Sab se naye 3 resorts
        ViewBag.NewResorts = await db.Resorts
            .OrderByDescending(r => r.Id)
            .Take(3)
            .ToListAsync();

        // Sab se naye 3 restaurants
        ViewBag.NewRestaurants = await db.Restaurants
            .OrderByDescending(r => r.Id)
            .Take(3)
            .ToListAsync();

        // Sirf wo packages jo abhi valid hain (expire nahi hue)
        ViewBag.Packages = await db.TourPackages
            .Where(p => p.ValidTill == null || p.ValidTill >= DateTime.Today)
            .ToListAsync();

        return View();
    }

    // Tourist Spots ki list
    public async Task<IActionResult> Spots()
    {
        return View(await db.TouristSpots.ToListAsync());
    }

    // Travel / transport ki information
    public async Task<IActionResult> Travel()
    {
        return View(await db.TravelInfos.ToListAsync());
    }

    // Hotels ki list
    public async Task<IActionResult> Hotels()
    {
        return View(await db.Hotels.ToListAsync());
    }

    // Restaurants ki list
    public async Task<IActionResult> Restaurants()
    {
        return View(await db.Restaurants.ToListAsync());
    }

    // Resorts ki list
    public async Task<IActionResult> Resorts()
    {
        return View(await db.Resorts.ToListAsync());
    }
}