using Karnel_Travel.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karnel_Travel.Areas.Admin.Controllers
{
    // Admin area controller; access limited to the Admin role.
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        // Number of rows shown per page.
        private const int PageSize = 10;

        // Identity user manager.
        private readonly UserManager<ApplicationUser> _userManager;

        // Constructor: dependencies are injected by ASP.NET.
        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // Lists users with search, status filter and pagination.
        public async Task<IActionResult> Index(string? q, string? status, int page = 1)
        {
            // Current UTC time used for lockout checks.
            var now = DateTimeOffset.UtcNow;
            // Base query over all users.
            var query = _userManager.Users.AsQueryable();

            // Search by name or email.
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(u => (u.FullName ?? "").Contains(q) || (u.Email ?? "").Contains(q));
            }

            // Filter by status: active = not locked out; inactive = locked out.
            if (status == "active")
                query = query.Where(u => u.LockoutEnd == null || u.LockoutEnd <= now);
            else if (status == "inactive")
                query = query.Where(u => u.LockoutEnd != null && u.LockoutEnd > now);

            // Pagination calculations.
            int totalItems = await query.CountAsync();
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize));
            page = Math.Clamp(page, 1, totalPages);

            // Fetch the current page of users.
            var users = await query
                .OrderBy(u => u.Email)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Build display rows, including each user's role.
            var rows = new List<UserRow>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                rows.Add(new UserRow
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? "",
                    Country = u.Country,
                    Role = roles.FirstOrDefault() ?? "User",
                    IsActive = !(u.LockoutEnd.HasValue && u.LockoutEnd > now)
                });
            }

            // Assemble the view model with paging and summary counts.
            var model = new UsersIndexViewModel
            {
                Users = rows,
                Query = q,
                Status = status,
                Page = page,
                PageSize = PageSize,
                TotalPages = totalPages,
                TotalItems = totalItems,
                AllUsers = await _userManager.Users.CountAsync(),
                ActiveUsers = await _userManager.Users.CountAsync(u => u.LockoutEnd == null || u.LockoutEnd <= now)
            };
            // Inactive users = total minus active.
            model.InactiveUsers = model.AllUsers - model.ActiveUsers;

            return View(model);
        }

        // Activates or deactivates a user account.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id, string? q, string? status, int page = 1)
        {
            // Load the target user.
            var user = await _userManager.FindByIdAsync(id);
            // Return 404 when the record does not exist.
            if (user == null) return NotFound();

            // Admin accounts and the signed-in user cannot be deactivated.
            if (await _userManager.IsInRoleAsync(user, DbSeeder.AdminRole) || user.Id == _userManager.GetUserId(User))
            {
                TempData["Error"] = "Admin accounts cannot be deactivated.";
                return RedirectToAction(nameof(Index), new { q, status, page });
            }

            // Check whether the account is currently active.
            bool isActive = !(user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow);

            // Deactivate: lock the account indefinitely and invalidate active sessions.
            if (isActive)
            {
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                await _userManager.UpdateSecurityStampAsync(user);
                TempData["Success"] = $"{user.Email} has been deactivated.";
            }
            // Activate: remove the lockout and reset failed attempts.
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                await _userManager.ResetAccessFailedCountAsync(user);
                TempData["Success"] = $"{user.Email} has been activated.";
            }

            return RedirectToAction(nameof(Index), new { q, status, page });
        }
    }

    // Row model for the users table.
    public class UserRow
    {
        public string Id { get; set; } = "";
        public string? FullName { get; set; }
        public string Email { get; set; } = "";
        public string? Country { get; set; }
        public string Role { get; set; } = "User";
        public bool IsActive { get; set; }
    }

    // View model for the users list page.
    public class UsersIndexViewModel
    {
        public List<UserRow> Users { get; set; } = new();
        public string? Query { get; set; }
        public string? Status { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int AllUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
    }
}
