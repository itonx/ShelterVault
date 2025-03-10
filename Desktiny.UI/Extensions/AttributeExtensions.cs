using System;
using System.Linq;
using System.Reflection;

namespace Desktiny.UI.Extensions
{
    public static class AttributeExtensions
    {
#nullable enable
        public static T? GetAttribute<T>(this Enum? enumValue) where T : Attribute
        {
            if (enumValue == null) return null;
            Type enumType = enumValue.GetType();
            MemberInfo? memberInfo = enumType.GetMember(enumValue.ToString()).FirstOrDefault();
            object? attributes = memberInfo?.GetCustomAttributes(typeof(T), false).FirstOrDefault();
            return (T?)attributes;
        }
    }
}
