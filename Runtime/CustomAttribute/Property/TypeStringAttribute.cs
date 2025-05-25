using System;
using System.Diagnostics;

namespace LF.Runtime
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class TypeStringAttribute:CustomPropertyAttributeBase
    {
        public readonly Type BaseType;
        public  readonly bool WithBase;
        public TypeStringAttribute(Type baseType,bool withBase = true)
        {
            BaseType = baseType;
            WithBase = withBase;
        }
    }
}