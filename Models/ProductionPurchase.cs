using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models;

public partial class ProductionPurchase
{
    [Display(Name = "ID заказа на изготовление")]
    public int IdProductionPurchase { get; set; }

    [Required(ErrorMessage = "Продавец обязателен")]
    [Display(Name = "Продавец")]
    public int IdSeller { get; set; }

    [Required(ErrorMessage = "Покупатель обязателен")]
    [Display(Name = "Покупатель")]
    public int IdBuyer { get; set; }

    [Display(Name = "Направление от продавца")]
    public bool DirectionFromSeller { get; set; }

    [StringLength(10000, ErrorMessage = "Текст слишком длинный")]
    [Display(Name = "Сообщение")]
    public string? TextAccounts { get; set; }

    [Display(Name = "Товар")]
    public int? IdProduct { get; set; }

    
    public virtual Account IdBuyerNavigation { get; set; } = null!;
    public virtual Product? IdProductNavigation { get; set; }
    public virtual Account IdSellerNavigation { get; set; } = null!;
}
