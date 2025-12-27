using FoodStore.Application.Common.Helper;
using FoodStore.Application.DTOs.Orders;
using FoodStore.Application.Interface.Orders;
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

        public async Task<List<OrderHistoryDto>> GetMyOrderHistoryAsync()
        {
            var customerId = _httpContext.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            return await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Join(_context.OrderStatuses,
                    o => o.StatusId,
                    s => s.StatusId,
                    (o, s) => new OrderHistoryDto
                    {
                        OrderID = o.OrderId,
                        OrderDate = o.OrderDate,
                        TotalAmount = o.TotalAmout,
                        StatusName = s.StatusName,
                        DeliveryAddress = o.DeliveryAddress,
                        Note = o.Note
                    })
                .OrderByDescending(x => x.OrderDate)
                .ToListAsync();
        }

        public async Task<OrderFullResponseDto> GetOrderDetailAsync(string orderId)
        {
            // 1. Lấy thông tin đơn hàng
            var order = await _context.Orders
                .Include(o => o.Status)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return null;

            // 2. Lấy thông tin Khách hàng (Sử dụng UserId để join)
            // Theo script: CustomerID trong Orders tham chiếu đến Users(UserID)
            var customerProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == order.CustomerId);

            // 3. Lấy thông tin Shipper (Sử dụng ProfileID vì Shipper là Staff/Admin)
            // Theo logic của bạn: Staff dùng Username làm ProfileID
            var shipperProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.ProfileId == order.ShipperId);

            // 4. Lấy chi tiết các món ăn/combo
            var details = await _context.OrderDetails
                .Where(d => d.OrderId == orderId)
                .Select(d => new OrderDetailDto
                {
                    OrderDetailId = d.OrderDetailId,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    // Lấy tên món hoặc tên combo
                    FoodName = _context.FoodItems.Where(f => f.FoodId == d.FoodId).Select(f => f.FoodName).FirstOrDefault(),
                    ComboName = _context.Combos.Where(c => c.ComboId == d.ComboId).Select(c => c.ComboName).FirstOrDefault()
                }).ToListAsync();

            return new OrderFullResponseDto
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                StatusName = order.Status?.StatusName,
                DeliveryAddress = order.DeliveryAddress,
                Note = order.Note,
                TotalAmount = order.TotalAmout,

                // Trả về thông tin đã map đúng ID
                CustomerName = customerProfile != null ? (customerProfile.FirstName + " " + customerProfile.LastName) : "N/A",
                ShipperName = shipperProfile != null ? (shipperProfile.FirstName + " " + shipperProfile.LastName) : "Chưa bàn giao",
                Items = details
            };
        }

        
    }
}
