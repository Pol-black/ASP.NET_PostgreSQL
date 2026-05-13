using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ProductionPurchase;

public class EditModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public EditModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Models.ProductionPurchase ProductionPurchase { get; set; } = default!;

    public SelectList SellerList { get; set; } = default!;
    public SelectList BuyerList { get; set; } = default!;
    public SelectList ProductList { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var item = await _context.ProductionPurchases.FindAsync(id);
        if (item == null) return NotFound();

        ProductionPurchase = item;
        await LoadListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            _context.Attach(ProductionPurchase).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ItemExists(ProductionPurchase.IdProductionPurchase))
                return NotFound();
            throw;
        }

        return RedirectToPage("./Index");
    }

    private async Task LoadListsAsync()
    {
        var accounts = await _context.Accounts
            .Select(a => new { a.IdAccount, a.AccountName })
            .ToListAsync();

        var products = await _context.Products
            .Select(p => new { p.IdProduct, p.Name })
            .ToListAsync();

        SellerList = new SelectList(accounts, "IdAccount", "AccountName");
        BuyerList = new SelectList(accounts, "IdAccount", "AccountName");
        ProductList = new SelectList(products, "IdProduct", "Name");
    }

    private bool ItemExists(int id) =>
        _context.ProductionPurchases.Any(e => e.IdProductionPurchase == id);
}