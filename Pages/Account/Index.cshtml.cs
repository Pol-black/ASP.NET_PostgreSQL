using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Account
{
    public class IndexModel : PageModel
    {
        private readonly ArtMarketDbContext _context;

        public IndexModel(ArtMarketDbContext context)
        {
            _context = context;
        }

        public string SearchString { get; set; } = "";
        public string RoleFilter { get; set; } = "";

        public IList<Models.Account> Account { get; set; } = default!;

        public async Task OnGetAsync(string search, string role)
        {
            SearchString = search ?? "";
            RoleFilter = role ?? "";

            var query = _context.Accounts
                .Include(a => a.IdRoleNavigation)
                .AsQueryable();

            // Поиск по имени, email или телефону
            if (!string.IsNullOrEmpty(SearchString))
            {
                query = query.Where(a =>
                    a.AccountName.Contains(SearchString) ||
                    a.Email.Contains(SearchString) ||
                    a.Phone.Contains(SearchString));
            }

            // Фильтр по роли
            if (!string.IsNullOrEmpty(RoleFilter))
            {
                query = query.Where(a => a.IdRoleNavigation.RoleName == RoleFilter);
            }

            Account = await query.ToListAsync();
        }
    }
}