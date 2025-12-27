using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.Common.Helper
{
    public static class FileHelper
    {
        private const string FolderPath = @"D:\FoodStoreMini\FoodStore.Uploads\uploads";

        public static async Task<string> SaveImageAsync(IFormFile file)
        {
            if (file == null) return null;
            if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
            var fileName = $"{Uuidv7Generator.NewUuid7().ToString()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(FolderPath, fileName);
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
            return fileName;
        }

        public static void DeleteImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;
            var fullPath = Path.Combine(FolderPath, fileName);
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
    }
}
