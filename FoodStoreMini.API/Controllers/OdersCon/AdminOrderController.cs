using FoodStore.Application.DTOs.Orders;
using FoodStore.Application.Interface.Orders;
using FoodStore.Domain.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodStoreMini.API.Controllers.OdersCon
{
    [Route("api/AdminOrder")]
    [ApiController]
    public class AdminOrderController : ControllerBase
    {
        private readonly IAdminOrderService _adminOrderService;
        private readonly AppDbContext _context;

        public AdminOrderController(IAdminOrderService adminOrderService, AppDbContext context)
        {
            _adminOrderService = adminOrderService;
            _context = context;
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("admin/pending-orders")]
        public async Task<IActionResult> GetPendingOrders()
        {
            var pendingStatus = await _context.OrderStatuses.FirstOrDefaultAsync(s => s.StatusName == "Pending");

            var orders = await _context.Orders
                .Where(o => o.StatusId == pendingStatus.StatusId)
                .Select(o => new { o.OrderId, o.OrderDate, o.TotalAmout, o.DeliveryAddress })
                .ToListAsync();

            return Ok(orders);
        }

        //============CONTROLLER TO UPDATE ORDER STATUS==========
        [Authorize(Roles = "Admin, Staff")]
        [HttpPut("{orderId}/Update-OrderStatus")]
        public async Task<IActionResult> UpdateStatus(string orderId, [FromBody] UpdateOrderStatusDto request)
        {
            var result = await _adminOrderService.UpdateOrderStatusAsync(orderId, request);

            if (!result)
                return BadRequest("Không tìm thấy đơn hàng hoặc trạng thái không hợp lệ.");

            return Ok(new
            {
                Message = "Cập nhật đơn hàng thành công",
                OrderId = orderId
            });
        }

        // 5.2 ADMIN: Xem lịch sử của 1 khách hàng cụ thể (Lọc theo ProfileID)
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("history/customer/{profileId}")]
        public async Task<IActionResult> GetByCustomer(string profileId)
        {
            var result = await _adminOrderService.GetOrdersByCustomerAsync(profileId);
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

