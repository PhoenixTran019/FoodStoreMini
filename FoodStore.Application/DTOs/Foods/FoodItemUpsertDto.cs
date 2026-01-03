using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.DTOs.Foods
{
    //DTO For sigle Food Item
    public class FoodItemUpsertDto
    {
        public string? FoodID { get; set; }

        public string? FoodName { get; set; }

        public string? Description { get; set; }

        public decimal? Price { get; set; }

        public string? ImageURL{ get; set; }

        public bool? IsAvailable { get; set; }

        public string? CategoryID { get; set; }

    }
}
