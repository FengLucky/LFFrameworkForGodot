using LF.Runtime;
using UnityEngine.UIElements;

namespace LF.Editor
{
    public class LabelHandle:CustomPropertyAttributeHandleBase
    {
        protected override void OnInit()
        {
            base.OnInit();
            if (Attribute is not LabelAttribute attribute)
            {
                return;
            }

            if (IsArrayElement)
            {
                var parent = Root.parent;
                while (parent != null)
                {
                    if (parent is Foldout foldout && Property.propertyPath.StartsWith(foldout.bindingPath+".Array.data"))
                    {
                        foldout.text = attribute.Text;
                        break;
                    }
                    parent = parent.parent;
                }
            }
            else
            {
                var label = Root.Q<Label>();
                if (label != null)
                {
                    label.text = attribute.Text;
                }
            }
        }
    }
}