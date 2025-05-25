using System;
using System.Reflection;
using LF.Runtime;
using UnityEditor.UIElements;
using UnityEngine;

namespace LF.Editor
{
    public class OnValueChangedHandle:CustomPropertyAttributeHandleBase
    {
        private MethodInfo _method;
        private readonly object[] _param = new object[1];
        private readonly object[] _arrayParam = new object[2];
        private readonly Type[] _paramType = new Type[1];
        private readonly Type[] _arrayParamType = new Type[2]{null,typeof(int)};
        protected override void OnInit()
        {
            base.OnInit();

           
            var attribute = Attribute as OnValueChangedAttribute;
            if (attribute == null)
            {
                return;
            }
            
            if (IsArrayElement)
            {
                _arrayParamType[0] = Property.boxedValue.GetType();
                _method = ParentObjectValue.GetType().GetMethod(attribute.MethodName,
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static, null, _arrayParamType, null);
            }
            else
            {
                _paramType[0] = Property.boxedValue.GetType();
                _method = ParentObjectValue.GetType().GetMethod(attribute.MethodName,
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static, null, _paramType, null);
            }
            
            if (_method != null)
            {
                Root.RegisterCallback<SerializedPropertyChangeEvent>(evt =>
                {
                    if (ParentObjectValue == null)
                    {
                        return;
                    }

                    if (IsArrayElement)
                    {
                        _arrayParam[0] = evt.changedProperty.boxedValue;
                        _arrayParam[1] = ArrayIndex;
                        _method.Invoke(ParentObjectValue,_arrayParam);
                    }
                    else
                    {
                        _param[0] = evt.changedProperty.boxedValue;
                        _method.Invoke(ParentObjectValue,_param);
                    }
                });
            }
            else
            {
                if (IsArrayElement)
                {
                    Debug.LogError($"{Property.propertyPath} 找不到 OnValueChanged 的回调方法，数组元素回调方法格式为 void CallBack({Property.type} value,int index)");
                }
                else
                {
                    Debug.LogError($"{Property.propertyPath} 找不到 OnValueChanged 的回调方法，回调方法格式为 void CallBack({Property.type} value)");
                }
            }
        }
        
    }
}