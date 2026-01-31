using System;
using System.Collections.Generic;
using System.Text;

namespace TLCCompanion.Parser.Models
{
    public class Indicator
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public IndicatorProperties Properties { get; set; } = new IndicatorProperties();
    }

    public class IndicatorProperties
    {
        public List<Icon> Icons { get; set; } = new List<Icon>();
    }

    public class Icon
    {
        public IconValue Value { get; set; } = new IconValue();
    }

    public class IconValue
    {
        public string ObjectName { get; set; } = string.Empty;
        public string ObjectPath { get; set; } = string.Empty;
    }
}
