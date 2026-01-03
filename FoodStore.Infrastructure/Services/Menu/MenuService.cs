using FoodStore.Application.Common.Helper;
using FoodStore.Application.DTOs.Foods;
using FoodStore.Application.Interface.Menu;
using FoodStore.Domain.Data;
using FoodStore.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
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

      

        private string GetFullImageUrl(string imageName)
        {
            if (_httpContext.HttpContext == null) return imageName;

            var request = _httpContext.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var name = string.IsNullOrEmpty(imageName) ? "5978100.png" : imageName;
            return $"{baseUrl}/uploads/{name}";
        }

        //==========SERVICE TO GET ALL FOODS==========
        public async Task<IEnumerable<FoodItemResponseDto>> GetAllFoodsAsync()
        {
            var foods = await _context.FoodItems.ToListAsync();

            // Lấy Base URL từ request hiện tại (VD: https://localhost:7001)
            var request = _httpContext.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            return foods.Select(f => new FoodItemResponseDto
            {
                FoodID = f.FoodId,
                FoodName = f.FoodName,
                Description = f.Description,
                Price = f.Price,
                // Gán link: nếu null thì lấy ảnh mặc định, ngược lại nối baseUrl
                ImageURL = string.IsNullOrEmpty(f.ImageUrl)
                    ? $"{baseUrl}/uploads/5978100.png"
                    : $"{baseUrl}/uploads/{f.ImageUrl}",
                CategoryID = f.CategoryId,
                IsAvailable = f.IsAvailable
            }).ToList();
        }

        //==========SERVICE TO GET FOOD DETAIL==========
        public async Task<FoodItemResponseDto> GetFoodItemDetailAsync(string foodId)
        {
            var food = await _context.FoodItems.FindAsync(foodId);
            if (food == null) return null;

            return new FoodItemResponseDto
            {
                FoodID = food.FoodId,
                FoodName = food.FoodName,
                Description = food.Description,
                Price = food.Price,
                ImageURL = GetFullImageUrl(food.ImageUrl), // Sử dụng hàm private static helper của bạn
                CategoryID = food.CategoryId,
                IsAvailable = food.IsAvailable
            };
        }

        //==========SERVICE TO CREATE A FOOD ITEM==========
        public async Task<string> CreateFoodAsync(FoodItemUpsertDto dto, IFormFile image, string userID)
        {

            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == dto.CategoryID);
            if (!categoryExists)
            {
                throw new Exception("Danh mục (Category) không tồn tại.");
            }



            var foodId = Uuidv7Generator.NewUuid7().ToString();

            // LOGIC: Nếu image null, gán tên file mặc định, ngược lại lưu file mới
            // SỬ DỤNG FILEHELPER NHƯ THIẾT KẾ
            string imageName = (image == null) ? "5978100.png" : await FileHelper.SaveImageAsync(image);

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
                UserId = userID,
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
        public async Task<bool> UpdateFoodAsync(string FoodId, FoodItemUpsertDto dto, IFormFile image, string userID)
        {
            var food = await _context.FoodItems.FindAsync(FoodId);

            if (food == null)
                return false;

            if (image != null)
            {
                // Gọi FileHelper để xử lý xóa/lưu theo logic động
                FileHelper.DeleteImage(food.ImageUrl);
                food.ImageUrl = await FileHelper.SaveImageAsync(image);
            }
            food.FoodName = dto.FoodName;
            food.Description = dto.Description;
            food.Price = dto.Price;
            food.CategoryId = dto.CategoryID;
            food.IsAvailable = dto.IsAvailable;

            _context.ActivityLogs.Add(new ActivityLog
            {
                LogId = Uuidv7Generator.NewUuid7().ToString(),
                UserId = userID,
                Action = "Update Food Item",
                TagetTable = "FoodItems",
                TargetId = FoodId,
                TargetName = dto.FoodName,
                TimeStamp = DateTime.UtcNow
            });

            return await _context.SaveChangesAsync() > 0;
        }

        //==========SERVICE TO DELETE A FOOD ITEM==========
        public async Task<bool> DeleteFoodAsync(string FoodId, string userID)
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
                UserId = userID,
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
            var combos = await _context.Combos
                .Include(c => c.ComboDetails)
                    .ThenInclude(d => d.Food) // Load thêm thông tin Food để lấy FoodName
                .ToListAsync();

            var request = _httpContext.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            return combos.Select(c => new ComboResponseDto
            {
                ComboID = c.ComboId,
                ComboName = c.ComboName,
                Description = c.Description,
                Price = c.Price,
                ImageURL = GetFullImageUrl(c.ImageUrl), // Dùng hàm helper cho gọn
                IsAvailable = c.IsAvailable,
                Items = c.ComboDetails.Select(d => new ComboDetailRequestDto
                {
                    FoodID = d.FoodId,
                    Quantity = d.Quantity,
                    FoodName = d.Food?.FoodName // Thêm thuộc tính này vào DTO nếu muốn FE hiện tên món
                }).ToList()
            }).ToList();
        }

        //==========SERVICE TO GET COMBO DETAIL==========
        public async Task<ComboResponseDto> GetComboDetailAsync(string comboId)
        {
            var combo = await _context.Combos
        .Include(c => c.ComboDetails)
            .ThenInclude(d => d.Food) // Sửa từ FoodItem thành Food cho khớp với Entity ComboDetail của bạn
        .FirstOrDefaultAsync(c => c.ComboId == comboId);

            if (combo == null) return null;

            var request = _httpContext.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            return new ComboResponseDto
            {
                ComboID = combo.ComboId,
                ComboName = combo.ComboName,
                Description = combo.Description,
                Price = combo.Price,
                ImageURL = GetFullImageUrl(combo.ImageUrl),
                IsAvailable = combo.IsAvailable,
                // Map danh sách món ăn có trong combo
                Items = combo.ComboDetails.Select(d => new ComboDetailRequestDto
                {
                    FoodID = d.FoodId,
                    Quantity = d.Quantity,
                    // Nếu bạn muốn lấy thêm tên món ở đây thì dùng d.Food?.FoodName
                    FoodName = d.Food?.FoodName
                }).ToList()
            };
        }

        //==========SERVICE TO CREATE A COMBO ITEM==========
        public async Task<string> CreateComboAsync(ComboUpsertDto dto, IFormFile image, string userID)
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

                // --- SỬA Ở ĐÂY: Giải mã chuỗi JSON Items ---
                if (!string.IsNullOrEmpty(dto.Items))
                {
                    var itemsList = JsonSerializer.Deserialize<List<ComboDetailRequestDto>>(dto.Items,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (itemsList != null)
                    {
                        foreach (var item in itemsList)
                        {
                            _context.ComboDetails.Add(new ComboDetail
                            {
                                DetailId = Uuidv7Generator.NewUuid7().ToString(),
                                ComboId = comboId,
                                FoodId = item.FoodID,
                                Quantity = item.Quantity
                            });
                        }
                    }
                }

                //Write Log
                _context.ActivityLogs.Add(new ActivityLog
                {
                    LogId = Uuidv7Generator.NewUuid7().ToString(),
                    UserId = userID,
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

        //==========Service to have add food to combo==========
        public async Task<bool> AddFoodToComboAsync(string comboId, ComboDetailRequestDto itemDto)
        {
            var combo = await _context.Combos.AnyAsync(c => c.ComboId == comboId);
            if (!combo) return false;

            // Kiểm tra xem món này đã có trong combo chưa, nếu có thì cộng dồn số lượng
            var existingDetail = await _context.ComboDetails
                .FirstOrDefaultAsync(d => d.ComboId == comboId && d.FoodId == itemDto.FoodID);

            if (existingDetail != null)
            {
                existingDetail.Quantity += itemDto.Quantity;
            }
            else
            {
                _context.ComboDetails.Add(new ComboDetail
                {
                    DetailId = Uuidv7Generator.NewUuid7().ToString(),
                    ComboId = comboId,
                    FoodId = itemDto.FoodID,
                    Quantity = itemDto.Quantity
                });
            }

            return await _context.SaveChangesAsync() > 0;
        }

        //==========SERVICE TO REMOVE A SINGLE ITEM FROM COMBO==========
        public async Task<bool> RemoveFoodFromComboAsync(string comboId, string foodId)
        {
            var detail = await _context.ComboDetails
                .FirstOrDefaultAsync(d => d.ComboId == comboId && d.FoodId == foodId);

            if (detail == null) return false;

            _context.ComboDetails.Remove(detail);
            return await _context.SaveChangesAsync() > 0;
        }

        //==========SERVICE TO UPDATE A COMBO ITEM==========
        public async Task<ComboResponseDto?> UpdateComboAsync(string comboId, ComboUpsertDto dto, IFormFile? image, string userID)
        {
            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var combo = await _context.Combos
                    .Include(c => c.ComboDetails)
                    .FirstOrDefaultAsync(c => c.ComboId == comboId);

                if (combo == null) return null;

                // Clean code trong UpdateComboAsync
                if (image != null)
                {
                    if (combo.ImageUrl != "5978100.png")
                        FileHelper.DeleteImage(combo.ImageUrl);

                    combo.ImageUrl = await FileHelper.SaveImageAsync(image);
                }

                combo.ComboName = dto.ComboName;
                combo.Description = dto.Description;
                combo.Price = dto.Price;
                combo.IsAvailable = dto.IsAvailable;

                // --- SỬA Ở ĐÂY: Giải mã chuỗi JSON Items để ghi đè ---
                if (!string.IsNullOrEmpty(dto.Items))
                {
                    var itemsList = JsonSerializer.Deserialize<List<ComboDetailRequestDto>>(dto.Items,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (itemsList != null)
                    {
                        // Xóa các món cũ đi để thay bằng danh sách mới từ JSON
                        _context.ComboDetails.RemoveRange(combo.ComboDetails);
                        foreach (var item in itemsList)
                        {
                            _context.ComboDetails.Add(new ComboDetail
                            {
                                DetailId = Uuidv7Generator.NewUuid7().ToString(),
                                ComboId = comboId,
                                FoodId = item.FoodID,
                                Quantity = item.Quantity
                            });
                        }
                    }
                }

                _context.ActivityLogs.Add(new ActivityLog
                {
                    LogId = Uuidv7Generator.NewUuid7().ToString(),
                    UserId = userID,
                    Action = "Update Combo Infor",
                    TagetTable = "Combos",
                    TargetId = comboId,
                    TargetName = dto.ComboName,
                    TimeStamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await trans.CommitAsync();
                return await GetComboDetailAsync(comboId);
            }
            catch (Exception)
            {
                await trans.RollbackAsync();
                return null;
            }
        }

        //==========SERVICE TO DELETE A COMBO ITEM==========
        public async Task<bool> DeleteComboAsync  (string comboId, string userID)
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
                UserId = userID,
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
