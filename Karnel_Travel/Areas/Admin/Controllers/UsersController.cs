using Karnel_Travel.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karnel_Travel.Areas.Admin.Controllers
{
    // Admin ke liye Users page: registered users ki list, Active/Deactive option aur pagination (10 per page)
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        // Ek page par kitne users dikhane hain
        private const int PageSize = 10;

        private readonly UserManager<ApplicationUser> _userManager;

        // Constructor: ASP.NET khud UserManager de deta hai (dependency injection)
        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: Admin/Users
        // q = naam/email search, status = "active" ya "inactive", page = page number
        public async Task<IActionResult> Index(string? q, string? status, int page = 1)
        {
            var now = DateTimeOffset.UtcNow;
            var query = _userManager.Users.AsQueryable();

            // Naam ya email se search
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(u => (u.FullName ?? "").Contains(q) || (u.Email ?? "").Contains(q));
            }

            // Active = lockout nahi laga hua. Deactive = lockout aage tak laga hua hai
            if (status == "active")
                query = query.Where(u => u.LockoutEnd == null || u.LockoutEnd <= now);
            else if (status == "inactive")
                query = query.Where(u => u.LockoutEnd != null && u.LockoutEnd > now);

            // Pagination ka hisab
            int totalItems = await query.CountAsync();
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize));
            page = Math.Clamp(page, 1, totalPages);

            var users = await query
                .OrderBy(u => u.Email)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Is page ke users ki rows tayyar karo (role ke saath)
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

            var model = new UsersIndexViewModel
            {
                Users = rows,
                Query = q,
                Status = status,
                Page = page,
                PageSize = PageSize,
                TotalPages = totalPages,
                TotalItems = totalItems,
                // Upar ke subtitle ke liye poore system ki ginti
                AllUsers = await _userManager.Users.CountAsync(),
                ActiveUsers = await _userManager.Users.CountAsync(u => u.LockoutEnd == null || u.LockoutEnd <= now)
            };
            // Deactive = total - active
            model.InactiveUsers = model.AllUsers - model.ActiveUsers;

            return View(model);
        }

        // POST: Admin/Users/ToggleStatus
        // Active user ko Deactive aur Deactive ko Active kar deta hai
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id, string? q, string? status, int page = 1)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Admin account ya apna khud ka account deactivate nahi ho sakta
            if (await _userManager.IsInRoleAsync(user, DbSeeder.AdminRole) || user.Id == _userManager.GetUserId(User))
            {
                TempData["Error"] = "Admin accounts cannot be deactivated.";
                return RedirectToAction(nameof(Index), new { q, status, page });
            }

            bool isActive = !(user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow);

            if (isActive)
            {
                // Deactivate: lockout hamesha ke liye laga do (Login par block ho jayega)
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                // Security stamp badalne se agar user abhi login hai to wo bhi logout ho jayega
                await _userManager.UpdateSecurityStampAsync(user);
                TempData["Success"] = $"{user.Email} has been deactivated.";
            }
            else
            {
                // Activate: lockout hata do
                await _userManager.SetLockoutEndDateAsync(user, null);
                await _userManager.ResetAccessFailedCountAsync(user);
                TempData["Success"] = $"{user.Email} has been activated.";
            }

            return RedirectToAction(nameof(Index), new { q, status, page });
        }
    }

    // Table ki ek row ka data
    public class UserRow
    {
        public string Id { get; set; } = "";
        public string? FullName { get; set; }
        public string Email { get; set; } = "";
        public string? Country { get; set; }
        public string Role { get; set; } = "User";
        public bool IsActive { get; set; }
    }

    // Poore Users page ka data (list + filters + pagination)
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
