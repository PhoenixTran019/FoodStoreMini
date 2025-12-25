using System;
using System.Collections.Generic;

namespace FoodStore.Domain.Entities;

public partial class FoodItem
{
    public string FoodId { get; set; } = null!;

    public string? CategoryId { get; set; }

    public string? FoodName { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsAvailable { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<ComboDetail> ComboDetails { get; set; } = new List<ComboDetail>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
