using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ItemPurchase;

public class IndexModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public IndexModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    public IList<Models.ItemPurchase> ItemPurchase { get; set; } = new List<Models.ItemPurchase>();

    public async Task OnGetAsync()
    {
        ItemPurchase = await _context.ItemPurchases
            .Include(i => i.IdProductNavigation)
            .Include(i => i.IdPurchaseNavigation)
            .ToListAsync();
    }
}