using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace LF.Editor
{
    [InitializeOnLoad]
    public class CustomToolbar
    {
        private static GUIStyle buttonStyle;

        private static GUIStyle ButtonStyle
        {
            get
            {
                if (buttonStyle == null)
                {
                    buttonStyle = new GUIStyle("Button")
                    {
                        fontSize = 15,
                        alignment = TextAnchor.MiddleCenter,
                        imagePosition = ImagePosition.ImageAbove,
                        fontStyle = FontStyle.Bold,
                    };
                }

                return buttonStyle;
            }
        }
        
        static CustomToolbar()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        }

        static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("开始游戏", ButtonStyle))
            {
                if (EditorBuildSettings.scenes.Length == 0)
                {
                    Debug.LogError("没有配置打包场景，无法跳转");
                    return;
                }
                SceneHelper.StartScene(EditorBuildSettings.scenes[0].path);
            }
        }
    }
}