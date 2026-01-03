using FoodStore.Application.DTOs.Admin;
using FoodStore.Domain.Data;
using Microsoft.EntityFrameworkCore;
using FoodStore.Application.Interface.Admin;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.Http;
using FoodStore.Domain.Entities;
using FoodStore.Application.Common.Helper;

namespace FoodStore.Infrastructure.Services.Admin
{
    public class AdminCustomerService : IAdminCustomerService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _http;

        public AdminCustomerService (AppDbContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
        }

        //==========Service to get all customers===========
        public async Task<List<CustomerAdminDto>> GetAllCustomersAsync()
        {
            return await _context.Users
                // 1. Join với bảng Roles để biết ai là Customer
                .Join(_context.Roles,
                    u => u.RoleId,
                    r => r.RoleId,
                    (u, r) => new { u, r })
                // 2. Lọc chỉ lấy những người có RoleName là Customer
                .Where(x => x.r.RoleName == "Customer")
                // 3. Join với bảng UserProfiles để lấy thông tin chi tiết
                .Join(_context.UserProfiles,
                    combined => combined.u.UserId,
                    p => p.UserId,
                    (combined, p) => new CustomerAdminDto
                    {
                        userID = combined.u.UserId,
                        UserName = combined.u.Username,
                        FullName = p.FirstName + " " + p.LastName,
                        Email = p.Email,
                        PhoneNumber = combined.u.PhoneNumber,
                        Address = p.Address,
                        IsActive = combined.u.IsActive,
                        CreateAt = combined.u.CreateAt ?? DateTime.Now
                    })
                .ToListAsync();
        }

        //==========SERVICE TO GET CUSTOMER BY ID===========
        public async Task<CustomerAdminDto> GetCustomerByIdAsync(string userId)
        {
            var customer = await _context.Users
                .Where(u => u.UserId == userId)
                .Join(_context.UserProfiles,
                    u => u.UserId,
                    p => p.UserId,
                    (u, p) => new CustomerAdminDto
                    {
                        userID = u.UserId,
                        UserName = u.Username,
                        FullName = p.FirstName + " " + p.LastName,
                        Email = p.Email,
                        PhoneNumber = u.PhoneNumber,
                        Address = p.Address,
                        IsActive = u.IsActive ?? true,
                        CreateAt = u.CreateAt ?? DateTime.Now
                    })
                .FirstOrDefaultAsync();

            return customer;
        }

        //==========SERVICE TO ACTIVATE/DEACTIVATE CUSTOMER===========
        public async Task<bool> ToggleCustomerStatusAsync(string targetUserId)
        {
            //Find the user has been impacted
            var targetUser = await _context.Users
                .FindAsync(targetUserId);

            if (targetUser == null) return false;

            //Reverse the IsActive status
            bool oldStatus = targetUser.IsActive ?? true;
            targetUser.IsActive = !oldStatus;

            string actionText = targetUser.IsActive == true ? "activated" : "deactivated";

            var adminProfileId = _http.HttpContext?.User?.FindFirstValue("ProfileId")
                          ?? "Unknown_Admin";

            var log = new ActivityLog
            {
                LogId = Uuidv7Generator.NewUuid7().ToString(),
                UserId = adminProfileId,
                Action = $"Admin {actionText} customer account",
                TagetTable = "Users",
                TargetId = targetUser.UserId,
                TargetName = targetUser.PhoneNumber,
                TimeStamp = DateTime.UtcNow
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
