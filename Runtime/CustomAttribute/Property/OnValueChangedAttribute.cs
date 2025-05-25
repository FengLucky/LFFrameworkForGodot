using System.Diagnostics;

namespace LF.Runtime
{
    [Conditional("UNITY_EDITOR")]
    public class OnValueChangedAttribute:CustomPropertyAttributeBase
    {
        public readonly string MethodName;
        public OnValueChangedAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}