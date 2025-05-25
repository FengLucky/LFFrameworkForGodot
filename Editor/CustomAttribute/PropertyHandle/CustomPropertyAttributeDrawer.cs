using LF.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace LF.Editor
{
    [CustomPropertyDrawer(typeof(CustomPropertyAttributeBase),true)]
    public partial class CustomPropertyAttributeDrawer:PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (IsLastCustomAttribute(fieldInfo, attribute as CustomPropertyAttributeBase))
            {
                PropertyField field = new PropertyField(property);
                HandleCustomAttribute(fieldInfo,field,property);
                
                
                return field;
            }
            return new PropertyField(property);
        }
    }
}