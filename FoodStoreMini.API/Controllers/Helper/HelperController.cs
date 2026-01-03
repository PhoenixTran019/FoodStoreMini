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

        [Authorize(Roles = "Admin")]
        [HttpGet("Food-Categories")]
        public async Task<IActionResult> GetFoodCategories()
        {
            var categoriesList = await _context.Categories
                .Select(c => new
                {
                    c.CategoryId,
                    c.CategoryName
                })
                .ToListAsync();
            if(categoriesList == null || !categoriesList.Any())
            {
                return NotFound("No Categories found");
            }

            return Ok(categoriesList);
        }

        [Authorize(Roles = "Admin, Staff")]
        [HttpGet("AvailableShippers")]
        public async Task<IActionResult> GetAvailableShippers()
        {
            var shippers = await _context.Users
                .Where(u => u.IsActive == true)
                .Join(_context.Roles, u => u.RoleId, r => r.RoleId, (u, r) => new { u, r })
                .Where(x => x.r.RoleName == "Staff") // Lọc những người có quyền Staff/Shipper
                .Join(_context.UserProfiles,
                        combined => combined.u.UserId, // Khớp UserID của bảng Users
                        p => p.UserId,                 // Với UserID của bảng UserProfiles
                        (combined, p) => new
                        {
                            // ĐÂY LÀ GIÁ TRỊ QUAN TRỌNG NHẤT
                            ShipperId = combined.u.UserId,
                            FullName = p.FirstName + " " + p.LastName,
                            PhoneNumber = p.StaffPhone
                        })
                .ToListAsync();

            return Ok(shippers);
        }

    }
}
