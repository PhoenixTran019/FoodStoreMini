using FoodStore.Application.DTOs.Foods;
using FoodStore.Application.Interface.Menu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FoodStoreMini.API.Controllers.Menu
{
    [Route("api/Menu")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        //==========CONTROLLER TO GET ALL FOODS==========
        [HttpGet("foods")]
        [Authorize]
        public async Task<IActionResult> GetAllFoods()
        {
            var foods = await _menuService.GetAllFoodsAsync();
            return Ok(foods);
        }

        //==========CONTROLLER TO CREATE A FOOD ITEM==========
        [HttpPost("create-food")]
        [Consumes("mutipart/form-data")]
        [Authorize (Roles = "Admin")]
        public async Task<IActionResult> CreateFood([FromForm] FoodItemUpsertDto dto, IFormFile? image)
        {
            try
            {
                var foodId = await _menuService.CreateFoodAsync(dto, image);
                return Ok(new { Message = "Create Food Success", FoodID = foodId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("foods/{foodId}/Update-Food")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateFood (string foodId, [FromForm] FoodItemUpsertDto dto, IFormFile? image)
        {
            var success = await _menuService.UpdateFoodAsync(foodId, dto, image);
            if(!success) return NotFound (new { Message = "Food Not Found" });

            return Ok (new { Message = "Update Food Success" });
        }

        //==========CONTROLLER TO DELETE A FOOD ITEM==========
        [HttpDelete("foods/{foodId}/delete-food")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFood (string foodId)
        {
            var success = await _menuService.DeleteFoodAsync(foodId);
            if(!success) return NotFound (new { Message = "Food Not Found" });

            return Ok (new { Message = "Delete Food Success" });
        }

        //==========CONTROLLER TO GET ALL COMBOS==========
        [HttpGet("combos")]
        [Authorize]
        public async Task<IActionResult> GetCombos()
            => Ok (await _menuService.GetAllCombosAsync());

        //==========CONTROLLER TO CREATE A COMBO==========
        [HttpPost("create-combo")]
        [Authorize (Roles = "Admin")]
        public async Task<IActionResult> CreateCombo([FromForm] ComboUpsertDto dto, IFormFile? image)
        {
            var comboId = await _menuService.CreateComboAsync(dto, image);
            return Ok (new { Message = "Create Combo Success", ComboID = comboId } );
        }

        //==========CONTROLLER TO UPDATE A COMBO==========
        [HttpPut("Combo/{comboId}/Update-Combo")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCombo(string comboId, [FromForm] ComboUpsertDto dto, IFormFile? image)
        {
            var success = await _menuService.UpdateComboAsync(comboId, dto, image);
            return success ? Ok (new { Message = "Update Combo Success" }) : NotFound (new { Message = "Combo Not Found" });
        }

        //==========CONTROLLER TO DELETE A COMBO==========
        [HttpDelete("Combo/{comboId}/delete-combo")]
        [Authorize(Roles= "Admin")]
        public async Task<IActionResult>DeleteCombo (string comboId)
        {
            var success = await _menuService.DeleteComboAsync(comboId);
            return success ? Ok (new { Message = "Delete Combo Success" }) : NotFound (new { Message = "Combo Not Found" });
        }

    }
}
