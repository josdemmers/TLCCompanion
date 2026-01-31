namespace TLCCompanion.Models
{
    public class POIWorldmap
    {
        public string Name { get; set; } = string.Empty;
        public string Localized { get; set; } = string.Empty;
        public float PositionX { get; set; } = 0;
        public float PositionY { get; set; } = 0;
        public float PositionZ { get; set; } = 0;
        public string IconName { get; set; } = string.Empty;
    }
}
