using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Chat
{
    public class RoomModel : PageModel
    {
        private readonly ArtMarketDbContext _context;
        public RoomModel(ArtMarketDbContext context)
        {
            _context = context;
        }
        public List<Models.ProductionPurchase> ChatMessages { get; set; } = new();
        public Models.Account CurrentUser { get; set; }
        public Models.Account SellerUser { get; private set; }
        public Models.Account BuyerUser { get; private set; }
        public int? CurrentUserId { get; private set; }

        [BindProperty] // прив€зка пол€ к форме в HTML
        public string NewMessage { get; set; }


        public async Task OnGetAsync(int idSeller, int idBuyer)
        {
            Console.WriteLine($"Ќачало чата между idSeller: {idSeller}, idBuyer: {idBuyer}");

            if (int.TryParse(User.FindFirst("idaccount")?.Value, out int id))
            {
                CurrentUserId = id;
            }
            SellerUser = await _context.Accounts.FindAsync(idSeller);
            BuyerUser = await _context.Accounts.FindAsync(idBuyer);
            CurrentUser = await _context.Accounts
                .Include(a => a.IdRoleNavigation) // «агружаем словесное описание роли
                .FirstOrDefaultAsync(a => a.IdAccount == CurrentUserId);

            ChatMessages = await _context.ProductionPurchases
                .Include(p => p.IdBuyerNavigation)
                .Include(p => p.IdSellerNavigation)
                .Where(p => p.IdSeller == idSeller)
                .Where(p => p.IdBuyer == idBuyer)
                ?.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(int idSeller, int idBuyer)
        {
            if (string.IsNullOrWhiteSpace(NewMessage))
            {   // ≈сли сообщение пустое, просто перезагружаем страницу
                return RedirectToPage(new { idSeller, idBuyer });
            }

            // ѕолучаем ID текущего пользовател€ из сессии/Claims
            if (int.TryParse(User.FindFirst("idaccount")?.Value, out int id))
            {
                CurrentUserId = id;
            }

            // —оздаем новый объект сообщени€ 
            var message = new Models.ProductionPurchase
            {
                IdSeller = idSeller,
                IdBuyer = idBuyer,
                TextAccounts = NewMessage, // текст сообщени€ из формы html
                // ≈сли текущий пользователь Ч продавец, флаг устанавливаетс€ в true
                DirectionFromSeller = (CurrentUserId == idSeller)
            };

            _context.ProductionPurchases.Add(message);
            await _context.SaveChangesAsync();

            // ѕеренаправл€ем пользовател€ обратно в этот же чат, чтобы увидеть новое сообщение
            return RedirectToPage(new { idSeller, idBuyer });
        }
    }
}