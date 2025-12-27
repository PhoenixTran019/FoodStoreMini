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
    [Route("api/CustomerOrder")]
    [ApiController]
    public class CustomerOrderController : ControllerBase
    {
        private readonly ICusOrderService _orderService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly AppDbContext _context;

        public CustomerOrderController(ICusOrderService orderService, AppDbContext context)
        {
            _orderService = orderService;
            _context = context;
        }

        //==========CONTROLLER TO CREATE ORDER===========(Ckeck out)
        [HttpPost("CreateOrder")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateOrder([FromBody]CreateOrderRequestDto request)
        {
            //Check avilable form database in put
            if (request == null || request.Details == null || !request.Details.Any())
            {
                return BadRequest("Order information or shopping cart details must not be left blank.");
            }
            try
            {
                //Call service to process create order
                var orederId = await _orderService.CreateOrderAsync(request);

                return Ok(new
                {
                    Success = true,
                    Message = "Order created successfully.",
                    OrderId = orederId
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating order: {ex.Message}");
            }
        }

        //==========Customer Check Order Status==========
        [Authorize]
        [HttpGet("CheckOrderStatus/{orderId}")]
        public async Task<IActionResult> GetOrderStatus (string orderId)
        {
            // 1. Kiểm tra đơn hàng có tồn tại và thuộc về User này không
            var currentUserId = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return NotFound("Đơn hàng không tồn tại.");

            // Nếu là Customer thì chỉ được xem đơn của mình
            if (User.IsInRole("Customer") && order.CustomerId != currentUserId)
            {
                return Forbid();
            }

            // 2. Lấy hành trình đơn hàng (Giữ nguyên logic JOIN của bạn)
            var timeLine = await _context.OrderTrackings
                .Where(t => t.OrderId == orderId)
                .Join(_context.OrderStatuses,
                    t => t.StatusId,
                    s => s.StatusId,
                    (t, s) => new
                    {
                        StatusName = s.StatusName,
                        UpdateTime = t.UpdateTime ?? DateTime.UtcNow,
                        // Hiển thị lý do cho khách hàng thấy
                        Reason = t.StaffNote
                    })
                .OrderByDescending(x => x.UpdateTime)
                .ToListAsync();
            return Ok(timeLine);
        }

        [Authorize]
        [HttpGet("Order-detail/{orderId}")]
        public async Task<IActionResult> GetOrderDetail(string orderId)
        {
            var order = await _orderService.GetOrderDetailAsync(orderId);
            if (order == null) return NotFound("Đơn hàng không tồn tại.");

            // Kiểm tra quyền (Bảo mật)
            var currentRole = User.FindFirstValue(ClaimTypes.Role);
            var currentSub = User.FindFirstValue(JwtRegisteredClaimNames.Sub); // UserId đối với KH

            if (currentRole == "Customer" && order.OrderId != null)
            {
                // Kiểm tra xem đơn này có phải của khách này không (so sánh UserId)
                var dbOrder = await _context.Orders.FindAsync(orderId);
                if (dbOrder.CustomerId != currentSub) return Forbid();
            }

            return Ok(order);
        }

        

    }
}
