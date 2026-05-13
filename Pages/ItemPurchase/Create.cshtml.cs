using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ItemPurchase;

public class CreateModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public CreateModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Models.ItemPurchase ItemPurchase { get; set; } = default!;

    public SelectList PurchaseList { get; set; } = default!;
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
            _context.ItemPurchases.Add(ItemPurchase);
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
        var purchases = await _context.Purchases
            .Select(p => new { p.IdPurchase, p.NumberPurchase })
            .ToListAsync();

        var products = await _context.Products
            .Select(p => new { p.IdProduct, p.Name })
            .ToListAsync();

        PurchaseList = new SelectList(purchases, "IdPurchase", "NumberPurchase");
        ProductList = new SelectList(products, "IdProduct", "Name");
    }
}