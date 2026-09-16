using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Account;

public class DetailsModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public DetailsModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    public Models.Account Account { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var account = await _context.Accounts
            .Include(a => a.IdRoleNavigation)
            .FirstOrDefaultAsync(m => m.IdAccount == id);

        if (account == null) return NotFound();

        Account = account;
        return Page();
    }
}