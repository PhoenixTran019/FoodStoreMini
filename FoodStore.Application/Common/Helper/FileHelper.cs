using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FoodStore.Application.Common.Helper
{
    public static class FileHelper
    {
        // Sử dụng AppDomain.CurrentDomain.BaseDirectory để tìm đường dẫn thực thi
        // Sau đó lùi lại các cấp để tìm đến project Uploads của bạn
        private static string GetUploadPath()
        {
            // Lấy thư mục bin/Debug... của project hiện tại
            string currentPath = AppDomain.CurrentDomain.BaseDirectory;

            // Tìm thư mục gốc của giải pháp (Solution) và trỏ vào project Uploads của bạn
            // Cách này linh hoạt hơn là gán cứng ổ D:\
            var directory = new DirectoryInfo(currentPath);

            // Lùi lại cho đến khi tìm thấy thư mục chứa project Uploads
            // Cấu trúc dự kiến: Root/FoodStore.API và Root/FoodStore.Uploads/uploads
            string rootPath = directory.Parent.Parent.Parent.Parent.FullName;
            return Path.Combine(rootPath, "FoodStore.Uploads", "uploads");
        }

        private static readonly string FolderPath = GetUploadPath();

        public static async Task<string> SaveImageAsync(IFormFile file)
        {
            if (file == null) return null;

            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            var fileName = $"{Uuidv7Generator.NewUuid7().ToString()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(FolderPath, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }

        public static void DeleteImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || fileName == "5978100.png") return;

            var fullPath = Path.Combine(FolderPath, fileName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}