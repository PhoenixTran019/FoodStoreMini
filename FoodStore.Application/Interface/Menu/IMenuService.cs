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

        Task<FoodItemResponseDto> GetFoodItemDetailAsync(string foodId);

        Task<string> CreateFoodAsync(FoodItemUpsertDto dto, IFormFile image, string userID);

        Task<bool> UpdateFoodAsync(string id, FoodItemUpsertDto dto, IFormFile image, string userID);

        Task<bool> DeleteFoodAsync(string FoodId, string userID);

        //==========SERVICE CRUD FOR COMBO==========
        Task<IEnumerable<ComboResponseDto>> GetAllCombosAsync();

        Task<ComboResponseDto> GetComboDetailAsync(string comboId);

        Task<bool> AddFoodToComboAsync(string comboId, ComboDetailRequestDto itemDto);

        Task<bool> RemoveFoodFromComboAsync(string comboId, string foodId);

        Task<string> CreateComboAsync(ComboUpsertDto dto, IFormFile image, string userID);

        Task<ComboResponseDto> UpdateComboAsync(string comboId, ComboUpsertDto dto, IFormFile? image, string userID);

        Task<bool> DeleteComboAsync(string comboId, string userID);
    }
}
