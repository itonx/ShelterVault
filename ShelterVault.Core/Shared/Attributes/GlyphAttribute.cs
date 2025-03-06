using System;

namespace ShelterVault.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class GlyphAttribute : Attribute
    {
        public string Icon { get; }

        public GlyphAttribute(string icon)
        {
            Icon = icon;
        }
    }
}
