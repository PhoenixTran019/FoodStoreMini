using FoodStore.Application.DTOs.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.Interface.Orders
{
    public interface ICusOrderService
    {
        Task<string> CreateOrderAsync(CreateOrderRequestDto request);

        Task<List<OrderHistoryDto>> GetMyOrderHistoryAsync();

        Task<OrderFullResponseDto> GetOrderDetailAsync(string orderId);

        
    }
}
