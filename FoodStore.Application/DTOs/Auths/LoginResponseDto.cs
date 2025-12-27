using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.DTOs.Auths
{
    public class LoginResponseDto
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Token { get; set; }
        public string? Role { get; set; }
        public string? RedirectUrl { get; set; }
        public string? ProfileId { get; set; }
    }
}
