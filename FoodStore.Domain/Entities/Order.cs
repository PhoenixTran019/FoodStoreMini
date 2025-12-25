using System;
using System.Collections.Generic;

namespace FoodStore.Domain.Entities;

public partial class Order
{
    public string OrderId { get; set; } = null!;

    public string? CustomerId { get; set; }

    public string? ShipperId { get; set; }

    public string? StatusId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal? TotalAmout { get; set; }

    public string? DeliveryAddress { get; set; }

    public string? Note { get; set; }

    public virtual User? Customer { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<OrderTracking> OrderTrackings { get; set; } = new List<OrderTracking>();

    public virtual User? Shipper { get; set; }

    public virtual OrderStatus? Status { get; set; }
}
