namespace Karnel_Travel.Models;

// Data for Search and Advanced Search: filters (input) and results (output) in one class.
public class SearchViewModel
{
    // Filter: name or category keyword.
    public string? Keyword { get; set; }

    // Filter: city or place.
    public string? Location { get; set; }

    // Filter: maximum price.
    public decimal? MaxPrice { get; set; }

    // Filter: minimum rating.
    public int? MinRating { get; set; }

    // Filter: minimum rooms, tables or units.
    public int? MinQuantity { get; set; }

    // Filter: show only available items.
    public bool AvailableOnly { get; set; }

    // Result: matching tourist spots.
    public List<TouristSpot> Spots { get; set; } = new();

    // Result: matching hotels.
    public List<Hotel> Hotels { get; set; } = new();

    // Result: matching restaurants.
    public List<Restaurant> Restaurants { get; set; } = new();

    // Result: matching resorts.
    public List<Resort> Resorts { get; set; } = new();
}
