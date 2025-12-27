using System;
using System.Collections.Generic;

namespace FoodStore.Domain.Entities;

public partial class OrderTracking
{
    public string TrackingId { get; set; } = null!;

    public string? OrderId { get; set; }

    public string? StatusId { get; set; }

    public DateTime? UpdateTime { get; set; }

    public string? StaffNote { get; set; }

    public virtual Order? Order { get; set; }

    public virtual OrderStatus? Status { get; set; }
}
