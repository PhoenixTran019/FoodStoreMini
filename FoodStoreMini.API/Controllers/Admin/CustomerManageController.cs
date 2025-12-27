using FoodStore.Application.Interface.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FoodStoreMini.API.Controllers.Admin
{
    [Route("api/ManageCustomer")]
    [ApiController]
    public class CustomerManageController : ControllerBase
    {
        private readonly IAdminCustomerService _adminCus;

        public CustomerManageController(IAdminCustomerService adminCus)
        {
            _adminCus = adminCus;
        }

        //==========CONTROLLER TO GET ALL CUSTOMERS===========
        [HttpGet("GetAllCustomers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _adminCus.GetAllCustomersAsync();

            if (customers == null || !customers.Any())
            {
                return NotFound("No customers found.");
            }
            return Ok(customers);
        }

        //==========CONTROLLER TO GET CUSTOMER BY ID===========
        [HttpGet("GetCustomerById/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById (string userId)
        {
            var customer = await _adminCus.GetCustomerByIdAsync(userId);
            if (customer == null)
            {
                return NotFound($"Customer with ID {userId} not found.");
            }
            return Ok(customer);
        }

        //==========CONTROLLER TO LOCK OR UNLOCK CUSTOMER ACCOUNT===========
        [HttpPatch("toggle-status/{userId}")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> ToggleStatus (string userId)
        {
            var result = await _adminCus.ToggleCustomerStatusAsync(userId);
            if (!result)
            {
                return BadRequest("Failed to toggle customer status.");
            }
            return Ok(new
            {
                Message = "Customer status toggled successfully.",
                TargetID = userId
            });
        }
    }
}
