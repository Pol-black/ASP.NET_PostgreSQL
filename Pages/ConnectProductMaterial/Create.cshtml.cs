using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorPagesMovie.Pages.ConnectProductMaterial;

public class CreateModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public CreateModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Models.ConnectProductMaterial ConnectProductMaterial { get; set; } = default!;

    public SelectList ProductList { get; set; } = default!;
    public SelectList MaterialList { get; set; } = default!;

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
            _context.ConnectProductMaterials.Add(ConnectProductMaterial);
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
        var products = await _context.Products
            .Select(p => new { p.IdProduct, p.Name })
            .ToListAsync();

        var materials = await _context.CatalogForMaterials
            .Select(m => new { m.IdMaterial, m.MaterialName })
            .ToListAsync();

        ProductList = new SelectList(products, "IdProduct", "Name");
        MaterialList = new SelectList(materials, "IdMaterial", "MaterialName");
    }
}