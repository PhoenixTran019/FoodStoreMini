using FoodStore.Application.Common.Helper;
using FoodStore.Application.DTOs.Auths;
using FoodStore.Application.Interface.Auth;
using FoodStore.Domain.Data;
using FoodStore.Domain.Entities;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Infrastructure.Services.Auths
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;

        public AccountService(AppDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        //==========SERVICE TO LOGIN==========
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            // 1. Chuẩn hóa đầu vào
            var input = dto.Username?.Trim();
            if (string.IsNullOrEmpty(input)) throw new Exception("Tài khoản không được để trống");

            // 2. Tìm User (Bao gồm cả Role để lấy RoleName)
            // Hệ thống cho phép đăng nhập bằng Username (Staff) hoặc SĐT (Customer)
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == input || u.PhoneNumber == input);

            if (user == null || user.IsActive == false)
                throw new Exception("Tài khoản không tồn tại hoặc đã bị khóa");

            // 3. Kiểm tra Password
            var hashedInput = _jwtService.HashPassword(dto.Password);
            if (user.PasswordHash != hashedInput)
                throw new Exception("Mật khẩu không chính xác");

            // 4. XÁC ĐỊNH PROFILE ID (Điểm mấu chốt bạn yêu cầu)
            // Nếu là Staff/Admin: ProfileID = Username
            // Nếu là Customer: ProfileID = Tìm trong bảng UserProfiles dựa trên UserId
            string profileId = "";
            if (user.Role.RoleName == "Admin" || user.Role.RoleName == "Staff" || user.Role.RoleName == "Shipper")
            {
                profileId = user.Username; // Staff dùng Username làm ID
            }
            else
            {
                var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.UserId);
                profileId = profile?.ProfileId ?? "NoProfile"; // Customer dùng UUID ProfileId
            }

            // 5. Tạo Token (Truyền 3 tham số theo JwtService của bạn)
            var token = _jwtService.CreateToken(user, user.Role.RoleName, profileId);

            // 6. Điều hướng URL dựa trên Role
            var redirectUrl = user.Role.RoleName switch
            {
                "Admin" => "/admin/dashboard",
                "Staff" => "/staff/orders",
                "Customer" => "/customer/dashboard",
                _ => "/home"
            };

            return new AuthResponseDto
            {
                Username = user.Username ?? user.PhoneNumber,
                Token = token,
                Role = user.Role.RoleName,
                RedirectUrl = redirectUrl,
                ProfileId = profileId
            };
        }

        //==========SERVICE TO CUSTOMER REGISTER===========
        public async Task<string> RegisterCustomerAsync(RegisterCustomerDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.PhoneNumber == dto.PhoneNumber))
                throw new Exception("Phone Number is exist");

            var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = Uuidv7Generator.NewUuid7().ToString();
                var profileId = Uuidv7Generator.NewUuid7().ToString();

                var user = new User
                {
                    UserId = userId,
                    PhoneNumber = dto.PhoneNumber,
                    PasswordHash = _jwtService.HashPassword(dto.Password),
                    RoleId = customerRole.RoleId,
                    IsActive = true
                };

                var profile = new UserProfile
                {
                    ProfileId = profileId,
                    UserId = userId,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email
                };

                var log = new ActivityLog
                {
                    LogId = Uuidv7Generator.NewUuid7().ToString(),
                    UserId = userId,
                    Action = "Create Account",
                    TargetId = profileId,
                    TargetName = dto.FirstName + " " + dto.LastName
                };

                _context.Users.Add(user);
                _context.UserProfiles.Add(profile);
                _context.ActivityLogs.Add(log);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return "Resigter Success";
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
