using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.DTOs.Orders
{
    public class OrderDetailDto
    {
        public string OrderDetailId { get; set; }
        public string? FoodName { get; set; }
        public string? ComboName { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? SubTotal => Quantity * UnitPrice;
    }
}
