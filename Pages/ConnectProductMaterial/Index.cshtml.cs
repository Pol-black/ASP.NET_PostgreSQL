using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ConnectProductMaterial;

public class IndexModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public IndexModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    public IList<Models.ConnectProductMaterial> ConnectProductMaterial { get; set; } = new List<Models.ConnectProductMaterial>();

    public async Task OnGetAsync()
    {
        ConnectProductMaterial = await _context.ConnectProductMaterials
            .Include(c => c.IdProductNavigation)
            .Include(c => c.IdMaterialNavigation)
            .ToListAsync();
    }
}