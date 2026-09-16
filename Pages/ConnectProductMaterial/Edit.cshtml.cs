using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ConnectProductMaterial;

public class EditModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public EditModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Models.ConnectProductMaterial ConnectProductMaterial { get; set; } = default!;

    public SelectList ProductList { get; set; } = default!;
    public SelectList MaterialList { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var item = await _context.ConnectProductMaterials.FindAsync(id);
        if (item == null) return NotFound();

        ConnectProductMaterial = item;
        await LoadListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            _context.Attach(ConnectProductMaterial).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ItemExists(ConnectProductMaterial.Id))
                return NotFound();
            throw;
        }

        return RedirectToPage("./Index");
    }

    private async Task LoadListsAsync()
    {
        var products = await _context.Products
            .Select(p => new { p.IdProduct, p.Name })
            .ToListAsync();

        var materials = await _context.CatalogForMaterials
            .Select(m => new { m.IdMaterial, m.MaterialName })
            .ToListAsync();

        ProductList = new SelectList(products, "IdProduct", "Name");
        MaterialList = new SelectList(materials, "IdMaterial", "MaterialName");
    }

    private bool ItemExists(int id) =>
        _context.ConnectProductMaterials.Any(e => e.Id == id);
}