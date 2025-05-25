using System.Diagnostics;

namespace LF.Runtime
{
    [Conditional("UNITY_EDITOR")]
    public class ButtonAttribute:CustomMethodAttributeBase
    {
        public readonly string Name;
        public ButtonAttribute(string name)
        {
            Name = name;
        }

        public ButtonAttribute()
        {
            
        }
    }
}