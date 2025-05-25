using System.Reflection;
using LF.Runtime;
using UnityEngine.UIElements;

namespace LF.Editor
{
    public abstract class CustomMethodAttributeHandleBase
    {
        protected MethodInfo MethodInfo { get; private set; }
        protected object Object { get; private set; }
        protected CustomMethodAttributeBase Attribute { get; private set; }

        public void Init(MethodInfo info, object obj,CustomMethodAttributeBase attribute)
        {
            MethodInfo = info;
            Object = obj;
            Attribute = attribute;
            OnInit();
        }

        public abstract VisualElement CreateMethodGUI();
        protected virtual void OnInit()
        {
        }
    }
}