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
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            //Check avilable form database in put
            if (request == null || request.Details == null || !request.Details.Any())
            {
                return BadRequest("Order information or shopping cart details must not be left blank.");
            }
            try
            {
                //Call service to process create order
                var orederId = await _orderService.CreateOrderAsync(request, userId);

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

        //==========CONTROLLER SUPPOST TO CUSTOMER TAKE ACTIVE ORDER==========
        [Authorize]
        [HttpGet("active-orders")]
        public async Task<IActionResult> GetActiveOrders()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
             ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // In ra màn hình console của server
            Console.WriteLine($"--- DEBUG: UserId nhận được từ Token là: {userId} ---");

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("--- WARNING: Không tìm thấy Sub Claim trong Token! ---");
            }

            var orders = await _orderService.GetActiveOrdersAsync(userId);
            return Ok(orders);
        }



        //==========Customer Check Order Status==========
        [Authorize]
        [HttpGet("CheckOrderStatus/{orderId}")]
        public async Task<IActionResult> GetOrderStatus (string orderId)
        {
            // 1. Kiểm tra đơn hàng có tồn tại và thuộc về User này không
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return NotFound("Đơn hàng không tồn tại.");

            // Nếu là Customer thì chỉ được xem đơn của mình
            if (User.IsInRole("Customer") && order.CustomerId != userId)
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
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrderHistory()
        {
            // 1. Lấy UserId (sub) từ JWT Token đã xác thực
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
             ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // In ra màn hình console của server
            Console.WriteLine($"--- DEBUG: UserId nhận được từ Token là: {userId} ---");

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("--- WARNING: Không tìm thấy Sub Claim trong Token! ---");
            }

            if (string.IsNullOrEmpty(userId)) return Unauthorized("Không xác định được người dùng.");

            // 2. Gọi Service với tham số ID vừa lấy
            var orders = await _orderService.GetMyOrderHistoryAsync(userId);

            return Ok(orders);
        }


        [Authorize(Roles = "Customer")]
        [HttpGet("Order-detail/{orderId}")]
        public async Task<IActionResult> GetDetail(string orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var result = await _orderService.GetOrderDetailAsync(orderId, userId, isAdminOrStaff: false);

            if (result == null) return NotFound("Không tìm thấy đơn hàng.");
            return Ok(result);
        }
    }
}
