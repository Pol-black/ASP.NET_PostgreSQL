using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ProductionPurchase;

public class DeleteModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public DeleteModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    [BindProperty]
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

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null) return NotFound();

        var item = await _context.ProductionPurchases.FindAsync(id);
        if (item != null)
        {
            _context.ProductionPurchases.Remove(item);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}