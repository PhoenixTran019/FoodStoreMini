using FoodStore.Application.DTOs.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.Interface.Orders
{
    public interface IAdminOrderService
    {
        Task<bool> UpdateOrderStatusAsync(string orderId,UpdateOrderStatusDto request, string userID);

        Task<List<OrderHistoryDto>> GetOrdersByStatusAsync(List<string> statusNames, string? customerId = null);

        Task<List<OrderHistoryDto>> GetOrdersByCustomerAsync(string customerProfileId);

        Task<List<OrderHistoryDto>> GetOrdersByTimeRangeAsync(DateTime start, DateTime end);
    }
}
