using System;
using System.Diagnostics;
using UnityEngine;

namespace LF.Runtime
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class CustomPropertyAttributeBase:PropertyAttribute
    {
        
    }
}