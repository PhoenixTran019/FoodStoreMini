using FoodStore.Application.DTOs.Order;
using FoodStore.Application.Interface.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FoodStoreMini.API.Controllers.OdersCon
{
    [Route("api/CustomerOrder")]
    [ApiController]
    public class CustomerOrderController : ControllerBase
    {
        private readonly ICusOrderService _orderService;

        public CustomerOrderController(ICusOrderService orderService)
        {
            _orderService = orderService;
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

    }
}
