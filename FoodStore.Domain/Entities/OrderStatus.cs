using System;
using System.Collections.Generic;

namespace FoodStore.Domain.Entities;

public partial class OrderStatus
{
    public string StatusId { get; set; } = null!;

    public string? StatusName { get; set; }

    public virtual ICollection<OrderTracking> OrderTrackings { get; set; } = new List<OrderTracking>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
