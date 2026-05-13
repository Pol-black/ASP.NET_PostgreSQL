using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ItemPurchase;

public class EditModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public EditModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Models.ItemPurchase ItemPurchase { get; set; } = default!;

    public SelectList PurchaseList { get; set; } = default!;
    public SelectList ProductList { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var item = await _context.ItemPurchases.FindAsync(id);
        if (item == null) return NotFound();

        ItemPurchase = item;
        await LoadListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            _context.Attach(ItemPurchase).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ItemExists(ItemPurchase.IdItemPurchase))
                return NotFound();
            throw;
        }

        return RedirectToPage("./Index");
    }

    private async Task LoadListsAsync()
    {
        var purchases = await _context.Purchases
            .Select(p => new { p.IdPurchase, p.NumberPurchase })
            .ToListAsync();

        var products = await _context.Products
            .Select(p => new { p.IdProduct, p.Name })
            .ToListAsync();

        PurchaseList = new SelectList(purchases, "IdPurchase", "NumberPurchase");
        ProductList = new SelectList(products, "IdProduct", "Name");
    }

    private bool ItemExists(int id) =>
        _context.ItemPurchases.Any(e => e.IdItemPurchase == id);
}