using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ProductionPurchase;

public class CreateModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public CreateModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Models.ProductionPurchase ProductionPurchase { get; set; } = default!;

    public SelectList SellerList { get; set; } = default!;
    public SelectList BuyerList { get; set; } = default!;
    public SelectList ProductList { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadListsAsync();
            return Page();
        }

        try
        {
            _context.ProductionPurchases.Add(ProductionPurchase);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Ошибка при сохранении: " + ex.Message);
            await LoadListsAsync();
            return Page();
        }
    }

    private async Task LoadListsAsync()
    {
        var accountsSeller = await _context.Accounts
            .Where(a => a.IdRoleNavigation != null && a.IdRoleNavigation.RoleName == "seller")
            .Select(a => new { a.IdAccount, a.AccountName })
            .ToListAsync();

        var accountsBuyer = await _context.Accounts
            .Where(a => a.IdRoleNavigation != null && a.IdRoleNavigation.RoleName == "buyer")
            .Select(a => new { a.IdAccount, a.AccountName })
            .ToListAsync();

        var products = await _context.Products
            .Select(p => new { p.IdProduct, p.Name })
            .ToListAsync();

        SellerList = new SelectList(accountsSeller, "IdAccount", "AccountName");
        BuyerList = new SelectList(accountsBuyer, "IdAccount", "AccountName");
        ProductList = new SelectList(products, "IdProduct", "Name");
    }
}