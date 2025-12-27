using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.DTOs.Foods
{
    //DTO Returned
    public class FoodItemResponseDto :FoodItemUpsertDto
    {
        public string? FoodID { get; set; }

        public string? ImageURL { get; set; }
    }
}
