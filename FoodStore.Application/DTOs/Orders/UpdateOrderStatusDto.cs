using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.DTOs.Orders
{
    public class UpdateOrderStatusDto
    {
        public string? OrderID { get; set; }

        public string? NewStatusID { get; set; }

        public string? ShipperID { get; set; }

        public string? StaffNote { get; set; }
    }
}
