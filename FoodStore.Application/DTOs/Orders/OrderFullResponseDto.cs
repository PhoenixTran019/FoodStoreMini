using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.DTOs.Orders
{
    public class OrderFullResponseDto
    {
        public string? OrderId { get; set; }

        public DateTime? OrderDate { get; set; }

        public string? StatusName { get; set; }

        public string? CustomerName { get; set; }

        public string? DeliveryAddress { get; set; }

        public string? Note { get; set; }

        public string? ShipperName { get; set; } // Để biết ai đang giao

        public decimal? TotalAmount { get; set; }

        public List<OrderDetailDto>? Items { get; set; }
    }
}
