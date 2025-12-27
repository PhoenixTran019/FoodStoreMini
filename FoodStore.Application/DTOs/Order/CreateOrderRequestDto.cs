using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.DTOs.Order
{
    public class CreateOrderRequestDto
    {
        public string? DeliveryAddress { get; set; }

        public string? Note { get; set; }

        public List<OrderDetailRequestDto>? Details { get; set; }
    }
}
