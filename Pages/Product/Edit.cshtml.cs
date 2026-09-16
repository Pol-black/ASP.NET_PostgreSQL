using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace RazorPagesMovie.Pages.Product
{
    public class EditModel : PageModel
    {
        private readonly RazorPagesMovie.Models.ArtMarketDbContext _context;

        public EditModel(RazorPagesMovie.Models.ArtMarketDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Models.Product Product { get; set; } = default!;
        [BindProperty]
        public string ReturnUrl { get; set; } = default!;


        public async Task<IActionResult> OnGetAsync(int? id, string? returnUrl)
        {
            if (id == null)
            {
                return NotFound();
            }
            ReturnUrl = Request.Headers["Referer"].ToString() ?? "/Index";


            // Логика возврата: приоритет параметру, затем Referer 
            ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString() ?? "/Product/Index";

            var product = await _context.Products.FirstOrDefaultAsync(m => m.IdProduct == id);
            if (product == null)
            {
                return NotFound();
            }
            Product = product;

            PopulateDropDowns(); 
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                PopulateDropDowns(); // Перезагрузка списков, если форма не валидна [cite: 12]
                return Page();
            }

            // Обработка нулевого покупателя
            if (Product.IdIndivBuyer == 0)
            {
                Product.IdIndivBuyer = null;
            }

            _context.Attach(Product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(Product.IdProduct))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // Возврат на предыдущую страницу 
            return Redirect(ReturnUrl);
        }

        private void PopulateDropDowns()
        {
            // 1. Продавцы: фильтруем по роли "seller" и берем AccountName для отображения 
            var sellers = _context.Accounts
                .Include(a => a.IdRoleNavigation)
                .Where(a => a.IdRoleNavigation.RoleName == "seller")
                .ToList();
            ViewData["IdSeller"] = new SelectList(sellers, "IdAccount", "AccountName", Product?.IdSeller);

            // 2. Покупатели: фильтруем по роли "buyer" и добавляем пустой пункт [cite: 11]
            var buyers = _context.Accounts
                .Include(a => a.IdRoleNavigation)
                .Where(a => a.IdRoleNavigation.RoleName == "buyer")
                .ToList();

            var buyersList = buyers.Select(b => new SelectListItem
            {
                Value = b.IdAccount.ToString(),
                Text = b.AccountName,
                Selected = b.IdAccount == Product?.IdIndivBuyer
            }).ToList();

            buyersList.Insert(0, new SelectListItem { Value = "", Text = "— Не выбран —" });
            ViewData["IdIndivBuyer"] = buyersList;
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.IdProduct == id);
        }
    }
}