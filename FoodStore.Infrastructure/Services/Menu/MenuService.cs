using FoodStore.Application.Common.Helper;
using FoodStore.Application.DTOs.Foods;
using FoodStore.Application.Interface.Menu;
using FoodStore.Domain.Data;
using FoodStore.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Infrastructure.Services.Menu
{
    public class MenuService : IMenuService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContext;

        public MenuService(AppDbContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;
        }

        private string CurrentUserId => _httpContext.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                                    ?? _httpContext.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //==========SERVICE TO GET ALL FOODS==========
        public async Task<IEnumerable<FoodItemResponseDto>> GetAllFoodsAsync()
        {
            return await _context.FoodItems
                .Select(f => new FoodItemResponseDto
                {
                    FoodID = f.FoodId,
                    FoodName = f.FoodName,
                    Description = f.Description,
                    Price = f.Price,
                    ImageURL = f.ImageUrl,
                    CategoryID = f.CategoryId,
                    IsAvailable = f.IsAvailable
                }).ToListAsync();
        }

        //==========SERVICE TO CREATE A FOOD ITEM==========
        public async Task<string> CreateFoodAsync(FoodItemUpsertDto dto, IFormFile image)
        {
            var foodId = Uuidv7Generator.NewUuid7().ToString();

            // LOGIC: Nếu image null, gán tên file mặc định, ngược lại lưu file mới
            string imageName = (image == null)
                                ? "5978100.png"
                                : await FileHelper.SaveImageAsync(image);

            var food = new FoodItem
            {
                FoodId = foodId,
                FoodName = dto.FoodName,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryID,
                IsAvailable = true,
                ImageUrl = imageName
            };
            _context.FoodItems.Add(food);

            var log = new ActivityLog
            {
                LogId = Uuidv7Generator.NewUuid7().ToString(),
                UserId = CurrentUserId,
                Action = "Create Food Item",
                TagetTable = "FoodItems",
                TargetId = foodId,
                TargetName = dto.FoodName,
                TimeStamp = DateTime.UtcNow
            };
            _context.ActivityLogs.Add(log);

            await _context.SaveChangesAsync();
            return foodId;
        }

        //==========SERVICE TO UPDATE A FOOD ITEM==========
        public async Task<bool> UpdateFoodAsync(string FoodId, FoodItemUpsertDto dto, IFormFile image)
        {
            var food = await _context.FoodItems.FindAsync(FoodId);

            if (food == null)
                return false;

            if (image != null)
            {
                FileHelper.DeleteImage(food.ImageUrl);
                food.ImageUrl = await FileHelper.SaveImageAsync(image); //save new image
            }
            food.FoodName = dto.FoodName;
            food.Description = dto.Description;
            food.Price = dto.Price;
            food.CategoryId = dto.CategoryID;
            food.IsAvailable = dto.IsAvailable;

            _context.ActivityLogs.Add(new ActivityLog
            {
                LogId = Uuidv7Generator.NewUuid7().ToString(),
                UserId = CurrentUserId,
                Action = "Update Food Item",
                TagetTable = "FoodItems",
                TargetId = FoodId,
                TargetName = dto.FoodName,
                TimeStamp = DateTime.UtcNow
            });

            return await _context.SaveChangesAsync() > 0;
        }

        //==========SERVICE TO DELETE A FOOD ITEM==========
        public async Task<bool> DeleteFoodAsync(string FoodId)
        {
            var food = await _context.FoodItems.FindAsync(FoodId);

            if (food == null)
                return false;

            // Chỉ xóa file trên đĩa nếu nó KHÔNG PHẢI là ảnh mặc định
            if (food.ImageUrl != "5978100.png")
            {
                FileHelper.DeleteImage(food.ImageUrl);
            }

            _context.ActivityLogs.Add(new ActivityLog
            {
                LogId = Uuidv7Generator.NewUuid7().ToString(),
                UserId = CurrentUserId,
                Action = "Delete Food Item",
                TagetTable = "FoodItems",
                TargetId = FoodId,
                TargetName = food.FoodName,
                TimeStamp = DateTime.UtcNow
            });

            _context.FoodItems.Remove(food);
            return await _context.SaveChangesAsync() > 0;
        }

        //==========SERVICE TO GET ALL COMBOS==========
        public async Task<IEnumerable<ComboResponseDto>> GetAllCombosAsync()
        {
            return await _context.Combos
                .Include(c => c.ComboDetails)
                .Select(c => new ComboResponseDto
                {
                    ComboID = c.ComboId,
                    ComboName = c.ComboName,
                    Description = c.Description,
                    Price = c.Price,
                    ImageURL = c.ImageUrl,
                    IsAvailable = c.IsAvailable,
                    Items = c.ComboDetails.Select(d => new ComboDetailRequestDto
                    {
                        FoodID = d.FoodId,
                        Quantity = d.Quantity
                    }).ToList()

                }).ToListAsync();
        }

        //==========SERVICE TO CREATE A COMBO ITEM==========
        public async Task<string> CreateComboAsync(ComboUpsertDto dto, IFormFile image)
        {
            using var trans = await _context.Database.BeginTransactionAsync();

            try
            {
                var comboId = Uuidv7Generator.NewUuid7().ToString();
                // LOGIC: Nếu image null, gán tên file mặc định, ngược lại lưu file mới
                string imageName = (image == null)
                                    ? "5978100.png"
                                    : await FileHelper.SaveImageAsync(image);

                var combo = new Combo
                {
                    ComboId = comboId,
                    ComboName = dto.ComboName,
                    Description = dto.Description,
                    Price = dto.Price,
                    IsAvailable = true,
                    ImageUrl = imageName
                };
                _context.Combos.Add(combo);

                foreach (var item in dto.Items)
                {
                    _context.ComboDetails.Add(new ComboDetail
                    {
                        DetailId = Uuidv7Generator.NewUuid7().ToString(),
                        ComboId = comboId,
                        FoodId = item.FoodID,
                        Quantity = item.Quantity
                    });
                }

                //Write Log
                _context.ActivityLogs.Add(new ActivityLog
                {
                    LogId = Uuidv7Generator.NewUuid7().ToString(),
                    UserId = CurrentUserId,
                    Action = "Create Combo",
                    TagetTable = "Combos",
                    TargetId = comboId,
                    TargetName = dto.ComboName,
                    TimeStamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await trans.CommitAsync();
                return comboId;
            }
            catch (Exception)
            {
                await trans.RollbackAsync();
                throw;
            }

        }

        //==========SERVICE TO UPDATE A COMBO ITEM==========
        public async Task<bool> UpdateComboAsync(string comboId, ComboUpsertDto dto, IFormFile? image)
        {
            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var combo = await _context.Combos
                    .Include(c => c.ComboDetails)
                    .FirstOrDefaultAsync(c => c.ComboId == comboId);

                if (combo == null) return false;

                //Process image updates (Remove old images if they are not the default ones)
                if (image != null)
                {
                    if (image != null)
                    {
                        if (combo.ImageUrl != "5978100.png") FileHelper.DeleteImage(combo.ImageUrl);
                        combo.ImageUrl = await FileHelper.SaveImageAsync(image);
                    }
                }

                combo.ComboName = dto.ComboName;
                combo.Description = dto.Description;
                combo.Price = dto.Price;
                combo.IsAvailable = dto.IsAvailable;

                //Update combo food details: Delete old details and add new ones
                _context.ComboDetails.RemoveRange(combo.ComboDetails);
                foreach (var item in dto.Items)
                {
                    _context.ComboDetails.Add(new ComboDetail
                    {
                        DetailId = Uuidv7Generator.NewUuid7().ToString(),
                        ComboId = comboId,
                        FoodId = item.FoodID,
                        Quantity = item.Quantity
                    });
                }

                _context.ActivityLogs.Add(new ActivityLog
                {
                    LogId = Uuidv7Generator.NewUuid7().ToString(),
                    UserId = CurrentUserId,
                    Action = "Update Combo Infor",
                    TagetTable = "Combos",
                    TargetId = comboId,
                    TargetName = dto.ComboName,
                    TimeStamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await trans.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await trans.RollbackAsync();
                return false;
            }
        }

        //==========SERVICE TO DELETE A COMBO ITEM==========
        public async Task<bool> DeleteComboAsync  (string comboId)
        {
            var combo = await _context.Combos.FindAsync(comboId);
            if (combo == null) return false;

            // Chỉ xóa file trên đĩa nếu nó KHÔNG PHẢI là ảnh mặc định
            if (combo.ImageUrl != "5978100.png")
            {
                FileHelper.DeleteImage(combo.ImageUrl);
            }

            _context.ActivityLogs.Add(new ActivityLog
            {
                LogId = Uuidv7Generator.NewUuid7().ToString(),
                UserId = CurrentUserId,
                Action = "Delete Combo",
                TagetTable = "Combos",
                TargetId = comboId,
                TargetName = combo.ComboName,
                TimeStamp = DateTime.UtcNow
            });

            _context.Combos.Remove(combo);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
