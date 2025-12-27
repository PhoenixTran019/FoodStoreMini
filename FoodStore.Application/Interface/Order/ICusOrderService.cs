using FoodStore.Application.DTOs.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.Interface.Order
{
    public interface ICusOrderService
    {
        Task<string> CreateOrderAsync(CreateOrderRequestDto request);


    }
}
