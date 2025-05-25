using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using LF.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LF.Editor
{
    public partial class CustomPropertyAttributeDrawer
    {
        private static readonly Dictionary<Type,Type> HandleDict = new();
        static CustomPropertyAttributeDrawer()
        {
            RegisterHandle<LabelAttribute,LabelHandle>();
            RegisterHandle<OnValueChangedAttribute,OnValueChangedHandle>();
            RegisterHandle<MinValueAttribute,MinValueHandle>();
            RegisterHandle<MaxValueAttribute,MaxValueHandle>();
            RegisterHandle<TypeStringAttribute,TypeStringHandle>();
        }

        public static void RegisterHandle<T,TV>() where T : CustomPropertyAttributeBase where TV:CustomPropertyAttributeHandleBase
        {
            HandleDict[typeof(T)] = typeof(TV);
        }

        public static void HandleCustomAttribute(FieldInfo fieldInfo,VisualElement root,SerializedProperty property)
        {
            var info = fieldInfo;
            var serializedProperty = property.Copy();
            root.RegisterCallback(async (AttachToPanelEvent evt) =>
            {
                await Task.Yield();
                var attributes = GetCustomAttributes(fieldInfo);
                foreach (var attribute in attributes)
                {
                    if (HandleDict.TryGetValue(attribute.GetType(), out var type))
                    {
                        try
                        {
                            var handle = Activator.CreateInstance(type) as CustomPropertyAttributeHandleBase;
                            handle?.Init(root,info,serializedProperty,attribute);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }
                }
            });
        }
    }
}