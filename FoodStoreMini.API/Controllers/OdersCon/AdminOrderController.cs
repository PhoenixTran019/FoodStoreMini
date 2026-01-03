using FoodStore.Application.DTOs.Orders;
using FoodStore.Application.Interface.Orders;
using FoodStore.Domain.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FoodStoreMini.API.Controllers.OdersCon
{
    [Route("api/AdminOrder")]
    [ApiController]
    public class AdminOrderController : ControllerBase
    {
        private readonly IAdminOrderService _adminOrderService;
        private readonly AppDbContext _context;
        private readonly ICusOrderService _cusOrderService;

        public AdminOrderController(IAdminOrderService adminOrderService, AppDbContext context, ICusOrderService cusOrderService)
        {
            _adminOrderService = adminOrderService;
            _context = context;
            _cusOrderService = cusOrderService;
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("admin/active-orders")]
        public async Task<IActionResult> GetAdminActiveOrders()
        {
            // Định nghĩa các trạng thái "Active"
            var activeStatuses = new List<string> { "Pending", "Confirmed", "Processing", "Shipping" };

            // Gọi service và không truyền CustomerId để lấy toàn bộ đơn hàng
            var orders = await _adminOrderService.GetOrdersByStatusAsync(activeStatuses);

            return Ok(orders);
        }

        //============CONTROLLER TO UPDATE ORDER STATUS==========
        [Authorize(Roles = "Admin, Staff")]
        [HttpPut("{orderId}/Update-OrderStatus")]
        public async Task<IActionResult> UpdateStatus(string orderId, [FromBody] UpdateOrderStatusDto request)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
             ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // In ra màn hình console của server
            Console.WriteLine($"--- DEBUG: UserId nhận được từ Token là: {userId} ---");

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("--- WARNING: Không tìm thấy Sub Claim trong Token! ---");
            }

            var result = await _adminOrderService.UpdateOrderStatusAsync(orderId, request, userId);

            if (!result)
                return BadRequest("Không tìm thấy đơn hàng hoặc trạng thái không hợp lệ.");

            return Ok(new
            {
                Message = "Cập nhật đơn hàng thành công",
                OrderId = orderId
            });
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("detail/{orderId}")]
        public async Task<IActionResult> GetAdminDetail(string orderId)
        {
            // Truyền isAdminOrStaff = true để bỏ qua bước check CustomerId
            var result = await _cusOrderService.GetOrderDetailAsync(orderId, null, isAdminOrStaff: true);

            if (result == null) return NotFound("Đơn hàng không tồn tại.");
            return Ok(result);
        }

        // 5.2 ADMIN: Xem lịch sử của 1 khách hàng cụ thể (Lọc theo ProfileID)
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("history/customer/{userId}")]
        public async Task<IActionResult> GetByCustomer(string userId)
        {
            var result = await _adminOrderService.GetOrdersByCustomerAsync(userId);
            return Ok(result);
        }

        // 5.3 ADMIN: Xem lịch sử theo khoảng thời gian
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("history/timeline")]
        public async Task<IActionResult> GetByTimeRange([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            // Fix ngày kết thúc để lấy trọn vẹn dữ liệu đến cuối ngày
            var finalEnd = end.Date.AddDays(1).AddTicks(-1);
            var result = await _adminOrderService.GetOrdersByTimeRangeAsync(start, finalEnd);
            return Ok(result);
        }

    }
}

