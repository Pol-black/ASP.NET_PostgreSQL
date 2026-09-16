using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ConnectProductMaterial;

public class DeleteModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public DeleteModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Models.ConnectProductMaterial ConnectProductMaterial { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var item = await _context.ConnectProductMaterials
            .Include(c => c.IdProductNavigation)
            .Include(c => c.IdMaterialNavigation)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (item == null) return NotFound();

        ConnectProductMaterial = item;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null) return NotFound();

        var item = await _context.ConnectProductMaterials.FindAsync(id);
        if (item != null)
        {
            _context.ConnectProductMaterials.Remove(item);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}