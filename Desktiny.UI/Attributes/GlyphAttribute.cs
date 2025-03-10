using System;

namespace Desktiny.UI.Attributes
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
