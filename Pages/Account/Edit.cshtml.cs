using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Account;

public class EditModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public EditModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Models.Account Account { get; set; } = default!;

    public SelectList RoleList { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var account = await _context.Accounts.FirstOrDefaultAsync(m => m.IdAccount == id);
        if (account == null) return NotFound();

        Account = account;

        // Заполняем список ролей
        var roles = await _context.Roles.ToListAsync();
        RoleList = new SelectList(roles, "IdRole", "RoleName");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            _context.Attach(Account).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!AccountExists(Account.IdAccount))
                return NotFound();
            throw;
        }

        return RedirectToPage("./Index");
    }

    private bool AccountExists(int id) => _context.Accounts.Any(e => e.IdAccount == id);
}