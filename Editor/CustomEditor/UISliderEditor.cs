using LF.Runtime;
using UnityEditor;
using UnityEditor.UI;

namespace LF.Editor
{
    [UnityEditor.CustomEditor(typeof(UISlider), true), CanEditMultipleObjects]
    public class UISliderEditor:SliderEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("text"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("intTextFormat"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("floatTextFormat"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}