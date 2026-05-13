using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorPagesMovie.Pages.Purchase;

public class CreateModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public CreateModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Models.Purchase Purchase { get; set; } = default!;

    public SelectList BuyerList { get; set; } = default!;
    public SelectList SellerList { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadAccountListsAsync();
        Purchase.CreatedAt = DateOnly.FromDateTime(DateTime.Today); // по умолчанию — сегодня
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadAccountListsAsync();
            return Page();
        }

        try
        {
            _context.Purchases.Add(Purchase);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Ошибка при сохранении: " + ex.Message);
            await LoadAccountListsAsync();
            return Page();
        }
    }

    private async Task LoadAccountListsAsync()
    {
        var accounts = await _context.Accounts
            .Select(a => new { a.IdAccount, a.AccountName })
            .ToListAsync();

        BuyerList = new SelectList(accounts, "IdAccount", "AccountName");
        SellerList = new SelectList(accounts, "IdAccount", "AccountName");
    }
}