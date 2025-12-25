using System;
using System.Collections.Generic;

namespace FoodStore.Domain.Entities;

public partial class Combo
{
    public string ComboId { get; set; } = null!;

    public string? ComboName { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsAvailable { get; set; }

    public virtual ICollection<ComboDetail> ComboDetails { get; set; } = new List<ComboDetail>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
