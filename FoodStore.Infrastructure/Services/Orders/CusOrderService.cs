using FoodStore.Application.Common.Helper;
using FoodStore.Application.DTOs.Order;
using FoodStore.Application.Interface.Order;
using FoodStore.Domain.Data;
using FoodStore.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Infrastructure.Services.Orders
{
    public class CusOrderService : ICusOrderService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContext;

        public CusOrderService(AppDbContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;
        }

        public async Task<string> CreateOrderAsync(CreateOrderRequestDto request)
        {
            var pendingStatus = await _context.OrderStatuses
                .FirstOrDefaultAsync (s => s.StatusName == "Pending");

            //Take UserID from JWT Token
            var customerId = _httpContext.HttpContext.User?.Claims
                                .FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            var orderId = Uuidv7Generator.NewUuid7().ToString();

            if (string.IsNullOrEmpty(customerId)) throw new UnauthorizedAccessException("Need Login.");
            //Create Order
            var order = new Order
            {
                OrderId = orderId,
                CustomerId = customerId,
                OrderDate = DateTime.UtcNow,
                DeliveryAddress = request.DeliveryAddress,
                Note = request.Note,
                StatusId = pendingStatus?.StatusId, //Take ID of "Pending" status
                TotalAmout = 0
            };

            decimal? total = 0;
            var details = new List<OrderDetail>();

            //Handing Order or combo
            foreach (var item in request.Details)
            {
                decimal? unitPrice = 0;
                if (!string.IsNullOrEmpty(item.FoodID))
                {
                    var food = await _context.FoodItems.FindAsync(item.FoodID);
                    unitPrice = food?.Price ?? 0;
                }
                else if(!string.IsNullOrEmpty(item.ComboID))
                {
                    var combo = await _context.Combos.FindAsync(item.ComboID);
                    unitPrice = combo?.Price ?? 0;
                }
                var orderDetail = new OrderDetail
                {
                    OrderDetailId = Uuidv7Generator.NewUuid7().ToString(),
                    OrderId = orderId,
                    FoodId = item.FoodID,
                    ComboId = item.ComboID,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice
                };

                total += (unitPrice * item.Quantity);
                details.Add(orderDetail);
            }
            order.TotalAmout = total;

            //Execute transactions to protect data.
            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Orders.Add(order);
                _context.OrderDetails.AddRange(details);

                //Write action log
                _context.ActivityLogs.Add(new ActivityLog
                {
                    LogId = Uuidv7Generator.NewUuid7().ToString(),
                    UserId = _httpContext.HttpContext?.User?.FindFirstValue("ProfileId") ?? customerId,
                    Action = "Create New Order",
                    TagetTable = "Orders",
                    TargetId = order.OrderId,
                    TargetName = $"OrderID: {order.OrderDate:yyyyMMdd}",
                    TimeStamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await trans.CommitAsync();
                return order.OrderId;
            }
            catch
            {
                await trans.RollbackAsync();
                throw;
            }
        }
    }
}
