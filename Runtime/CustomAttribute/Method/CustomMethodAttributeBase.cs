using System;
using System.Diagnostics;

namespace LF.Runtime
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class CustomMethodAttributeBase:Attribute
    {
        
    }
}