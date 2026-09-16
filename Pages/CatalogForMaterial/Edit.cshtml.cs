using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.CatalogForMaterial;

public class EditModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public EditModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Models.CatalogForMaterial CatalogForMaterial { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var material = await _context.CatalogForMaterials.FirstOrDefaultAsync(m => m.IdMaterial == id);
        if (material == null) return NotFound();

        CatalogForMaterial = material;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            _context.Attach(CatalogForMaterial).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CatalogForMaterialExists(CatalogForMaterial.IdMaterial))
                return NotFound();
            throw;
        }

        return RedirectToPage("./Index");
    }

    private bool CatalogForMaterialExists(int id) =>
        _context.CatalogForMaterials.Any(e => e.IdMaterial == id);
}