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
        Task<string> CreateOrderAsync(CreateOrderRequestDto request, string customerID);

        Task<List<OrderHistoryDto>> GetActiveOrdersAsync(string customerID);

        Task<List<OrderHistoryDto>> GetMyOrderHistoryAsync(string customerID);

        Task<OrderFullResponseDto> GetOrderDetailAsync(string orderId, string? customerID = null, bool isAdminOrStaff = false);



    }
}
