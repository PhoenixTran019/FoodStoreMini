using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.DTOs.Foods
{
    public class ComboResponseDto : ComboUpsertDto
    {
        public string? ComboID { get; set; }
        public string? ComboName { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? ImageURL { get; set; }
        public bool? IsAvailable { get; set; }

        public List<ComboDetailRequestDto> Items { get; set; }
    }
}
