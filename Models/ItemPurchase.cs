using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models;

public partial class ItemPurchase
{
    [Display(Name = "ID позиции заказа")]
    public int IdItemPurchase { get; set; }

    [Required(ErrorMessage = "Заказ обязателен")]
    [Display(Name = "ID заказа")]
    public int IdPurchase { get; set; }

    [Required(ErrorMessage = "Товар обязателен")]
    [Display(Name = "Id товара")]
    public int IdProduct { get; set; }


    [Required(ErrorMessage = "Количество в заказе обязательно")]
    [Range(1, 100, ErrorMessage = "Количество должно быть от 1 до 100")]
    [Display(Name = "Количество в заказе")]
    public int QuantityInPurchase { get; set; }

    [Required(ErrorMessage = "Цена в заказе обязательна")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть положительной")]
    [Display(Name = "Цена заказа")]
    public decimal PriceInPurchase { get; set; }

    [Required(ErrorMessage = "Статус позиции обязателен")]
    [RegularExpression(
       @"^(pending|shipped|delivered|cancelled|accepted|in progress)$",
       ErrorMessage = "Недопустимый статус. Допустимые значения: pending, shipped, delivered, cancelled, accepted, in progress."
   )]
    [StringLength(30, ErrorMessage = "Статус не должен превышать 30 символов")]
    [Display(Name = "Статус заказа")]
    public string StatusItem { get; set; } = null!;
    [Display(Name = "Товар")]
    public virtual Product IdProductNavigation { get; set; } = null!;
    [Display(Name = "Заказ")]
    public virtual Purchase IdPurchaseNavigation { get; set; } = null!;
}
