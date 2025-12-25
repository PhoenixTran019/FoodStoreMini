using System;
using System.Collections.Generic;

namespace FoodStore.Domain.Entities;

public partial class OrderDetail
{
    public string OrderDetailId { get; set; } = null!;

    public string? OrderId { get; set; }

    public string? FoodId { get; set; }

    public string? ComboId { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public virtual Combo? Combo { get; set; }

    public virtual FoodItem? Food { get; set; }

    public virtual Order? Order { get; set; }
}
