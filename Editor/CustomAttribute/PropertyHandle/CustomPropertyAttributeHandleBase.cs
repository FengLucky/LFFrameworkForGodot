using System.Reflection;
using LF.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace LF.Editor
{
    public class CustomPropertyAttributeHandleBase
    {
        protected VisualElement Root { get; private set; }
        protected FieldInfo FieldInfo { get; private set; }
        protected SerializedProperty Property { get; private set; }
        protected object ParentObjectValue { get; private set; }
        protected int ArrayIndex { get; private set; } = -1;
        protected bool IsArrayElement { get; private set; } = false;
        protected CustomPropertyAttributeBase Attribute { get; private set; }

        public void Init(VisualElement root, FieldInfo info, SerializedProperty property,CustomPropertyAttributeBase propertyAttribute)
        {
            if (!property.InvokeMethod<bool>("get_isValid"))
            {
                return;
            }
            Root = root;
            FieldInfo = info;
            Property = property;
            Attribute = propertyAttribute;
            ParentObjectValue = CustomEditor.GetParentValue(property);
            if (!string.IsNullOrWhiteSpace(property.propertyPath))
            {
                string[] pathComponents = property.propertyPath.Split('.');
                var lastComponent = pathComponents[^1];
                if (lastComponent.StartsWith("data[") && lastComponent.EndsWith("]"))
                {
                    ArrayIndex = int.Parse(lastComponent.Substring(5, lastComponent.Length - 6));
                    IsArrayElement = true;
                }
            }

            OnInit();
        }

        protected virtual void OnInit()
        {
        }
    }
}