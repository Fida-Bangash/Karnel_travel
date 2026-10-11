namespace Karnel_Travel.Models
{
    // Error page ko data deta hai (koi masla aaye to ye page dikhta hai)
    public class ErrorViewModel
    {
        public string? RequestId { get; set; } // Request ki unique ID (masla dhoondne ke liye)

        // Sirf tab true jab RequestId maujood ho (page par ID dikhane ke liye)
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
