using System;
using System.Reflection;
using LF.Runtime;

namespace LF.Editor
{
    public static class CustomAttributeUtil
    {
        public static string GetClassName(Type type)
        {
            var nameAttribute = type.GetCustomAttribute<ClassNameAttribute>();
            if (nameAttribute == null)
            {
                return type.FullName;
            }

            return $"{nameAttribute.Name}_{type.Name}";
        }
    }
}