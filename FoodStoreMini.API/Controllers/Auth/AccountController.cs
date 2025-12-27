using FoodStore.Application.DTOs.Auths;
using FoodStore.Application.Interface.Auth;
using Microsoft.AspNetCore.Mvc;

namespace FoodStoreMini.API.Controllers.Auth
{
    [ApiController]
    [Route("api/Account")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var result = await _accountService.LoginAsync(dto);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("Register-customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerDto dto)
        {
            try
            {
                var result = await _accountService.RegisterCustomerAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
