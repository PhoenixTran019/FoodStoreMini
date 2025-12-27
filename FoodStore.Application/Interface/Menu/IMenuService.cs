using FoodStore.Application.DTOs.Foods;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.Interface.Menu
{
    public interface IMenuService
    {
        //==========SERVICE CRUD FOR FOODS==========
        Task<IEnumerable<FoodItemResponseDto>> GetAllFoodsAsync();

        Task<string> CreateFoodAsync(FoodItemUpsertDto dto, IFormFile image);

        Task<bool> UpdateFoodAsync(string id, FoodItemUpsertDto dto, IFormFile image);

        Task<bool> DeleteFoodAsync(string FoodId);

        //==========SERVICE CRUD FOR COMBO==========
        Task<IEnumerable<ComboResponseDto>> GetAllCombosAsync();

        Task<string> CreateComboAsync(ComboUpsertDto dto, IFormFile image);

        Task<bool> UpdateComboAsync(string id, ComboUpsertDto dto, IFormFile? image);

        Task<bool> DeleteComboAsync(string comboId);
    }
}
