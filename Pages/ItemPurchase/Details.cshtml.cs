using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ItemPurchase;

public class DetailsModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public DetailsModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    public Models.ItemPurchase ItemPurchase { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var item = await _context.ItemPurchases
            .Include(i => i.IdProductNavigation)
            .Include(i => i.IdPurchaseNavigation)
            .FirstOrDefaultAsync(m => m.IdItemPurchase == id);

        if (item == null) return NotFound();

        ItemPurchase = item;
        return Page();
    }
}