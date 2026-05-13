using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.CatalogForMaterial;

public class DetailsModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public DetailsModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    public Models.CatalogForMaterial CatalogForMaterial { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var material = await _context.CatalogForMaterials.FirstOrDefaultAsync(m => m.IdMaterial == id);
        if (material == null) return NotFound();

        CatalogForMaterial = material;
        return Page();
    }
}