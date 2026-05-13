using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;
using System;

namespace RazorPagesMovie.Pages.Seller
{
    public class CreateCustomProductModel : PageModel
    {
        private readonly ArtMarketDbContext _context;

        public CreateCustomProductModel(ArtMarketDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Models.ProductionPurchase Order { get; set; }

        [BindProperty]
        public Models.Product CustomProduct { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Для тестирования используем ID продавца = 2
            var currentUserId = 2;

            Order = await _context.ProductionPurchases
                .Include(p => p.IdBuyerNavigation)
                .FirstOrDefaultAsync(m => m.IdProductionPurchase == id);

            if (Order == null)
            {
                return NotFound();
            }

            if (Order.IdProduct.HasValue)
            {
                // Товар уже создан
                return RedirectToPage("/Seller/Index");
            }

            CustomProduct = new Models.Product
            {
                IdSeller = currentUserId,
                IdIndivBuyer = Order.IdBuyer,
                QuantityForSale = 1,
                Status = "reserved"
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var currentUserId = 2; // ID тестового продавца

            try
            {
                // Для теста просто создаем товар вручную вместо вызова процедуры
                var newProduct = new Models.Product
                {
                    Name = CustomProduct.Name,
                    TypeArt = CustomProduct.TypeArt,
                    IdSeller = CustomProduct.IdSeller,
                    IdIndivBuyer = CustomProduct.IdIndivBuyer,
                    QuantityForSale = 1,
                    Price = CustomProduct.Price,
                    Status = "reserved"
                };

                _context.Products.Add(newProduct);
                await _context.SaveChangesAsync();

                // Обновляем заказ
                var order = await _context.ProductionPurchases.FindAsync(id);
                if (order != null)
                {
                    order.IdProduct = newProduct.IdProduct;
                    await _context.SaveChangesAsync();
                }

                return RedirectToPage("/Seller/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ошибка при создании товара: {ex.Message}");
                return Page();
            }
        }
    }
}