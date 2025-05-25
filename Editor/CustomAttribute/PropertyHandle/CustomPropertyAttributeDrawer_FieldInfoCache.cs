using System.Collections.Generic;
using System.Reflection;
using LF.Runtime;

namespace LF.Editor
{
    public partial class CustomPropertyAttributeDrawer
    {
        private static readonly Dictionary<FieldInfo,List<CustomPropertyAttributeBase>> CustomAttributeDict = new();

        private static bool IsLastCustomAttribute(FieldInfo fieldInfo, CustomPropertyAttributeBase propertyAttribute)
        {
            var customAttributes = GetCustomAttributes(fieldInfo);
            if (customAttributes.IsNullOrEmpty())
            {
                return propertyAttribute.GetType() == customAttributes[^1].GetType();
            }
            return true;
        }

        private static List<CustomPropertyAttributeBase> GetCustomAttributes(FieldInfo fieldInfo)
        {
            if (!CustomAttributeDict.TryGetValue(fieldInfo, out var list))
            {
                var allAttribute = fieldInfo.GetCustomAttributes(typeof(CustomPropertyAttributeBase), true);
                list = new List<CustomPropertyAttributeBase>();
                if (!allAttribute.IsNullOrEmpty())
                {
                    foreach (var attr in allAttribute)
                    {
                        list.Add(attr as CustomPropertyAttributeBase);
                    }
                }
                CustomAttributeDict.Add(fieldInfo,list);
            }

            return list;
        }
    }
}