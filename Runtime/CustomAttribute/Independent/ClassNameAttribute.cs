using System;
using System.Diagnostics;

namespace LF.Runtime
{
    [AttributeUsage(AttributeTargets.Class)]
    [Conditional("UNITY_EDITOR")]
    public class ClassNameAttribute:Attribute
    {
        public readonly string Name;

        public ClassNameAttribute(string name)
        {
            Name = name;
        }
    }
}