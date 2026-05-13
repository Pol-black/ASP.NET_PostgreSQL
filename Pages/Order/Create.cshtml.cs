using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RazorPagesMovie.Pages.Order
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ArtMarketDbContext _context;

        public CreateModel(ArtMarketDbContext context)
        {
            _context = context;
        }

        public Models.Product Product { get; set; }

        [BindProperty]
        public int ProductId { get; set; }

        [BindProperty]
        public decimal Price { get; set; }

        [BindProperty]
        [Range(1, 100, ErrorMessage = "Количество должно быть от 1 до 100")]
        [Display(Name = "Количество")]
        public int Quantity { get; set; } = 1;

        [BindProperty]
        [Required(ErrorMessage = "Способ доставки обязателен")]
        [RegularExpression("^(courier|pickup|mail)$", ErrorMessage = "Неверный способ доставки")]
        [Display(Name = "Способ доставки")]
        public string MethodDelivery { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Адрес получения обязателен")]
        [StringLength(200, ErrorMessage = "Адрес не должен превышать 200 символов")]
        [Display(Name = "Адрес получения")]
        public string AddressReceiving { get; set; }

        [BindProperty]
        [StringLength(500, ErrorMessage = "Комментарий не должен превышать 500 символов")]
        [Display(Name = "Комментарий")]
        public string Comment { get; set; }

        public decimal TotalPrice => Price * Quantity;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.Email);
            if (userIdClaim == null) return RedirectToPage("/Authorization");

            var userEmail = userIdClaim.Value;
            var buyer = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == userEmail);
            if (buyer == null) return RedirectToPage("/Authorization");

            Product = await _context.Products
                .Include(p => p.IdSellerNavigation)
                .FirstOrDefaultAsync(m => m.IdProduct == id);

            if (Product == null)
            {
                TempData["ErrorMessage"] = "? Товар не найден.";
                return RedirectToPage("/Index");
            }

            // Проверяем доступность
            if (Product.Status != "available" &&
                !(Product.IdIndivBuyer.HasValue && Product.IdIndivBuyer.Value == buyer.IdAccount))
            {
                TempData["ErrorMessage"] = "? Этот товар недоступен для заказа.";
                return RedirectToPage("/Index");
            }

            if (Product.IdIndivBuyer == null && Product.QuantityForSale < 1)
            {
                TempData["ErrorMessage"] = "? Товара нет в наличии.";
                return RedirectToPage("/Index");
            }

            ProductId = Product.IdProduct;
            Price = Product.Price;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await ReloadProductData();
                return Page();
            }

            var userIdClaim = User.FindFirst(ClaimTypes.Email);
            if (userIdClaim == null) return RedirectToPage("/Authorization");

            var userEmail = userIdClaim.Value;
            var buyer = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == userEmail);
            if (buyer == null) return RedirectToPage("/Authorization");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Получаем товар с блокировкой
                var product = await _context.Products
                    .FromSqlRaw("SELECT * FROM art_market_schema.product WHERE id_product = {0} FOR UPDATE", ProductId)
                    .Include(p => p.IdSellerNavigation)
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    ModelState.AddModelError(string.Empty, "? Товар не найден.");
                    await ReloadProductData();
                    return Page();
                }

                // Проверяем количество
                if (product.IdIndivBuyer == null)
                {
                    if (product.QuantityForSale < Quantity)
                    {
                        ModelState.AddModelError("Quantity",
                            $"? Недостаточно товара! Доступно: {product.QuantityForSale} шт.");
                        await ReloadProductData();
                        return Page();
                    }

                    // Уменьшаем количество
                    product.QuantityForSale -= Quantity;
                    if (product.QuantityForSale == 0)
                    {
                        product.Status = "sold";
                    }
                    _context.Products.Update(product);
                    await _context.SaveChangesAsync();
                }
                else if (product.IdIndivBuyer.Value != buyer.IdAccount)
                {
                    ModelState.AddModelError(string.Empty, "? Этот индивидуальный товар предназначен для другого покупателя.");
                    await ReloadProductData();
                    return Page();
                }

                // Получаем перевод способа доставки
                string deliveryMethodRu = MethodDelivery switch
                {
                    "courier" => "Курьерская доставка",
                    "pickup" => "Самовывоз",
                    "mail" => "Почтовая доставка",
                    _ => MethodDelivery
                };

                // Создаем заказ
                var newOrder = new Models.Purchase
                {
                    NumberPurchase = $"ORD{DateTime.Now:yyyyMMddHHmmssfff}",
                    IdSeller = product.IdSeller,
                    IdBuyer = buyer.IdAccount,
                    AddressDeparture = product.IdSellerNavigation?.Address ?? "Адрес уточняется",
                    AddressReceiving = AddressReceiving,
                    MethodDelivery = MethodDelivery,
                    CreatedAt = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.Purchases.Add(newOrder);
                await _context.SaveChangesAsync();

                // Создаем позицию заказа
                var orderItem = new Models.ItemPurchase
                {
                    IdPurchase = newOrder.IdPurchase,
                    IdProduct = product.IdProduct,
                    QuantityInPurchase = Quantity,
                    PriceInPurchase = product.Price,
                    StatusItem = "pending"
                };

                _context.ItemPurchases.Add(orderItem);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Успешное сообщение с улучшенным оформлением
                TempData["SuccessMessage"] = GenerateSuccessMessage(newOrder, product, deliveryMethodRu);

                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, $"? Ошибка при создании заказа: {ex.Message}");
                await ReloadProductData();
                return Page();
            }
        }

        private string GenerateSuccessMessage(Models.Purchase order, Models.Product product, string deliveryMethodRu)
        {
            return $@"Заказ успешно оформлен!";
        }

        private async Task ReloadProductData()
        {
            Product = await _context.Products
                .Include(p => p.IdSellerNavigation)
                .FirstOrDefaultAsync(m => m.IdProduct == ProductId);

            if (Product != null)
            {
                Price = Product.Price;
            }
        }
    }
}