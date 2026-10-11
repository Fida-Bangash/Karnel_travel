namespace Karnel_Travel.Models;

// Search aur Advanced Search page ka data: filters (input) aur results (output) ek hi class me
public class SearchViewModel
{
    // ---------- Filters (user jo form me bharta hai) ----------
    public string? Keyword { get; set; }        // Naam ya category ka lafz

    public string? Location { get; set; }       // Shehar ya jagah

    public decimal? MaxPrice { get; set; }      // Zyada se zyada price

    public int? MinRating { get; set; }        // Kam az kam rating (quality)

    public int? MinQuantity { get; set; }       // Kam az kam rooms/tables/units (quantity)

    public bool AvailableOnly { get; set; }     // Sirf available wale dikhao

    // ---------- Results (search ke natije) ----------
    public List<TouristSpot> Spots { get; set; } = new();

    public List<Hotel> Hotels { get; set; } = new();

    public List<Restaurant> Restaurants { get; set; } = new();

    public List<Resort> Resorts { get; set; } = new();
}
