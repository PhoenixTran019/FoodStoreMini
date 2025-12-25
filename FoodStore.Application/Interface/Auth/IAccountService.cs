using FoodStore.Application.DTOs.Auths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.Interface.Auth
{
    public interface IAccountService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);

        Task<string> RegisterCustomerAsync(RegisterCustomerDto registerDto);
    }
}
