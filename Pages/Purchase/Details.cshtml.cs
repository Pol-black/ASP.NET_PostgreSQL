using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Purchase;

public class DetailsModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public DetailsModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    public Models.Purchase Purchase { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var purchase = await _context.Purchases
            .Include(p => p.IdBuyerNavigation)
            .Include(p => p.IdSellerNavigation)
            .FirstOrDefaultAsync(m => m.IdPurchase == id);

        if (purchase == null) return NotFound();

        Purchase = purchase;
        return Page();
    }
}