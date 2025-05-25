using System;
using System.Diagnostics;

namespace LF.Runtime
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class LabelAttribute:CustomPropertyAttributeBase
    {
        public readonly string Text;
        public LabelAttribute(string text)
        {
            Text = text;
        }
    }
}