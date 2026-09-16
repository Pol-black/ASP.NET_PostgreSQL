using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ProductionPurchase;

public class DetailsModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public DetailsModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    public Models.ProductionPurchase ProductionPurchase { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var item = await _context.ProductionPurchases
            .Include(p => p.IdBuyerNavigation)
            .Include(p => p.IdSellerNavigation)
            .Include(p => p.IdProductNavigation)
            .FirstOrDefaultAsync(m => m.IdProductionPurchase == id);

        if (item == null) return NotFound();

        ProductionPurchase = item;
        return Page();
    }
}