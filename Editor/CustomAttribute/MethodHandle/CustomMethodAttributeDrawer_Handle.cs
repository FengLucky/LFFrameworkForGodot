using System;
using System.Collections.Generic;
using System.Reflection;
using LF.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace LF.Editor
{
    public partial class CustomMethodAttributeDrawer
    {
        private static readonly Dictionary<Type,Type> HandleDict = new();
        static CustomMethodAttributeDrawer()
        {
            RegisterHandle<ButtonAttribute,ButtonHandle>();
        }

        public static void RegisterHandle<T,TV>() where T : CustomMethodAttributeBase where TV:CustomMethodAttributeHandleBase
        {
            HandleDict[typeof(T)] = typeof(TV);
        }

        private static void HandleCustomAttribute(VisualElement root, object target,MethodInfo info,CustomMethodAttributeBase attribute)
        {
            if (HandleDict.TryGetValue(attribute.GetType(), out var type))
            {
                try
                {
                    var handle = Activator.CreateInstance(type) as CustomMethodAttributeHandleBase;
                    handle?.Init(info, target, attribute);
                    root.Add(handle?.CreateMethodGUI());
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}