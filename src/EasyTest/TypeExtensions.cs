using System;
using System.Reflection;

namespace EasyTest
{
    internal static class TypeExtensions
    {
        public static bool HasAttribute<TAttribute>(this MemberInfo method) where TAttribute : Attribute
        {
            return method.GetCustomAttribute(typeof(TAttribute)) != null;
        }
    }
}