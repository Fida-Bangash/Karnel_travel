namespace Karnel_Travel.Models
{
    // Data for the error page.
    public class ErrorViewModel
    {
        // Unique request id, used to trace the problem.
        public string? RequestId { get; set; }

        // True when a request id exists (shown on the page).
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
