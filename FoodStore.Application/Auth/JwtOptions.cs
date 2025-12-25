using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.Auth
{
    public class JwtOptions
    {
        public string Key { get; set; } = string.Empty;

        public string Issuer { get; set; } = "FoodStore";

        public string Audience { get; set; } = "FoodStoreClient";

        public int ExpiresInMinutes { get; set; } = 60;
    }
}
