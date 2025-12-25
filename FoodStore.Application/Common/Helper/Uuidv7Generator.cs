using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FoodStore.Application.Common.Helper
{
    public class Uuidv7Generator
    {
        public static Guid NewUuid7()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var bytes = new byte[16];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            //Set timestamp (First 48 bits)
            bytes[0] = (byte)((timestamp >> 40) & 0xff);
            bytes[1] = (byte)((timestamp >> 32) & 0xff);
            bytes[2] = (byte)((timestamp >> 24) & 0xff);
            bytes[3] = (byte)((timestamp >> 16) & 0xff);
            bytes[4] = (byte)((timestamp >> 8) & 0xff);
            bytes[5] = (byte)(timestamp & 0xff);

            //Set version to 7
            bytes[6] = (byte)((bytes[6] & 0xff) | 0x70);

            //Set variant to RFC 4122
            bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);

            return new Guid(bytes);
        }

    }
}
