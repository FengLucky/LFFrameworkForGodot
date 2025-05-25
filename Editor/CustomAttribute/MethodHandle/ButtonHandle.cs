using LF.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace LF.Editor
{
    public class ButtonHandle:CustomMethodAttributeHandleBase
    {
        public override VisualElement CreateMethodGUI()
        {
            if (Attribute is not ButtonAttribute attribute)
            {
                return null;
            }

            if (MethodInfo.GetParameters().Length > 0)
            {
                Debug.LogError($"{MethodInfo.Name} ButtonAttribute 不支持带参数方法");
                return null;
            }
            
            var name = string.IsNullOrWhiteSpace(attribute.Name) ? MethodInfo.Name : attribute.Name;
            var btn = new Button
            {
                text = name
            };
            btn.clicked += OnClick;
            return btn;
        }

        private void OnClick()
        {
            MethodInfo.Invoke(Object, null);
        }
    }
}