using FoodStore.Application.DTOs.Foods;
using FoodStore.Application.Interface.Menu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

        [HttpGet("foods/{foodId}")]
        [Authorize]
        public async Task<ActionResult<FoodItemResponseDto>> GetFoodDetail(string foodId)
        {
            var food = await _menuService.GetFoodItemDetailAsync(foodId);
            if (food == null) return NotFound("Không tìm thấy món ăn.");
            return Ok(food);
        }

        //==========CONTROLLER TO CREATE A FOOD ITEM==========
        [HttpPost("create-food")]
        [Consumes("multipart/form-data")]
        [Authorize (Roles = "Admin")]
        public async Task<IActionResult> CreateFood([FromForm] FoodItemUpsertDto dto, IFormFile? image)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // In ra màn hình console của server
            Console.WriteLine($"--- DEBUG: UserId nhận được từ Token là: {userId} ---");

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("--- WARNING: Không tìm thấy Sub Claim trong Token! ---");
            }

            try
            {
                var foodId = await _menuService.CreateFoodAsync(dto, image, userId);
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
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // In ra màn hình console của server
            Console.WriteLine($"--- DEBUG: UserId nhận được từ Token là: {userId} ---");

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("--- WARNING: Không tìm thấy Sub Claim trong Token! ---");
            }
            var success = await _menuService.UpdateFoodAsync(foodId, dto, image, userId);
            if(!success) return NotFound (new { Message = "Food Not Found" });

            return Ok (new { Message = "Update Food Success" });
        }

        //==========CONTROLLER TO DELETE A FOOD ITEM==========
        [HttpDelete("foods/{foodId}/delete-food")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFood (string foodId)
        {

            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // In ra màn hình console của server
            Console.WriteLine($"--- DEBUG: UserId nhận được từ Token là: {userId} ---");

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("--- WARNING: Không tìm thấy Sub Claim trong Token! ---");
            }

            var success = await _menuService.DeleteFoodAsync(foodId, userId);
            if(!success) return NotFound (new { Message = "Food Not Found" });

            return Ok (new { Message = "Delete Food Success" });
        }

        //==========CONTROLLER TO GET ALL COMBOS==========
        [HttpGet("combos")]
        [Authorize]
        public async Task<IActionResult> GetCombos()
            => Ok (await _menuService.GetAllCombosAsync());

        [HttpGet("combos/{comboId}")]
        [Authorize]
        public async Task<ActionResult<ComboResponseDto>> GetComboDetail(string comboId)
        {
            var combo = await _menuService.GetComboDetailAsync(comboId);
            if (combo == null) return NotFound("Không tìm thấy combo.");
            return Ok(combo);
        }

        //==========CONTROLLER TO CREATE A COMBO==========
        [HttpPost("create-combo")]
        [Authorize (Roles = "Admin")]
        public async Task<IActionResult> CreateCombo([FromForm] ComboUpsertDto dto, IFormFile? image)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // In ra màn hình console của server
            Console.WriteLine($"--- DEBUG: UserId nhận được từ Token là: {userId} ---");

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("--- WARNING: Không tìm thấy Sub Claim trong Token! ---");
            }

            var comboId = await _menuService.CreateComboAsync(dto, image, userId);
            return Ok (new { Message = "Create Combo Success", ComboID = comboId } );
        }

        // Thêm 1 món vào combo hiện tại
        [HttpPost("combos/{comboId}/items")]
        public async Task<IActionResult> AddItemToCombo(string comboId, [FromBody] ComboDetailRequestDto dto)
        {
            var result = await _menuService.AddFoodToComboAsync(comboId, dto);
            if (!result) return BadRequest("Không thể thêm món vào combo.");
            return Ok("Đã thêm món thành công.");
        }

        // Xóa hẳn 1 món khỏi combo
        [HttpDelete("combos/{comboId}/items/{foodId}")]
        public async Task<IActionResult> RemoveItemFromCombo(string comboId, string foodId)
        {
            var result = await _menuService.RemoveFoodFromComboAsync(comboId, foodId);
            if (!result) return NotFound("Món ăn không tồn tại trong combo này.");
            return NoContent();
        }



        //==========CONTROLLER TO UPDATE A COMBO==========
        [HttpPut("Combo/{comboId}/Update-Combo")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCombo(string comboId, [FromForm] ComboUpsertDto dto, IFormFile? image)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // In ra màn hình console của server
            Console.WriteLine($"--- DEBUG: UserId nhận được từ Token là: {userId} ---");

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("--- WARNING: Không tìm thấy Sub Claim trong Token! ---");
            }

            var updatedCombo = await _menuService.UpdateComboAsync(comboId, dto, image, userId);

            if (updatedCombo == null)
                return NotFound("Cập nhật combo thất bại hoặc không tìm thấy combo.");

            return Ok(updatedCombo);
        }

        //==========CONTROLLER TO DELETE A COMBO==========
        [HttpDelete("Combo/{comboId}/delete-combo")]
        [Authorize(Roles= "Admin")]
        public async Task<IActionResult>DeleteCombo (string comboId)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // In ra màn hình console của server
            Console.WriteLine($"--- DEBUG: UserId nhận được từ Token là: {userId} ---");

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("--- WARNING: Không tìm thấy Sub Claim trong Token! ---");
            }

            var success = await _menuService.DeleteComboAsync(comboId, userId);
            return success ? Ok (new { Message = "Delete Combo Success" }) : NotFound (new { Message = "Combo Not Found" });
        }

    }
}
