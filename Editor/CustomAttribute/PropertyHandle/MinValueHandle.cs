using System;
using LF.Runtime;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace LF.Editor
{
    public class MinValueHandle:CustomPropertyAttributeHandleBase
    {
        private IntegerField _intField;
        private LongField _longField;
        private FloatField _floatField;
        private DoubleField _doubleField;
        
        protected override void OnInit()
        {
            _intField = Root.Q<IntegerField>();
            _longField = Root.Q<LongField>();
            _floatField = Root.Q<FloatField>();
            _doubleField = Root.Q<DoubleField>();
            
            RefreshIntClamp();
            Root.RegisterCallback<SerializedPropertyChangeEvent>(evt =>
            {
                RefreshIntClamp();
            });
        }

        private void RefreshIntClamp()
        {
            if (IsIntValue())
            {
                if (_intField != null)
                {
                    var clampValue = (int)GetIntValue(_intField.value);
                    if (clampValue != _intField.value)
                    {
                        _intField.value = clampValue;
                    }
                }
                if (_longField != null)
                {
                    var clampValue = GetIntValue(_longField.value);
                    if (clampValue != _longField.value)
                    {
                        _longField.value = clampValue;
                    }
                }
                if (_floatField != null)
                {
                    var clampValue = (float)GetIntValue((long)_floatField.value);
                    if (Math.Abs(clampValue - _floatField.value) > 0.000001f)
                    {
                        _floatField.value = clampValue;
                    }
                }
                if (_doubleField != null)
                {
                    var clampValue = (double)GetIntValue((long)_doubleField.value);
                    if (Math.Abs(clampValue - _doubleField.value) > 0.000001f)
                    {
                        _doubleField.value = clampValue;
                    }
                }
            }
            else
            {
                if (_intField != null)
                {
                    var clampValue = (int)GetDoubleValue(_intField.value);
                    if (clampValue != _intField.value)
                    {
                        _intField.value = clampValue;
                    }
                }
                if (_longField != null)
                {
                    var clampValue = (long)GetDoubleValue(_longField.value);
                    if (clampValue != _longField.value)
                    {
                        _longField.value = clampValue;
                    }
                }
                if (_floatField != null)
                {
                    var clampValue = (float)GetDoubleValue(_floatField.value);
                    if (Math.Abs(clampValue - _floatField.value) > 0.000001f)
                    {
                        _floatField.value = clampValue;
                    }
                }
                if (_doubleField != null)
                {
                    var clampValue = GetDoubleValue(_doubleField.value);
                    if (Math.Abs(clampValue - _doubleField.value) > 0.000001f)
                    {
                        _doubleField.value = clampValue;
                    }
                }
            }
        }

        protected virtual long GetIntValue(long value)
        {
            if (Attribute is MinValueAttribute attribute)
            {
                return Math.Max(value, attribute.IntValue);
            }

            return value;
        }
        
        protected virtual double GetDoubleValue(double value)
        {
            if (Attribute is MinValueAttribute attribute)
            {
                return Math.Max(value, attribute.FloatValue);
            }

            return value;
        }

        protected virtual bool IsIntValue()
        {
            if (Attribute is MinValueAttribute attribute)
            {
                return attribute.IsIntValue;
            }

            return false;
        }
    }
}