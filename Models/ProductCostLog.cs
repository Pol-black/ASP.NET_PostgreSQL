using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RazorPagesMovie.Models;

public partial class ProductCostLog
{
    [Key]
    [Display(Name = "ID записи")]
    public int LogId { get; set; }

    [Required]
    [Display(Name = "ID товара")]
    public int IdProduct { get; set; }

    [Required]
    [Range(0, 999999999999.99)]
    [Display(Name = "Общая стоимость")]
    public decimal TotalCost { get; set; }

    [Display(Name = "Дата обновления")]
    public DateTime UpdatedAt { get; set; }

    [Display(Name = "Товар")]
    public virtual Product IdProductNavigation { get; set; } = null!;
}