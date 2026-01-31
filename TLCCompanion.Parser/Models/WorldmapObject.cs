using System;
using System.Collections.Generic;
using System.Text;

namespace TLCCompanion.Parser.Models
{
    public class WorldmapObject
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public WorldmapObjectProperties Properties { get; set; } = new WorldmapObjectProperties();
    }

    public class WorldmapObjectProperties
    {
        public OverrideIndicator OverrideIndicator { get; set; } = new OverrideIndicator();
        public Properties properties { get; set; } = new Properties();
        public RelativeLocation RelativeLocation { get; set; } = new RelativeLocation();
    }

    public class OverrideIndicator
    {
        public string ObjectName { get; set; } = string.Empty;
        public string ObjectPath { get; set; } = string.Empty;
    }

    public class Properties
    {
        public string Group { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DisplayName DisplayName { get; set; } = new DisplayName();
    }

    public class DisplayName
    {
        public string CultureInvariantString { get; set; } = string.Empty;
        public string SourceString { get; set; } = string.Empty;
        public string LocalizedString { get; set; } = string.Empty;
    }

    public class RelativeLocation
    {
        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;
        public float Z { get; set; } = 0;
    }
}
