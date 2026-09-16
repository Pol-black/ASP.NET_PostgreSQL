using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.CatalogForMaterial;

public class CreateModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public CreateModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Models.CatalogForMaterial CatalogForMaterial { get; set; } = default!;

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            _context.CatalogForMaterials.Add(CatalogForMaterial);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Ошибка при сохранении: " + ex.Message);
            return Page();
        }
    }
}