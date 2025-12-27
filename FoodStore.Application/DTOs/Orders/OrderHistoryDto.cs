using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.DTOs.Orders
{
    public class OrderHistoryDto
    {
        public string? OrderID { get; set; }

        public DateTime? OrderDate { get; set; }

        public decimal? TotalAmount { get; set; }

        public string? StatusName { get; set; }

        public string? DeliveryAddress { get; set; }

        public string? Note { get; set; }

        public string? CustomerFullName { get; set; }
    }
}
