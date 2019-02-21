using System;

namespace Blueberry.DataTools
{
    public class ContentExporterAttribute : Attribute
    {
        public string DisplayName { get; set; }

        public ContentExporterAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }

    public class AtlasExporterAttribute : ContentExporterAttribute
    {
        public AtlasExporterAttribute(string displayName) : base(displayName)
        {
        }
    }
}
