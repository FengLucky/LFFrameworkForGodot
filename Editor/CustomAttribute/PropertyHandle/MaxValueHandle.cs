using System;
using System.Reflection;
using LF.Runtime;
using UnityEngine;

namespace LF.Editor
{
    public class MaxValueHandle:MinValueHandle
    {
        protected override void OnInit()
        {
            if (Attribute is not MaxValueAttribute attribute)
            {
                return;
            }

            var minAttribute = FieldInfo.GetCustomAttribute<MinValueAttribute>();
            if (minAttribute != null && (minAttribute.IntValue > attribute.IntValue || minAttribute.FloatValue > attribute.FloatValue))
            {
                attribute.IntValue = minAttribute.IntValue;
                attribute.FloatValue = minAttribute.FloatValue;
                Debug.LogError($"{Property.name} MinValue 的值大于 MaxValue 的值，已将 MaxValue 修正为 MinValue");
            }
            base.OnInit();
        }

        protected override bool IsIntValue()
        {
            if (Attribute is MaxValueAttribute attribute)
            {
                return attribute.IsIntValue;
            }

            return false;
        }

        protected override long GetIntValue(long value)
        {
            if (Attribute is MaxValueAttribute attribute)
            {
                return Math.Min(value, attribute.IntValue);
            }

            return value;
        }

        protected override double GetDoubleValue(double value)
        {
            if (Attribute is MaxValueAttribute attribute)
            {
                return Math.Min(value, attribute.FloatValue);
            }

            return value;
        }
    }
}