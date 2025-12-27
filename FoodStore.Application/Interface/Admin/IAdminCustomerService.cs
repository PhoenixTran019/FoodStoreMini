using FoodStore.Application.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.Interface.Admin
{
    public interface IAdminCustomerService
    {
        //Take list customer
        Task<List<CustomerAdminDto>> GetAllCustomersAsync();

        //Take detail customer by id
        Task<CustomerAdminDto> GetCustomerByIdAsync (string userId);

        //Lock or unlock customer account
        Task<bool> ToggleCustomerStatusAsync(string targetUserId);

    }
}
