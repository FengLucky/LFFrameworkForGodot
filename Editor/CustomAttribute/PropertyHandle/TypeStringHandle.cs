using System;
using System.Collections.Generic;
using System.Linq;
using LF.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace LF.Editor
{
    public class TypeStringHandle:CustomPropertyAttributeHandleBase
    {
        protected override void OnInit()
        {
            base.OnInit();
            
            if (Attribute is not TypeStringAttribute attribute)
            {
                return;
            }

            var text = Root.Q<TextField>();
            var textInput = text?.Children().FirstOrDefault(element => element is not Label);
            if (text == null || textInput == null || FieldInfo.FieldType != typeof(string))
            {
                Debug.LogError($"{nameof(TypeStringAttribute)}属性仅可用在 string 类型字段上：{Property.propertyPath}");
                return;
            }

            var data = new List<SearchElement.SearchItemData>();
            if (attribute.WithBase)
            {
                data.Add(new SearchElement.SearchItemData(attribute.BaseType,CustomAttributeUtil.GetClassName(attribute.BaseType)));
            }

            foreach (var type in attribute.BaseType.Assembly.GetTypes())
            {
                if (type.IsSubclassOf(attribute.BaseType))
                {
                    data.Add(new SearchElement.SearchItemData(type,CustomAttributeUtil.GetClassName(type)));
                }
            }
           
            textInput.RegisterCallback(new EventCallback<ClickEvent>(evt =>
            {
                SearchElement.OpenSearch(data,textInput, item =>
                {
                    text.value = (item.Value as Type)?.FullName;
                },false);
            }));
        }
    }
}