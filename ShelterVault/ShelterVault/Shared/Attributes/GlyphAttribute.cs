using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
