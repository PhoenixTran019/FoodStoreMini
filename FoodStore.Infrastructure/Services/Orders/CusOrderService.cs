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
using System.IdentityModel.Tokens.Jwt;

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

        public async Task<string> CreateOrderAsync(CreateOrderRequestDto request, string customerID)
        {
            // 1. Lấy trạng thái mặc định - Kiểm tra kỹ tên status trong DB
            var pendingStatus = await _context.OrderStatuses
                .FirstOrDefaultAsync(s => s.StatusName == "Pending");

            if (pendingStatus == null) throw new Exception("Hệ thống chưa thiết lập trạng thái 'Pending'.");
            

           

            // 3. Truy vấn nhanh thông tin giá và tính khả dụng
            var foodIds = request.Details.Where(d => !string.IsNullOrEmpty(d.FoodID)).Select(d => d.FoodID).ToList();
            var comboIds = request.Details.Where(d => !string.IsNullOrEmpty(d.ComboID)).Select(d => d.ComboID).ToList();

            var dbFoods = await _context.FoodItems.Where(f => foodIds.Contains(f.FoodId)).ToListAsync();
            var dbCombos = await _context.Combos.Where(c => comboIds.Contains(c.ComboId)).ToListAsync();

            var orderId = Uuidv7Generator.NewUuid7().ToString();
            decimal? total = 0; // Đổi thành decimal (không nullable) để tính toán chính xác
            var details = new List<OrderDetail>();

            // 4. Duyệt danh sách chi tiết và kiểm tra logic
            foreach (var item in request.Details)
            {
                decimal? unitPrice = 0;

                if (!string.IsNullOrEmpty(item.FoodID))
                {
                    var food = dbFoods.FirstOrDefault(f => f.FoodId == item.FoodID);
                    // Sửa lỗi CS0019 bằng cách so sánh rõ ràng với true
                    if (food == null || food.IsAvailable != true)
                        throw new Exception($"Món ăn ID {item.FoodID} không tồn tại hoặc ngừng bán.");

                    unitPrice = food.Price; // Đảm bảo Price trong Entity FoodItem không null hoặc dùng ?? 0
                }
                else if (!string.IsNullOrEmpty(item.ComboID))
                {
                    var combo = dbCombos.FirstOrDefault(c => c.ComboId == item.ComboID);
                    if (combo == null || combo.IsAvailable != true)
                        throw new Exception($"Combo ID {item.ComboID} không tồn tại hoặc ngừng bán.");

                    unitPrice = combo.Price;
                }

                details.Add(new OrderDetail
                {
                    OrderDetailId = Uuidv7Generator.NewUuid7().ToString(),
                    OrderId = orderId,
                    FoodId = item.FoodID,
                    ComboId = item.ComboID,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice
                });

                total += (unitPrice * item.Quantity);
            }

            // 5. Khởi tạo Order
            var order = new Order
            {
                OrderId = orderId,
                CustomerId = customerID,
                OrderDate = DateTime.UtcNow,
                DeliveryAddress = request.DeliveryAddress,
                Note = request.Note,
                StatusId = pendingStatus.StatusId,
                TotalAmout = total // Đảm bảo không truyền null vào đây
            };

            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Orders.Add(order);
                _context.OrderDetails.AddRange(details);

                // Ghi log hoạt động
                _context.ActivityLogs.Add(new ActivityLog
                {
                    LogId = Uuidv7Generator.NewUuid7().ToString(),
                    UserId = customerID,
                    Action = "Create New Order",
                    TagetTable = "Orders",
                    TargetId = order.OrderId,
                    TargetName = $"OrderID: {order.OrderId}",
                    TimeStamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await trans.CommitAsync();
                return order.OrderId;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                // Ném lỗi chi tiết để debug dễ hơn thay vì lỗi 500 chung chung
                throw new Exception($"Lỗi khi lưu đơn hàng: {ex.Message}", ex);
            }
        }

        public async Task<List<OrderHistoryDto>> GetActiveOrdersAsync(string customerID)
{
    // Chuyển hết về chữ HOA để so sánh không sai lệch
    var activeStatuses = new List<string> { "PENDING", "CONFIRMED", "PROCESSING", "SHIPPING" };

    return await _context.Orders
        .Where(o => o.CustomerId == customerID)
        .Join(_context.OrderStatuses,
            o => o.StatusId,
            s => s.StatusId,
            (o, s) => new { o, s })
        // Dùng Trim() để bỏ khoảng trắng thừa và ToUpper() để so sánh chuẩn xác
        .Where(x => activeStatuses.Contains(x.s.StatusName.Trim().ToUpper())) 
        .Select(x => new OrderHistoryDto
        {
            OrderID = x.o.OrderId,
            OrderDate = x.o.OrderDate,
            TotalAmount = x.o.TotalAmout ?? 0,
            StatusName = x.s.StatusName.Trim(), // Trả về tên đã xóa khoảng trắng
            DeliveryAddress = x.o.DeliveryAddress
        })
        .OrderByDescending(x => x.OrderDate)
        .ToListAsync();
}


        public async Task<List<OrderHistoryDto>> GetMyOrderHistoryAsync(string customerID)
        {
            // Danh sách các trạng thái kết thúc (History)
            var historyStatuses = new List<string> { "Cancelled", "Failed", "Completed", "Refunded" };

            return await _context.Orders
                .Where(o => o.CustomerId == customerID)
                .Join(_context.OrderStatuses,
                    o => o.StatusId,
                    s => s.StatusId,
                    (o, s) => new { o, s })
                .Where(x => historyStatuses.Contains(x.s.StatusName.Trim().ToUpper())) // Chỉ lấy đơn đã kết thúc
                .Select(x => new OrderHistoryDto
                {
                    OrderID = x.o.OrderId,
                    OrderDate = x.o.OrderDate,
                    TotalAmount = x.o.TotalAmout ?? 0,
                    StatusName = x.s.StatusName.Trim(),
                    DeliveryAddress = x.o.DeliveryAddress,
                    // Hiển thị lý do thất bại (StaffNote) cho các trạng thái không phải Completed
                    Note = _context.OrderTrackings
                        .Where(t => t.OrderId == x.o.OrderId)
                        .OrderByDescending(t => t.UpdateTime)
                        .Select(t => t.StaffNote)
                        .FirstOrDefault()
                })
                .OrderByDescending(x => x.OrderDate)
              .ToListAsync();
        }

        public async Task<OrderFullResponseDto> GetOrderDetailAsync(string orderId, string? customerID = null, bool isAdminOrStaff = false)
        {
            // 1. Tạo query cơ bản
            var query = _context.Orders
                .Include(o => o.Status)
                .AsQueryable();

            // 2. Kiểm tra quyền: Nếu không phải Admin/Staff thì bắt buộc phải khớp CustomerId
            if (!isAdminOrStaff)
            {
                if (string.IsNullOrEmpty(customerID)) return null;
                query = query.Where(o => o.OrderId == orderId && o.CustomerId == customerID);
            }
            else
            {
                query = query.Where(o => o.OrderId == orderId.Trim());
            }

            var order = await query.FirstOrDefaultAsync();
            if (order == null) return null;

            // 3. Lấy Profile khách hàng và Shipper
            var customerProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == order.CustomerId);

            // ShipperId thường là ProfileId của nhân viên giao hàng
            var shipperProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == order.ShipperId);

            // 4. Lấy chi tiết các món ăn (Dùng Join để tối ưu hiệu năng thay vì Select lồng)
            var details = await _context.OrderDetails
                .Where(d => d.OrderId == order.OrderId)
                .Select(d => new OrderDetailDto
                {
                    OrderDetailId = d.OrderDetailId,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice ?? 0,
                    // Lấy tên Food hoặc Combo
                    FoodName = _context.FoodItems.Where(f => f.FoodId == d.FoodId).Select(f => f.FoodName).FirstOrDefault(),
                    ComboName = _context.Combos.Where(c => c.ComboId == d.ComboId).Select(c => c.ComboName).FirstOrDefault()
                }).ToListAsync();

            // 5. Lấy hành trình đơn hàng (Trackings)
            var trackings = await _context.OrderTrackings
                .Where(t => t.OrderId == orderId)
                .Join(_context.OrderStatuses,
                    t => t.StatusId,
                    s => s.StatusId,
                    (t, s) => new OrderTrackingDto
                    {
                        StatusName = s.StatusName.Trim(),
                        UpdateTime = t.UpdateTime ?? DateTime.UtcNow,
                        Reason = t.StaffNote // Lý do cập nhật (StaffNote)
                    })
                .OrderByDescending(x => x.UpdateTime)
                .ToListAsync();

            // 6. Trả về kết quả
            return new OrderFullResponseDto
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                StatusName = order.Status?.StatusName.Trim(),
                DeliveryAddress = order.DeliveryAddress,
                Note = order.Note,
                TotalAmount = order.TotalAmout ?? 0,
                CustomerName = customerProfile != null ? $"{customerProfile.FirstName} {customerProfile.LastName}" : "Khách vãng lai",
                ShipperName = shipperProfile != null ? $"{shipperProfile.FirstName} {shipperProfile.LastName}" : "Chưa có shipper",
                Items = details,
                Trackings = trackings
            };
        }


    }
}
