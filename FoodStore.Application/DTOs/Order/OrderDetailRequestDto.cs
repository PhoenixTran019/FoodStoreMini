using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.DTOs.Order
{
    public class OrderDetailRequestDto
    {
        public string? FoodID { get; set; }

        public string? ComboID { get; set; }

        public int? Quantity { get; set; }
    }
}
