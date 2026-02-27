
using System.Text.Json;

namespace TLCCompanion.Entities
{
    public class PlayerCoordinates
    {
        public double X { get; set; } = 0.0;
        public double Y { get; set; } = 0.0;
        public double Z { get; set; } = 0.0;

        public static PlayerCoordinates FromJsonString(string response)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<PlayerCoordinates>(response, options)
                   ?? new PlayerCoordinates();

        }
    }
}
