using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.DTOs.Auths
{
    public class RegisterCustomerDto
    {
        public string? PhoneNumber { get; set; }

        public string? Password { get; set; } 

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }
    }
}
