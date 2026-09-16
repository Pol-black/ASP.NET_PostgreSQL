using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ProductionPurchase;

public class IndexModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public IndexModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    public IList<Models.ProductionPurchase> ProductionPurchase { get; set; } = new List<Models.ProductionPurchase>();

    public async Task OnGetAsync()
    {
        ProductionPurchase = await _context.ProductionPurchases
            .Include(p => p.IdBuyerNavigation)
            .Include(p => p.IdSellerNavigation)
            .Include(p => p.IdProductNavigation)
            .ToListAsync();

    }
}