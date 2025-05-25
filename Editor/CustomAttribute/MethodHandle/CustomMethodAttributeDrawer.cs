using System.Reflection;
using LF.Runtime;
using UnityEngine.UIElements;

namespace LF.Editor
{
    public partial class CustomMethodAttributeDrawer
    {
        public static void CreateMethodGUI(VisualElement root, object target)
        {
            var allMethods = target.GetType().GetRuntimeMethods();
            foreach (var method in allMethods)
            {
                var attributes = method.GetCustomAttributes<CustomMethodAttributeBase>();
                foreach (var attribute in attributes)
                {
                    HandleCustomAttribute(root, target,method,attribute);
                }
            }
        }
    }
}