using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;
using System.Security.Claims;

namespace RazorPagesMovie.Pages.Chat
{
    public class SelectChatPartnerModel : PageModel
    {
        private readonly ArtMarketDbContext _context;

        public SelectChatPartnerModel(ArtMarketDbContext context)
        {
            _context = context;
        }

        public string? Name { get; private set; }
        public string? Role { get; private set; }
        public string? Email { get; private set; }
        public int? IdAccount { get; private set; }

        // Списки покупателей и продавцов для выбора чата
        public IList<Models.Account> Buyers { get; set; } = default!;
        public IList<Models.Account> Sellers { get; set; } = default!;

        public async Task OnGetAsync()
        {
            // Получение данных текущего пользователя из Claims
            Name = User.FindFirst(ClaimTypes.Name)?.Value;
            Role = User.FindFirst(ClaimTypes.Role)?.Value;
            Email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (int.TryParse(User.FindFirst("idaccount")?.Value, out int id))
            {
                IdAccount = id;
            }

            // Запрос к БД: выбираем списком аккаунты, где роль "buyer" и "seller"
            // Также исключаем из списка самого себя (текущего пользователя)
            Buyers = await _context.Accounts
                .Where(a => a.IdRoleNavigation.RoleName == "buyer" && a.IdAccount != IdAccount)
                .Include(a => a.IdRoleNavigation)
                .ToListAsync();

            Sellers = await _context.Accounts
                .Where(a => a.IdRoleNavigation.RoleName == "seller" && a.IdAccount != IdAccount)
                .Include(a => a.IdRoleNavigation)
                .ToListAsync();
        }
    }
}