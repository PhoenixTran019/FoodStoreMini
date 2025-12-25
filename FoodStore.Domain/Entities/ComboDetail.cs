using System;
using System.Collections.Generic;

namespace FoodStore.Domain.Entities;

public partial class ComboDetail
{
    public string DetailId { get; set; } = null!;

    public string? ComboId { get; set; }

    public string? FoodId { get; set; }

    public int? Quantity { get; set; }

    public virtual Combo? Combo { get; set; }

    public virtual FoodItem? Food { get; set; }
}
