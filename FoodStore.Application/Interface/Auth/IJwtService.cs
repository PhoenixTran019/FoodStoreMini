using FoodStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.Interface.Auth
{
    public interface IJwtService
    {
        string CreateToken(User user, string roleName, string profileId);
        
        string HashPassword(string password);
    }
}
