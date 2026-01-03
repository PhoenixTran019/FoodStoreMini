using FoodStore.Application.Auth;
using FoodStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using FoodStore.Application.Interface.Auth;

namespace FoodStore.Infrastructure.Services.Auths
{
    public class JwtService : IJwtService
    {
        private readonly JwtOptions _opts;

        public JwtService(IOptions<JwtOptions> opts)
        {
            _opts = opts.Value;
        }

        public string CreateToken(User user, string roleName, string profileId)
        {
            var claims = new List<Claim>
            {
                //The unique identifier of a User in the Users table (Guid or Int)
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

                //Display name
                new Claim(ClaimTypes.Name, user.Username ?? user.PhoneNumber ?? "Unknown"),

                //Authorization role
                new Claim(ClaimTypes.Role, roleName),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),

                // KEY POINT: Save the ProfileId to the token.
                // Later, if you want to know which Profile the user is working on, just decrypt the token.
                new Claim("ProfileId", profileId)
            };

            //Signing
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //Packet
            var token = new JwtSecurityToken(
                issuer: _opts.Issuer,
                audience: _opts.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_opts.ExpiresInMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string HashPassword(string password)
        {
            using(SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();

            }
        }
    }
}
