using LF.Runtime;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;

namespace LF.Editor
{
    [UnityEditor.CustomEditor(typeof(UIText), true), CanEditMultipleObjects]
    public class UITextEditor:TMP_EditorPanelUI
    {
        public override void OnInspectorGUI()
        {
            
            // Make sure Multi selection only includes TMP Text objects.
            if (IsMixSelectionTypes()) return;

            serializedObject.Update();

            DrawTextInput();

            DrawMainSettings();
            
            DrawUITextOptional();

            DrawExtraSettings();

            EditorGUILayout.Space();

            if (serializedObject.ApplyModifiedProperties() || m_HavePropertiesChanged)
            {
                m_TextComponent.havePropertiesChanged = true;
                m_HavePropertiesChanged = false;
            }
        }

        private void DrawUITextOptional()
        {
            var localizationId = serializedObject.FindProperty("localizationId");
            EditorGUILayout.PropertyField(localizationId,new GUIContent("文本Id"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoRefreshOnLanguageChanged"),new GUIContent("切换语言时自动刷新"));

            if (GUILayout.Button("刷新文本"))
            {
                (target as UIText)?.RefreshText();
            }
        }
    }
}