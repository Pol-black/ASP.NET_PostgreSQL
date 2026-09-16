using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ConnectProductMaterial;

public class DetailsModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public DetailsModel(ArtMarketDbContext context)
    {
        _context = context;
    }

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
}