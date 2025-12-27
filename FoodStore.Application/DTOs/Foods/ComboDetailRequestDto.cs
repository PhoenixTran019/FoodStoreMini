using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.DTOs.Foods
{
    public class ComboDetailRequestDto
    {
        public string? FoodID { get; set; }

        public int? Quantity { get; set; }
    }
}
