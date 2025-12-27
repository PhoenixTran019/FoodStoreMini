using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.DTOs.Orders
{
    public class OrderTrackingDto
    {
        public string? StatusName { get; set; }

        public DateTime? UpdateTime { get; set; }
    }
}
