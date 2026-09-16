using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Purchase;

public class EditModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public EditModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Models.Purchase Purchase { get; set; } = default!;

    public SelectList BuyerList { get; set; } = default!;
    public SelectList SellerList { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var purchase = await _context.Purchases.FindAsync(id);
        if (purchase == null) return NotFound();

        Purchase = purchase;
        await LoadAccountListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            _context.Attach(Purchase).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PurchaseExists(Purchase.IdPurchase))
                return NotFound();
            throw;
        }

        return RedirectToPage("./Index");
    }

    private async Task LoadAccountListsAsync()
    {
        var accounts = await _context.Accounts
            .Select(a => new { a.IdAccount, a.AccountName })
            .ToListAsync();

        BuyerList = new SelectList(accounts, "IdAccount", "AccountName");
        SellerList = new SelectList(accounts, "IdAccount", "AccountName");
    }

    private bool PurchaseExists(int id) =>
        _context.Purchases.Any(e => e.IdPurchase == id);
}