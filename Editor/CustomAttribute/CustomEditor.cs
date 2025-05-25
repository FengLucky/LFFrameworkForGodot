using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace LF.Editor
{
    [CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(Object),true)]
    public class CustomEditor:UnityEditor.Editor
    {
        private bool _destroyed;
        private CancellationTokenSource _cts = new();
        public override VisualElement CreateInspectorGUI()
        {
            return CreateInspector(serializedObject,_cts.Token);
        }

        private void OnDestroy()
        {
            _cts.Cancel();
        }

        public static VisualElement CreateInspector(SerializedObject serializedObj,CancellationToken token = default)
        {
            var root = new VisualElement();
            var it = serializedObj.GetIterator();
            if (it.Next(true))
            {
                while (it.NextVisible(false))
                {
                    root.Add(new PropertyField(it.Copy()));
                }
            }
            
            root.RegisterCallback(async (AttachToPanelEvent evt) =>
            {
                await Task.Yield();
                if (token.IsCancellationRequested)
                {
                    return;
                }
                var allPropertyField = root.Query<PropertyField>();
                foreach (var field in allPropertyField.ToList())
                {
                    CreateChildMethodGUI(serializedObj,field);
                }
                CustomMethodAttributeDrawer.CreateMethodGUI(root,serializedObj.targetObject);
            });
            return root;
        }

        private static void CreateChildMethodGUI(SerializedObject serializedObj,PropertyField field)
        {
            if (string.IsNullOrWhiteSpace(field.bindingPath))
            {
                return;
            }

            var serialized = serializedObj.FindProperty(field.bindingPath);
            if (serialized == null)
            {
                return;
            }

            if (serialized.isArray)
            {
                return;
            }
                
            if (serialized.propertyType == SerializedPropertyType.Generic && 
                serialized.propertyType != SerializedPropertyType.String)
            {
                  CustomMethodAttributeDrawer.CreateMethodGUI(field,serialized.boxedValue);
            }
        }

        public static object GetParentValue(SerializedProperty property)
        {
            if (!string.IsNullOrWhiteSpace(property.propertyPath))
            {
                string[] pathComponents = property.propertyPath.Split('.');
                var lastComponent = pathComponents[^1];
                if (lastComponent.StartsWith("data[") && lastComponent.EndsWith("]"))
                {
                    if (pathComponents.Length > 3)
                    {
                        string parentPath = string.Join(".", pathComponents, 0, pathComponents.Length - 3);
                        return property.serializedObject.FindProperty(parentPath).boxedValue;
                    }
                    return property.serializedObject.targetObject;
                }
                if (pathComponents.Length > 1)
                {
                    string parentPath = string.Join(".", pathComponents, 0, pathComponents.Length - 1);
                    return property.serializedObject.FindProperty(parentPath).boxedValue;
                }
                return property.serializedObject.targetObject;
            }

            return null;
        }
    }
}