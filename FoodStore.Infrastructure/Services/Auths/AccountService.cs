using FoodStore.Application.Common.Helper;
using FoodStore.Application.DTOs.Auths;
using FoodStore.Application.Interface.Auth;
using FoodStore.Domain.Data;
using FoodStore.Domain.Entities;
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
        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            //Find user by identifier (username or phone)
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == loginDto.Identifier || u.PhoneNumber == loginDto.Identifier);

            if(user == null || user.PasswordHash != _jwtService.HashPassword(loginDto.Password))
                throw new Exception("Username or password is incorrect.");

            string profileId = "";

            //Specific ProjectID tracing logic
            if (user.Role.RoleName == "Admin" || user.Role.RoleName == "Staff")
            {
                profileId = user.Username;
            }
            else if(user.Role.RoleName == "Customer")
            {
                var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.UserId);
                profileId = profile?.ProfileId ?? "NOT_FOUND";
            }

            //Create Token
            var token = _jwtService.CreateToken(user, user.Role.RoleName, profileId);

            return new AuthResponseDto
            {
                Token = token,
                Role = user.Role.RoleName,
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
