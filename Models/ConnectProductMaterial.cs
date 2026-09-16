using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models;

public partial class ConnectProductMaterial
{
    [Display(Name = "ID связи")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Товар обязателен")]
    [Display(Name = "ID товара")]
    public int IdProduct { get; set; }

    [Required(ErrorMessage = "Материал обязателен")]
    [Display(Name = "ID материала")]
    public int IdMaterial { get; set; }


    [Required(ErrorMessage = "Количество обязательно")]
    [Range(0.001, 999999999.999, ErrorMessage = "Количество должно быть положительным (макс. 10 цифр, до 3 знаков после запятой)")]
    [Display(Name = "Вес/Количество")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "Единица измерения обязательна")]
    [StringLength(20, MinimumLength = 1, ErrorMessage = "Единица измерения должна содержать от 1 до 20 символов")]
    [Display(Name = "Ед. измерения")]
    public string Unit { get; set; } = null!;

    [Display(Name = "Материла")]
    public virtual CatalogForMaterial IdMaterialNavigation { get; set; } = null!;
    [Display(Name = "Товар")]
    public virtual Product IdProductNavigation { get; set; } = null!;
}
