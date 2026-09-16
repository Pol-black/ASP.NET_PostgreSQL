using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.CatalogForMaterial;

public class DeleteModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public DeleteModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Models.CatalogForMaterial CatalogForMaterial { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var material = await _context.CatalogForMaterials.FindAsync(id);
        if (material == null) return NotFound();

        CatalogForMaterial = material;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null) return NotFound();

        var material = await _context.CatalogForMaterials.FindAsync(id);
        if (material != null)
        {
            _context.CatalogForMaterials.Remove(material);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}