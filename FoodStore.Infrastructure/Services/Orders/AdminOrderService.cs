using FoodStore.Application.DTOs.Orders;
using FoodStore.Application.Interface.Orders;
using FoodStore.Domain.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoodStore.Domain.Entities;
using FoodStore.Application.Common.Helper;

namespace FoodStore.Infrastructure.Services.Orders
{
    public class AdminOrderService : IAdminOrderService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContext;

        public AdminOrderService (AppDbContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;
        }

        //============SERVICE TO UPDATE ORDER STATUS==========
        public async Task<bool> UpdateOrderStatusAsync(string orderId, UpdateOrderStatusDto request)
        {
            // 1. Tìm đơn hàng
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            // 2. Tìm thông tin Trạng thái mới để kiểm tra tên (Cancelled, Failed, Refunded)
            var newStatus = await _context.OrderStatuses
                .FirstOrDefaultAsync(s => s.StatusId == request.NewStatusID);
            if (newStatus == null) return false;

            // Danh sách các trạng thái yêu cầu giải thích
            var negativeStatuses = new List<string> { "Cancelled", "Failed", "Refunded" };

            // (Tùy chọn) Bắt lỗi nếu trạng thái xấu mà không có mô tả
            if (negativeStatuses.Contains(newStatus.StatusName) && string.IsNullOrWhiteSpace(request.StaffNote))
            {
                // Bạn có thể throw exception hoặc return false tùy thiết kế
                throw new Exception($"Trạng thái {newStatus.StatusName} bắt buộc phải có lý do.");
            }

            var actonId = _httpContext.HttpContext?.User?.FindFirstValue("ProfileId");

            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                // 3. Cập nhật trạng thái và Shipper trong bảng Orders
                order.StatusId = request.NewStatusID;
                if (!string.IsNullOrEmpty(request.ShipperID))
                {
                    order.ShipperId = request.ShipperID;
                }

                // 4. Ghi lại hành trình (OrderTracking) kèm theo mô tả của Staff
                // Lưu ý: Đảm bảo bảng OrderTracking của bạn đã có cột StaffDescription (hoặc tương đương)
                var tracking = new OrderTracking
                {
                    TrackingId = Uuidv7Generator.NewUuid7().ToString(),
                    OrderId = order.OrderId,
                    StatusId = request.NewStatusID,
                    UpdateTime = DateTime.UtcNow,
                    // Ghi nhận lý do vào đây để Khách hàng xem được trong Timeline
                    StaffNote = request.StaffNote
                };
                _context.OrderTrackings.Add(tracking);

                // 5. Ghi Activity Log (Nhật ký hoạt động của Admin)
                _context.ActivityLogs.Add(new ActivityLog
                {
                    LogId = Uuidv7Generator.NewUuid7().ToString(),
                    UserId = actonId,
                    Action = $"Update Order Status to {newStatus.StatusName}. Reason: {request.StaffNote}",
                    TagetTable = "Orders",
                    TargetId = order.OrderId,
                    TimeStamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await trans.CommitAsync();
                return true;
            }
            catch
            {
                await trans.RollbackAsync();
                throw;
            }
        }

        // 5.2 ADMIN: Xem lịch sử của 1 khách hàng cụ thể dựa trên ProfileID
        public async Task<List<OrderHistoryDto>> GetOrdersByCustomerAsync(string customerProfileId)
        {
            // Tìm UserId từ ProfileID (vì trong bảng Orders lưu CustomerID là UserId)
            var user = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.ProfileId == customerProfileId);

            if (user == null) return new List<OrderHistoryDto>();

            return await _context.Orders
                .Where(o => o.CustomerId == user.UserId)
                .Join(_context.OrderStatuses, o => o.StatusId, s => s.StatusId, (o, s) => new OrderHistoryDto
                {
                    OrderID = o.OrderId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmout,
                    StatusName = s.StatusName,
                    CustomerFullName = user.FirstName + " " + user.LastName
                })
                .ToListAsync();
        }

        // 5.3 ADMIN: Xem lịch sử theo khoảng thời gian
        public async Task<List<OrderHistoryDto>> GetOrdersByTimeRangeAsync(DateTime start, DateTime end)
        {
            return await _context.Orders
                .Where(o => o.OrderDate >= start && o.OrderDate <= end)
                .Join(_context.OrderStatuses, o => o.StatusId, s => s.StatusId, (o, s) => new { o, s })
                .Join(_context.UserProfiles, combined => combined.o.CustomerId, p => p.UserId, (combined, p) => new OrderHistoryDto
                {
                    OrderID = combined.o.OrderId,
                    OrderDate = combined.o.OrderDate,
                    TotalAmount = combined.o.TotalAmout,
                    StatusName = combined.s.StatusName,
                    CustomerFullName = p.FirstName + " " + p.LastName
                })
                .OrderByDescending(x => x.OrderDate)
                .ToListAsync();
        }
    }
}
