using FoodStore.Domain.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FoodStoreMini.API.Controllers.Helpers
{
    [Route("api/Helper")]
    [ApiController]
    public class HelperController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HelperController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize (Roles = "Admin, Staff")]
        [HttpGet("OrderStatus")]
        public async Task<IActionResult> GetOrderStatus()
        {
            //Take List ID and Name of Order Status
            var statusList = await _context.OrderStatuses
                .Select(s => new
                {
                    s.StatusId,
                    s.StatusName
                })
                .ToListAsync();

            if(statusList == null || !statusList.Any())
            {
                return NotFound("No order statuses found.");
            }

            return Ok(statusList);
        }

        [Authorize(Roles = "Admin, Staff")]
        [HttpGet("AvailableShippers")]
        public async Task<IActionResult> GetAvailableShippers()
        {
            var shippers = await _context.Users
                .Where(u => u.IsActive == true)
                .Join(_context.Roles, u => u.RoleId, r => r.RoleId, (u, r) => new { u, r })
                .Where(x => x.r.RoleName == "Staff")
                .Join(_context.UserProfiles,
                        combined => combined.u.Username,
                        p => p.ProfileId,
                        (combined, p) => new
                        {
                            ShipperProfileId = combined.u.Username,
                            FullName = p.FirstName + " " + p.LastName,
                            PhoneNumber = p.StaffPhone
                        })
                .ToArrayAsync();
            return Ok(shippers);
        }
    }
}
