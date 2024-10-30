using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
 
namespace ShelterVault.Shared.Extensions
{
    public static class AttributeExtensions
    {
        public static T GetAttribute<T>(this Enum enumValue) where T : Attribute
        {
            Type enumType = enumValue.GetType();
            MemberInfo memberInfo = enumType.GetMember(enumValue.ToString()).FirstOrDefault();
            object attributes = memberInfo?.GetCustomAttributes(typeof(T), false).FirstOrDefault();
            return (T)attributes;
        }
    }
}
