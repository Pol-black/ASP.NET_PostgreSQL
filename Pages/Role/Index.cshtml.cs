using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Role;

public class IndexModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public IndexModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    public IList<Models.Role> Role { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Role = await _context.Roles.ToListAsync();
    }
}
